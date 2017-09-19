﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;

namespace MasterPatientIndex.ProbabilisticMPI
{
    public class MatchEngine
    {
        private static ConfigurationEngine _mpiConfiguration;
        private readonly bool _mpiTraceEnabled = false;
        protected static readonly Logger Logger = new LoggerConfiguration().CreateLogger();
        protected static readonly Logger MPIMatchLogger = new LoggerConfiguration().CreateLogger(/*"MPIMatchLogger"*/);
        protected static readonly Logger MPINoMatchLogger = new LoggerConfiguration().CreateLogger(/*"MPINoMatchLogger"*/);

        public MatchEngine(ConfigurationEngine configurationEngine)
        {
            _mpiConfiguration = configurationEngine;
        }

        public List<MPIMatchRecord> FindMatches (List<MPIIdentifier> searchVector, IList<MasterPatientIndexItem> candidates)
        {
            if (_mpiTraceEnabled) MPIMatchLogger.Verbose("Compared {0} candidates to {1}", candidates.Count, Describe(searchVector));

            //setup parallelism 
            var maxDegreeOfParallelism = 1;
            ThreadPool.SetMinThreads(maxDegreeOfParallelism, maxDegreeOfParallelism);
            var options = new ParallelOptions{MaxDegreeOfParallelism = maxDegreeOfParallelism};

            //compare search vector against each candidate vector and calculate match score for each candidate
            var allMatchRecords = candidates.AsParallel()
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .WithDegreeOfParallelism(options.MaxDegreeOfParallelism)
                .Select(c => GetCandidateMatchRecord(searchVector, c))
                .ToList();
                           
            allMatchRecords = SetConfidenceLevels(allMatchRecords);

            //consider only medium and high confidence matches
            var matchResults = allMatchRecords.Where(m => m.MatchConfidenceLevel != MPIConfidenceLevelLookup.Low).ToList();


            if (matchResults.Any())
            {
                if (!_mpiTraceEnabled) return matchResults;

                var highMatches = allMatchRecords.Where(r => r.MatchConfidenceLevel != MPIConfidenceLevelLookup.High).ToList();
                MPIMatchLogger.Information("{0} High confidence matches:", highMatches.Count);
                foreach (var hm in highMatches)
                {
                    MPIMatchLogger.Information("TotalScore: {0}, IdentifierScores: {1}", hm.TotalMatchScore, Describe(hm.MatchVector));
                }
                var mediumMatches = allMatchRecords.Where(r => r.MatchConfidenceLevel != MPIConfidenceLevelLookup.Medium).ToList();
                MPIMatchLogger.Information("{0} Medium confidence matches:", mediumMatches.Count);
                foreach (var mm in mediumMatches)
                {
                    MPIMatchLogger.Information("TotalScore: {0}, IdentifierScores: {1}", mm.TotalMatchScore,Describe(mm.MatchVector));
                }
                return matchResults;

            }
            if (_mpiTraceEnabled) MPINoMatchLogger.Verbose(Describe(searchVector), searchVector);
            return matchResults;
        }

        private static MPIMatchRecord GetCandidateMatchRecord(IEnumerable<MPIIdentifier> searchVector, MasterPatientIndexItem candidate)
        {
            //get list of identifier values for candidate
            var candidateVector = MasterPatientIndexItem.ToVector(candidate);
         
            //compare every element search vector to corresponding element in candidate vector
            var candidateMatchScores = searchVector.Select(sve =>
                GetIdentifierScore(sve, candidateVector.FirstOrDefault(cve => cve.Identifier == sve.Identifier)))
                .ToList();
            return new MPIMatchRecord
            {
                MatchedAcuperaId = candidate.AcuperaId,
                MatchType = "Probabilistic",
                MatchVector = candidateMatchScores,
            };               
        }
        private static MPIIdentifier GetIdentifierScore (MPIIdentifier incoming, MPIIdentifier existing)
        {
            Contract.Requires(_mpiConfiguration != null && incoming != null && existing != null);
       
            if (IsNullEmptyOrUnknown(incoming.Value) || IsNullEmptyOrUnknown(existing.Value))
            { 
                //can't compare so return 0 (identifier has no bearing on match score)
                return new MPIIdentifier
                {
                    Identifier = existing.Identifier,
                    Score = 0,
                    Value = existing.Value,
                };
            }

            //get weights for this identifier
            var weightRecord = _mpiConfiguration.IdentifierMatchWeights.First(w => w.Key.Equals(incoming.Identifier)).Value;
            //default to no match
            decimal identifierScore;

            //use fuzzy matching to determine how similar search vector is to candidate vector
            var similarityScore = GetSimilarityScore(incoming.Identifier, incoming, existing);
            if (similarityScore == 0)
            {
                identifierScore = weightRecord.NonMatchWeight;
            }
            else if (similarityScore == 1)
            {
                identifierScore = weightRecord.MatchWeight;
            }
            else
            {
                //if strings are neither identical nor different, adjust match weight by degree of similarity (0 to 1)
                identifierScore = weightRecord.MatchWeight * similarityScore;
            }

            return new MPIIdentifier
            {
                Identifier = existing.Identifier,
                Value = existing.Value,
                Score = identifierScore
            };
        }

        private static bool IsNullEmptyOrUnknown(object identifierValue)
        {
            return (identifierValue == null ||
                    string.IsNullOrEmpty(identifierValue.ToString()) ||
                    identifierValue.ToString().Equals("Unknown", StringComparison.OrdinalIgnoreCase));
        }

        //Min: 0   Max: 1 
        private static decimal GetSimilarityScore(MPIIdentifierLookup identifier, MPIIdentifier incoming, MPIIdentifier existing)
        {           
            var incomingValue = incoming.Value;
            var existingValue = existing.Value;
            
            // if either vector elements have null values, treat as non match
            if (incomingValue == null || existingValue == null)
                return 0;

            switch (identifier)
            {
                case MPIIdentifierLookup.LastName:
                case MPIIdentifierLookup.FirstName:
                case MPIIdentifierLookup.SSN:
                case MPIIdentifierLookup.MRN:
                case MPIIdentifierLookup.UniversalId:
                case MPIIdentifierLookup.InsuranceId:
                case MPIIdentifierLookup.City:
                case MPIIdentifierLookup.State:
                case MPIIdentifierLookup.PostalCode:
                    return CompareStrings(incomingValue, existingValue);
                case MPIIdentifierLookup.DateOfBirth:
                    return CompareDates(incomingValue, existingValue);
                case MPIIdentifierLookup.Gender:
                    return CompareGenders(incomingValue, existingValue);
                   
            }
            return 0;
        }

        private static decimal CompareGenders(object incomingValue, object existingValue)
        {
            Contract.Requires(incomingValue != null & existingValue != null);
            var incomingGender = (GenderLookup) Enum.Parse(typeof (GenderLookup), incomingValue.ToString(), true);
            var existingGender = (GenderLookup) Enum.Parse(typeof (GenderLookup), existingValue.ToString(), true);
            return incomingGender == existingGender ? 1 : 0;
        }

        private static decimal CompareStrings(object incoming, object existing)
        {
            //if either value is empty, we can't determine match or non-match      
            var incomingStr = ((string) incoming);
            var existingStr = ((string) existing);
            if (string.IsNullOrEmpty(incomingStr) || string.IsNullOrEmpty(existingStr))
                return 0;
            //return Convert.ToDecimal(GetSimilarityFromDistance(incomingStr.ToUpper(), existingStr.ToUpper()));
           return Convert.ToDecimal(incomingStr.JaroDistance(existingStr));
        }
        
        private static decimal CompareDates(object incoming, object existing)
        {
            DateTime incomingDate;
            DateTime existingDate;

            if (!DateTime.TryParse(incoming.ToString(), out incomingDate) || !DateTime.TryParse(existing.ToString(), out existingDate))
                return 0;

            //TODO: check for transpositions in birth month and day
            //what other type of "fuzzy" checks can be done on dates?
            return DateTime.Compare(incomingDate, existingDate) == 0 ? 1 : 0;
        }

        private static List<MPIMatchRecord> SetConfidenceLevels(List<MPIMatchRecord> matchResults)
        {
            var updatedResults = matchResults;
            foreach (var matchRecord in updatedResults)
            {
                if (matchRecord.TotalMatchScore > ConfigurationEngine.HighConfidenceMatchThreshold)
                    matchRecord.MatchConfidenceLevel = MPIConfidenceLevelLookup.High;
                else if (matchRecord.TotalMatchScore > ConfigurationEngine.MediumConfidenceMatchThreshold)
                    matchRecord.MatchConfidenceLevel = MPIConfidenceLevelLookup.Medium;
                else matchRecord.MatchConfidenceLevel = MPIConfidenceLevelLookup.Low;
            }
            if (updatedResults.Count(r => r.MatchConfidenceLevel == MPIConfidenceLevelLookup.High) <= 1)
                return updatedResults;

            //if we get multiple matches with HIGH confidence,
            //make them all MEDIUM since we can't auto-merge in that case
            
            foreach (var mr in updatedResults.Where(r => r.MatchConfidenceLevel == MPIConfidenceLevelLookup.High))
            {
                mr.MatchConfidenceLevel = MPIConfidenceLevelLookup.Medium;
            }
            return updatedResults;
        }

#if(false)
        private static decimal GetSimilarityFromDistance(string incoming, string existing)
        {
            if ((incoming.Any(char.IsLetter) && !(existing.Any(char.IsLetter))) ||
                (existing.Any(char.IsLetter) && !(incoming.Any(char.IsLetter))))
                return 0;

            var distance = incoming.LevenshteinDistance(existing);
            if (distance == 0)
                return 1;

            var maxDistance = incoming.Length >= existing.Length ? incoming.Length : existing.Length;
            var normalizedDistance = (decimal)distance / maxDistance;
            var similarity = 1 - normalizedDistance;
            return similarity;
        }
#endif

        private static string Describe(IEnumerable<MPIIdentifier> vector)
        {
            return string.Empty;
            // TODO: return vector.Aggregate(string.Empty, (current, ve) => current + $"{ve.Identifier}, {ve.Value}, {ve.Score}");
        }

        public decimal TestJaroWinkler(string incoming, string existing)
        {
            return Convert.ToDecimal(incoming.JaroDistance(existing)); 
        }
        public decimal TestJaro(string incoming, string existing)
        {
            return Convert.ToDecimal(incoming.JaroDistance(existing));
        }

#if(false)
        public decimal TestLevenshtein(string incoming, string existing)
        {
            return Convert.ToDecimal(incoming.LevenshteinDistance(existing));
        }
        public decimal TestNormalizedLevenshtein(string incoming, string existing)
        {
            var distance = incoming.LevenshteinDistance(existing);
            if (distance == 0) return 1;
            var hammingDistance = incoming.HammingDistance(existing);
            var distanceNormalized = distance/hammingDistance;
            var similarity = 1 - distanceNormalized;
            return Convert.ToDecimal(similarity);
        }
#endif
    }

    public class MPIIdentifierWeight
    {
        public string Identifier { get; set; }
        public decimal MatchWeight { get; set; }
        public decimal NonMatchWeight { get; set; }
        public bool IsBlockingIdentifier  { get; set; } 
    } 
 
}

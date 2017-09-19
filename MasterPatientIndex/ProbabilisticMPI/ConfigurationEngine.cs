using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MasterPatientIndex.ProbabilisticMPI
{
    public class ConfigurationEngine 
    {
        //TODO: make these parameters configurable
        public static readonly decimal HighConfidenceMatchThreshold = (decimal) 5.15;
        public static readonly decimal MediumConfidenceMatchThreshold = (decimal) 4.0;
        public static readonly int StringKeyLength = 3;

        private const string MPIResourcesNamespace = "MasterPatientIndex.Resources";
        private const string IdentifierMatchWeightsFile = "IdentifierMatchWeights.csv";

        private readonly Dictionary<MPIIdentifierLookup, MPIIdentifierWeight> IdentifierMatchWeights =
            new Dictionary<MPIIdentifierLookup, MPIIdentifierWeight>();
        private readonly List<KeyValuePair<MPIIdentifierLookup, MPIIdentifierWeight>> BlockingIdentifiers;

        public ConfigurationEngine()
        {
            LoadIdentifierMatchWeights();
            BlockingIdentifiers = IdentifierMatchWeights.Where(w => w.Value.IsBlockingIdentifier).ToList();
        }

        private void LoadIdentifierMatchWeights()
        {
            
        }

        private void LoadIdentifierMatchWeightsReal()
        {
            //http://en.wikipedia.org/wiki/Record_linkage
            //Formula used to calculate match/non-match weights for identifiers:  (example: birth month)
            //Outcome   Proportion of links  Proportion of non-links    Frequency ratio         Weight
            //Match     m = 0.95             u ≈ 0.083                  m/u ≈ 11.4              ln(m/u)/ln(2) ≈ 3.51 
            //Non-match 1−m = 0.05          1-u ≈ 0.917                 (1-m)/(1-u) ≈ 0.0545    ln((1-m)/(1-u))/ln(2) ≈ -4.20 

            var manifestResourceStream = GetEmbeddedFileStreamFromNamespace(IdentifierMatchWeightsFile, MPIResourcesNamespace);

            using (var sr = new StreamReader(manifestResourceStream))
            {
                //skip header
                sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    var contents = sr.ReadLine();
                    if (string.IsNullOrEmpty(contents)) continue;
                    contents = contents.Trim();
                    var row = contents.Split(',');
                    if (row.Length == 0) continue;

                    var identifierName = row[0].Trim();
                    var outcomeType = row[1].Trim();

                    /*below columns are part of file and used in calculation
                     * of final weight but not needed in the application
                    var matchProbability = row[2].Trim();
                    var nonMatchProbability = row[3].Trim();
                    var frequency = row[4].Trim();
                     */
                    var weight = row[5].Trim();
                    var isBlocking = row[6].Trim();

                    MPIIdentifierLookup lookupResult;
                    var identifierKey = Enum.TryParse(identifierName, true, out lookupResult) ? lookupResult : MPIIdentifierLookup.Unsupported;

                    MPIIdentifierWeight matchWeightRecord;
                    if (!IdentifierMatchWeights.ContainsKey(identifierKey))
                    {
                        matchWeightRecord = new MPIIdentifierWeight
                        {
                            Identifier = identifierName,
                        };
                        IdentifierMatchWeights.Add(identifierKey, matchWeightRecord);
                    }
                    else
                    {
                        IdentifierMatchWeights.TryGetValue(identifierKey, out matchWeightRecord);
                    }
                    if (matchWeightRecord == null) continue;

                    if (outcomeType.Equals("Match", StringComparison.OrdinalIgnoreCase))
                        matchWeightRecord.MatchWeight = Convert.ToDecimal(weight);
                    else if (outcomeType.Equals("NonMatch", StringComparison.OrdinalIgnoreCase))
                        matchWeightRecord.NonMatchWeight = Convert.ToDecimal(weight);
                    matchWeightRecord.IsBlockingIdentifier = isBlocking.Equals("Y",
                        StringComparison.OrdinalIgnoreCase);
                }
            }
        }
        private static Stream GetEmbeddedFileStreamFromNamespace(string mappingFileName, string mappingFileNamespace)
        {
            var manifestResourceStream =
                Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream(string.Format("{0}.{1}", mappingFileNamespace, mappingFileName));

            if (manifestResourceStream == null)
            {
                throw new FileNotFoundException(mappingFileName + " file not found");
            }
            return manifestResourceStream;
        }
    }
}

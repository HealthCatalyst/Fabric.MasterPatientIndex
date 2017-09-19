using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterPatientIndex.ProbabilisticMPI
{
    public abstract class ProbabilisticMpiCacheBase
    {
        private readonly MatchEngine _matchEngine;
        private readonly ConfigurationEngine _configurationEngine;
        
        protected ProbabilisticMpiCacheBase()
        {
            _configurationEngine = new ConfigurationEngine();
            _matchEngine = new MatchEngine(_configurationEngine);
        }

        
        public IList<MPIMatchRecord> GetProbabilisticMatches (List<MPIIdentifier> searchVector)
        {
            //get subset of patients to match against 
            var profileCandidates = GetCandidateBlock(searchVector);
            
            return profileCandidates.Any() 
                ? _matchEngine.FindMatches(searchVector, profileCandidates) 
                : new List<MPIMatchRecord>();
        }
     
        public IList<MasterPatientIndexItem> GetCandidateBlock(List<MPIIdentifier> searchVector)
        {
            //TODO: use redis intersect functionality to combine blocking identifiers? 
            //do we even want to intersect the blocking criteria?  may make more sense
            //to search through 1 block and if no match, search through another block.
            //intersecting the blocking criteria might be too restrictive
            //for now, assume blocking on lastname only
            
            var nameIdentifier = searchVector.FirstOrDefault(v => v.Identifier == MPIIdentifierLookup.LastName);
            var lastName = nameIdentifier != null ? nameIdentifier.Value.ToString() : string.Empty;

            //if lastname in search vector is shorter than key, use all characters 
            var searchKey  = lastName.Length < ConfigurationEngine.StringKeyLength ? lastName : lastName.Substring(0, ConfigurationEngine.StringKeyLength);
            //var candidateIds = LookupPatientsByPartialName(searchKey).ToList();

            //return GetMasterPatientIndexRecordsForListOfPatients(candidateIds).ToList();

            return null;
        }

    }
}
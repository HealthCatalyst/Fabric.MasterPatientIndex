using System.Collections.Generic;
using System.Linq;
using MasterPatientIndex.Structures;

namespace MasterPatientIndex.ProbabilisticMPI
{
    public abstract class ProbabilisticMpiCacheBase
    {
        private readonly MatchEngine _matchEngine;
        private readonly ConfigurationEngine _configurationEngine;
        private readonly IPatientStore _patientStore;

        protected ProbabilisticMpiCacheBase(IPatientStore store)
        {
            _configurationEngine = new ConfigurationEngine();
            _matchEngine = new MatchEngine(_configurationEngine);
            _patientStore = store;
        }

        
        public IList<MPIMatchRecord> GetProbabilisticMatches (SearchVector searchVector)
        {
            //get subset of patients to match against 
            var profileCandidates = GetCandidateBlock(searchVector);
            
            return profileCandidates.Any() 
                ? _matchEngine.FindMatches(searchVector, profileCandidates) 
                : new List<MPIMatchRecord>();
        }
     
        public IList<SearchVector> GetCandidateBlock(SearchVector searchVector)
        {
            //TODO: use redis intersect functionality to combine blocking identifiers? 
            //do we even want to intersect the blocking criteria?  may make more sense
            //to search through 1 block and if no match, search through another block.
            //intersecting the blocking criteria might be too restrictive
            //for now, assume blocking on lastname only
            
            var nameIdentifier = searchVector.GetBlockCandidate();
            var lastName = nameIdentifier != null ? nameIdentifier.Value.ToString() : string.Empty;

            //if lastname in search vector is shorter than key, use all characters 
            var searchKey  = lastName.Length < ConfigurationEngine.StringKeyLength ? lastName : lastName.Substring(0, ConfigurationEngine.StringKeyLength);
            var candidateIds = _patientStore.LookupPatientsByPartialName(searchKey).ToList();

            return _patientStore.GetMasterPatientIndexRecordsForListOfPatients(candidateIds).ToList();

        }

    }
}
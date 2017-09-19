using System.Collections.Generic;
using System.Linq;

namespace MasterPatientIndex.Structures
{
    public class MPIMatchRecord
    {
        public int MatchedAcuperaId { get; set; }
        public string MatchType { get; set; }  //Exact or Probablistic

        public decimal TotalMatchScore
        {
            get { return MatchVector.Sum(ve => ve.Score); }
        }

        public MPIConfidenceLevelLookup MatchConfidenceLevel  { get; set; }

        public List<MPIIdentifier> MatchVector { get; set; }
    }
}
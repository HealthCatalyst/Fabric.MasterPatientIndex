namespace MasterPatientIndex.Structures
{
    public class MPIIdentifier
    {
        public string IdentifierName { get; set; }
        public MatchType MatchType { get; set; }

        public object Value  { get; set; }
        public decimal Score  { get; set; }

        public bool IsBlockCandidate { get; set; }

    }
}
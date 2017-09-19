using System.Collections.Generic;

namespace MasterPatientIndex.ProbabilisticMPI
{
    public interface IMpiConfiguration
    {
        decimal HighConfidenceMatchThreshold { get; }
        decimal MediumConfidenceMatchThreshold { get; }
        int StringKeyLength { get; }
        Dictionary<string, MPIIdentifierWeight> IdentifierMatchWeights { get; }


    }
}
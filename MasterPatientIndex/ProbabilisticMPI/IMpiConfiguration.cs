using System.Collections.Generic;

namespace MasterPatientIndex.ProbabilisticMPI
{
    public interface IMpiConfiguration
    {
        double HighConfidenceMatchThreshold { get; }
        double MediumConfidenceMatchThreshold { get; }
        int StringKeyLength { get; }
        Dictionary<string, MPIIdentifierWeight> IdentifierMatchWeights { get; }


    }
}
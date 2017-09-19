using System;
using System.Collections.Generic;
using System.Linq;
using MasterPatientIndex.Structures;

namespace MasterPatientIndex.ProbabilisticMPI
{
    public class SearchVector
    {
        public List<MPIIdentifier> Identifiers { get; set; }

        public MPIIdentifier GetIdentifierByName(string identifierName)
        {
            return Identifiers.FirstOrDefault(v =>
                v.IdentifierName.Equals(identifierName, StringComparison.OrdinalIgnoreCase));
        }

        public MPIIdentifier GetBlockCandidate()
        {
            return Identifiers.FirstOrDefault(v => v.IsBlockCandidate);
        }
    }
}
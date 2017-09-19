using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterPatientIndex.ProbabilisticMPI
{
    public class SearchVector
    {
        public List<MPIIdentifier> Identifiers { get; set; }

        public MPIIdentifier GetIdentifierByName(string identifierName)
        {
            return Identifiers.FirstOrDefault(v =>
                v.IdentifierType.ToString().Equals(identifierName, StringComparison.OrdinalIgnoreCase));
        }

        public MPIIdentifier GetIdentifierByType(MPIIdentifierLookup identifierType)
        {
            return Identifiers.FirstOrDefault(v => v.IdentifierType == identifierType);
        }
    }
}
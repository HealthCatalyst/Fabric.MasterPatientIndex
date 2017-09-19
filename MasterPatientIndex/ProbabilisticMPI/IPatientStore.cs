using System.Collections.Generic;

namespace MasterPatientIndex.ProbabilisticMPI
{
    public interface IPatientStore
    {
        IList<string> LookupPatientsByPartialName(string searchKey);
        IList<SearchVector> GetMasterPatientIndexRecordsForListOfPatients(List<string> candidateIds);
    }
}
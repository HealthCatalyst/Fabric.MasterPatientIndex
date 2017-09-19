using System;
using System.Collections.Generic;
using MasterPatientIndex;
using MasterPatientIndex.ProbabilisticMPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MasterPatientIndexTest
{
    [TestClass]
    public class ProbabilisticMpiTester
    {
        [TestMethod]
        public void TestSimpleProbabilisticMpi()
        {
            var mockPatientStore = new MockPatientStore();
            var probabilisticMpiCache = new ProbabilisticMpiCache(mockPatientStore);
            var mpiMatchRecords = probabilisticMpiCache.GetProbabilisticMatches(new SearchVector { });
        }
    }

    public class MockPatientStore : IPatientStore
    {
        public IList<string> LookupPatientsByPartialName(string searchKey)
        {
            throw new NotImplementedException();
        }

        public IList<SearchVector> GetMasterPatientIndexRecordsForListOfPatients(List<string> candidateIds)
        {
            throw new NotImplementedException();
        }
    }
}

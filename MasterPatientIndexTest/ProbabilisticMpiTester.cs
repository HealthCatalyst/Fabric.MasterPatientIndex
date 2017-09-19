using System;
using System.Collections.Generic;
using System.Linq;
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
            mockPatientStore.Add(new MockPatient
            {
                Key="1",
                LastName="Jones",
                FirstName="Jim"
            });
            mockPatientStore.Add(new MockPatient
            {
                Key = "2",
                LastName = "Jones",
                FirstName = "Mark"
            });

            var probabilisticMpiCache = new ProbabilisticMpiCache(mockPatientStore);

            var testPatient = new MockPatient
            {
                LastName = "Jones",
                FirstName = "Mark"
            };
            var mpiMatchRecords = probabilisticMpiCache.GetProbabilisticMatches(testPatient.ToSearchVector());

            Assert.IsNotNull(mpiMatchRecords);
            Assert.AreEqual(1, mpiMatchRecords.Count);
        }
    }

    public class MockPatientStore : IPatientStore
    {
        private readonly IList<MockPatient> _patients = new List<MockPatient>();

        public void Add(MockPatient patient)
        {
            _patients.Add(patient);
        }

        public IList<string> LookupPatientsByPartialName(string searchKey)
        {
            return _patients.Where(prop => prop.LastName.StartsWith(searchKey)).Select(p => p.Key).ToList();
        }

        public IList<SearchVector> GetMasterPatientIndexRecordsForListOfPatients(List<string> keys)
        {
            return _patients.Where(p => keys.Contains(p.Key)).Select(p=> p.ToSearchVector()).ToList();
        }
    }

    public class MockPatient
    {
        public string Key { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }

        public SearchVector ToSearchVector()
        {
            return new SearchVector
            {
                Identifiers = new List<MasterPatientIndex.Structures.MPIIdentifier>
                {
                    new MasterPatientIndex.Structures.MPIIdentifier
                    {
                        IdentifierName = "Key",
                        Value = Key
                    },
                    new MasterPatientIndex.Structures.MPIIdentifier
                    {
                        IdentifierName = "LastName",
                        IsBlockCandidate = true,
                        Value = LastName
                    },
                    new MasterPatientIndex.Structures.MPIIdentifier
                    {
                        IdentifierName = "FirstName",
                        Value = FirstName
                    },
                }
            };
        }
    }
}

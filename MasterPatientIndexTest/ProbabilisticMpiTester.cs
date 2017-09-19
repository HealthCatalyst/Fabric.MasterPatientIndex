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

            var mpiConfiguration = CreateMpiConfiguration();

            var probabilisticMpiCache = new ProbabilisticMpiCache(mockPatientStore, mpiConfiguration);

            var testPatient = new MockPatient
            {
                LastName = "Jones",
                FirstName = "Mark"
            };
            var mpiMatchRecords = probabilisticMpiCache.GetProbabilisticMatches(testPatient.ToSearchVector());

            Assert.IsNotNull(mpiMatchRecords);
            Assert.AreEqual(1, mpiMatchRecords.Count);

            // add another match
            mockPatientStore.Add(new MockPatient
            {
                Key = "3",
                LastName = "Jones",
                FirstName = "Marm"
            });

            mpiMatchRecords = probabilisticMpiCache.GetProbabilisticMatches(testPatient.ToSearchVector());

            Assert.IsNotNull(mpiMatchRecords);
            Assert.AreEqual(2, mpiMatchRecords.Count);

        }

        private static MpiConfiguration CreateMpiConfiguration()
        {
            var mpiConfiguration = new MpiConfiguration
            {
                HighConfidenceMatchThreshold = (decimal) 1.00,
                MediumConfidenceMatchThreshold = (decimal) 0.50,
                StringKeyLength = 3,
            };

            mpiConfiguration.IdentifierMatchWeights.Add("LastName", new MPIIdentifierWeight
            {
                Identifier = "LastName",
                MatchWeight = 1,
                NonMatchWeight = -1
            });

            mpiConfiguration.IdentifierMatchWeights.Add("FirstName", new MPIIdentifierWeight
            {
                Identifier = "FirstName",
                MatchWeight = 1,
                NonMatchWeight = -1
            });
            return mpiConfiguration;
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

    public class MpiConfiguration : IMpiConfiguration
    {
        public decimal HighConfidenceMatchThreshold { get; set; }
        public decimal MediumConfidenceMatchThreshold { get; set; }
        public int StringKeyLength { get; set; }
        public Dictionary<string, MPIIdentifierWeight> IdentifierMatchWeights => new Dictionary<string, MPIIdentifierWeight>();
    }

}

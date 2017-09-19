using System;
using System.Collections.Generic;
using System.Linq;
using MasterPatientIndex.ProbabilisticMPI;

namespace MasterPatientIndex
{
    public class MasterPatientIndexItem : IMasterPatientIndexItem, IComparable
    {
        public string SSN { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public DateTime? DOB { get; set; }
        public int AcuperaId { get; set; }
        public string SourceMedicalRecordId { get; set; }
        public string UniversalId { get; set; }
        public string InsuranceId { get; set; }
        public string FacilityId { get; set; }
        public GenderLookup Gender { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public DateTime? LastUpdatedDateTimeUtc { get; set; }
        public string Database { get; set; }
        public int NumberOfActions { get; set; }
        public bool IsEnrolled { get; set; }
        public bool ExcludeFromEnrollment { get; set; }
        public decimal RiskScore { get; set; }
        public bool IsCareManagementCandidate { get; set; }
        public int? CareCoordinatorUserId { get; set; }
        public DateTime? StatusLastUpdated { get; set; }
        public DateTime? NextAppointmentDateTimeUtc { get; set; }
		public bool IsWorkflowRunning { get; set; }
		public DateTime? LastWorkflowRunDate { get; set; }
        public string ActiveInsuranceCompanies { get; set; }
        public List<string> ActiveInsuranceCompanyList { get; set; }

        public int CompareTo(object otherObject)
	    {
	        if (!(otherObject is MasterPatientIndexItem otherPatientIndexItem)) return 1;

			if (!ValuesAreEqual(SSN, otherPatientIndexItem.SSN) ||
				!ValuesAreEqual(SourceMedicalRecordId, otherPatientIndexItem.SourceMedicalRecordId) ||
				!ValuesAreEqual(FirstName, otherPatientIndexItem.FirstName) ||
				!ValuesAreEqual(LastName, otherPatientIndexItem.LastName) ||
				!ValuesAreEqual(DOB, otherPatientIndexItem.DOB))
		    {
				return 1;
		    }

		    return 0;
	    }

        public static Func<MasterPatientIndexItem, List<MPIIdentifier>> ToVector = delegate(MasterPatientIndexItem item)
        {
            var vector = new List<MPIIdentifier>
            {
                new MPIIdentifier {Identifier = MPIIdentifierLookup.UniversalId, Value = item.UniversalId},
                new MPIIdentifier {Identifier = MPIIdentifierLookup.SSN, Value = item.SSN},
                new MPIIdentifier {Identifier = MPIIdentifierLookup.MRN, Value = item.SourceMedicalRecordId},
                new MPIIdentifier {Identifier = MPIIdentifierLookup.InsuranceId, Value = item.InsuranceId},
                //new MPIIdentifier {Identifier = MPIIdentifierLookup.FacilityId, Value = item.FacilityId},
                new MPIIdentifier {Identifier = MPIIdentifierLookup.LastName, Value = item.LastName},
                new MPIIdentifier {Identifier = MPIIdentifierLookup.FirstName, Value = item.FirstName},
                new MPIIdentifier {Identifier = MPIIdentifierLookup.Gender, Value = item.Gender.ToString()},
                new MPIIdentifier {Identifier = MPIIdentifierLookup.DateOfBirth, Value = item.DOB},
                new MPIIdentifier {Identifier = MPIIdentifierLookup.City, Value = item.City},
                new MPIIdentifier {Identifier = MPIIdentifierLookup.State, Value = item.State},
                new MPIIdentifier {Identifier = MPIIdentifierLookup.PostalCode, Value = item.PostalCode},
            };
            return vector;
        };

	    private bool ValuesAreEqual(string value1, string value2)
	    {
		    if (value1 == null && value2 == null) return true;
		    if (value1 == null || value2 == null) return false; //i.e. if one value is null and the other one is not
		    return (value1.TrimAndLower() == value2.TrimAndLower());
	    }

		private bool ValuesAreEqual(DateTime? value1, DateTime? value2)
		{
			if (!value1.HasValue && !value2.HasValue) return true;
			if (!value1.HasValue || !value2.HasValue) return false; //i.e. if one value is null and the other one is not
			return (value1.Value.Date == value2.Value.Date);
		}
    }
    
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

    public class MPIIdentifier
    {
        public MPIIdentifierLookup Identifier { get; set; }
        public object Value  { get; set; }
        public decimal Score  { get; set; }

    }

    public enum MPIConfidenceLevelLookup
    {
        Low,
        Medium,
        High
    }

    public enum MPIIdentifierLookup
    {
        UniversalId,
        SSN,
        MRN,
        InsuranceId,
        LastName,
        FirstName,
        Gender,
        DateOfBirth,
        City,
        State,
        PostalCode,
        Unsupported
    }

    public enum GenderLookup
    {
        Male,
        Female
    }
}

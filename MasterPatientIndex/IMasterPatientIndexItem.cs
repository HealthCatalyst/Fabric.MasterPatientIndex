using System;

namespace MasterPatientIndex
{
    public interface IMasterPatientIndexItem
    {
        string SSN { get; set; }

        string FirstName { get; set; }

        string LastName { get; set; }

        DateTime? DOB { get; set; }
        int AcuperaId { get; set; }
        string SourceMedicalRecordId { get; set; }

        string UniversalId { get; set; }
        string InsuranceId { get; set; }
        string FacilityId { get; set; }        
        GenderLookup Gender { get; set; }
        string PostalCode { get; set; }

        DateTime? LastUpdatedDateTimeUtc { get; set; }
        string Database { get; set; }
        int NumberOfActions { get; set; }
        bool IsEnrolled { get; set; }
        bool ExcludeFromEnrollment { get; set; }
        decimal RiskScore { get; set; }
        bool IsCareManagementCandidate { get; set; }

        DateTime? StatusLastUpdated { get; set; }

    }
}
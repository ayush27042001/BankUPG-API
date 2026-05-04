namespace BankUPG.SharedKernal.Responses
{
    public class PanVerificationResult
    {
        public bool IsValid { get; set; }
        public string? PanNumber { get; set; }
        public string? Name { get; set; }
        public string? NameOnPanCard { get; set; }
        public string? Type { get; set; }
        public int? ReferenceId { get; set; }
        public string? Message { get; set; }
        public string? PanStatus { get; set; }
        public string? AadhaarSeedingStatus { get; set; }
        public string? AadhaarSeedingStatusDesc { get; set; }
        public int? NameMatchScore { get; set; }
        public string? NameMatchResult { get; set; }
        public bool NameMatches { get; set; }
        public string? LastUpdatedAt { get; set; }
    }

    // Cashfree API Response Model
    public class CashfreePanResponse
    {
        public string? pan { get; set; }
        public string? type { get; set; }
        public int? reference_id { get; set; }
        public string? name_provided { get; set; }
        public string? registered_name { get; set; }
        public bool? valid { get; set; }
        public string? message { get; set; }
        public int? name_match_score { get; set; }
        public string? name_match_result { get; set; }
        public string? aadhaar_seeding_status { get; set; }
        public string? last_updated_at { get; set; }
        public string? name_pan_card { get; set; }
        public string? pan_status { get; set; }
        public string? aadhaar_seeding_status_desc { get; set; }
    }
}

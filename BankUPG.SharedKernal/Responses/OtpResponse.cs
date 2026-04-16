namespace BankUPG.SharedKernal.Responses
{
    public class OtpResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? RemainingSeconds { get; set; }
    }
}

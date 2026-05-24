namespace BankUPG.Application.Interfaces.Auth
{
    public interface ITokenBlocklistService
    {
        void Blocklist(string jti, DateTime expiry);
        bool IsBlocklisted(string jti);
    }
}

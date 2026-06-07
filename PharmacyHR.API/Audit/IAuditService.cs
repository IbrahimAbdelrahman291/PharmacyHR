namespace PharmacyHR.API.Audit
{
    public interface IAuditService
    {
        Task LogAsync(string userId, string userName, string action);
    }
}

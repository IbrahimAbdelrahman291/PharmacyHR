namespace PharmacyHR.API.Audit
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

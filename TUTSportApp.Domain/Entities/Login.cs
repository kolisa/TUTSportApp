using TUTSportApp.Domain.Common;

namespace TUTSportApp.Domain.Entities
{
    public class Login : AuditableEntity
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime? LastLoginDate { get; set; }
        public bool IsLocked { get; set; }
        public int FailedAttempts { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
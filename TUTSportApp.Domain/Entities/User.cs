using TUTSportApp.Domain.Common;

namespace TUTSportApp.Domain.Entities
{
    public class User : AuditableEntity
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public Guid? CompanyId { get; set; }
        public virtual Company? Company { get; set; }
        public virtual Login? Login { get; set; }
    }
}
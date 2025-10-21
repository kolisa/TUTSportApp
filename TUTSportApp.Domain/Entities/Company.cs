using System;
using System.Collections.Generic;
using TUTSportApp.Domain.Common;

namespace TUTSportApp.Domain.Entities
{
    public class Company : AuditableEntity
    {
    
        public string Name { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Changed to read-only to satisfy CA2227.
        public virtual ICollection<User> Users { get; } = new List<User>();
    }
}
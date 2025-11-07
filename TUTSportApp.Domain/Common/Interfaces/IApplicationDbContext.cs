using Microsoft.EntityFrameworkCore;

using TUTSportApp.Domain.Entities;
namespace TUTSportApp.Domain.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Login> Logins { get; }
        DbSet<Company> Companies { get; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
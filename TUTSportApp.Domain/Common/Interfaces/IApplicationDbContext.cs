using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using TUTSportApp.Domain.Entities;
using TUTSportApp.Domain.Models;

namespace TUTSportApp.Domain.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Login> Logins { get; }
        DbSet<Company> Companies { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

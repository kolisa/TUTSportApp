using System.Threading;
using System.Threading.Tasks;

namespace TUTSportApp.Domain.Common.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

using TUTSportApp.Domain.Entities;

namespace TUTSportApp.Application.Common.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<User>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
        Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> GetUserWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
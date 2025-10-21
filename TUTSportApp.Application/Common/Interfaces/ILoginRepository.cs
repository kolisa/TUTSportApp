using TUTSportApp.Domain.Entities;

namespace TUTSportApp.Application.Common.Interfaces
{
    public interface ILoginRepository : IGenericRepository<Login>
    {
        Task<Login?> GetByUsernameAsync(string username);
        Task<Login?> GetByUserIdAsync(Guid userId);
        Task UpdateFailedAttemptsAsync(Guid loginId, int attempts);
        Task<bool> IsUsernameUniqueAsync(string username);
    }
}
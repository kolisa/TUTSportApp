using TUTSportApp.Domain.Entities;

namespace TUTSportApp.Application.Common.Interfaces;

public interface ICompanyRepository : IGenericRepository<Company>
{
    Task<Company?> GetByRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Company>> GetActiveCompaniesAsync(CancellationToken cancellationToken = default);
    Task<bool> IsRegistrationNumberUniqueAsync(string registrationNumber, CancellationToken cancellationToken = default);
    Task<Company?> GetCompanyWithUsersAsync(Guid id, CancellationToken cancellationToken = default);
}

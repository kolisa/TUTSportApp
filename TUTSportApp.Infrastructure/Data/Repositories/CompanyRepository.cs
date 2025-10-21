using Microsoft.EntityFrameworkCore;
using TUTSportApp.Application.Common.Interfaces;
using TUTSportApp.Domain.Entities;
using TUTSportApp.Infrastructure.Data.Context;

namespace TUTSportApp.Infrastructure.Data.Repositories
{
    public class CompanyRepository : GenericRepository<Company>, ICompanyRepository
    {
        public CompanyRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Company?> GetByRegistrationNumberAsync(
            string registrationNumber,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(registrationNumber))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(registrationNumber));
            }

            return await Set
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.RegistrationNumber == registrationNumber, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<Company>> GetActiveCompaniesAsync(
            CancellationToken cancellationToken = default)
        {
            return await Set
                .AsNoTracking()
                .Where(c => c.IsActive && !c.IsDeleted)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> IsRegistrationNumberUniqueAsync(
            string registrationNumber,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(registrationNumber))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(registrationNumber));
            }

            return !await Set
                .AsNoTracking()
                .AnyAsync(c => c.RegistrationNumber == registrationNumber, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Company?> GetCompanyWithUsersAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await Set
                .Include(c => c.Users)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}


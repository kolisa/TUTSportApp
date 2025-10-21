using Microsoft.EntityFrameworkCore;
using TUTSportApp.Application.Common.Interfaces;
using TUTSportApp.Domain.Entities;
using TUTSportApp.Infrastructure.Data.Context;

namespace TUTSportApp.Infrastructure.Data.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private const string CiCollation = "SQL_Latin1_General_CP1_CI_AS";

        public UserRepository(ApplicationDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));
            }

            return await Set
                .AsNoTracking()
                // Case-insensitive, server-side comparison; no culture warnings, EF-translatable
                .FirstOrDefaultAsync(u => EF.Functions.Collate(u.Email, CiCollation) == email, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<User>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
            => await Set
                .AsNoTracking()
                .Where(u => u.CompanyId == companyId && !u.IsDeleted)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

        public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));
            }

            return !await Set
                .AsNoTracking()
                .AnyAsync(u => EF.Functions.Collate(u.Email, CiCollation) == email, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<User?> GetUserWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
            => await Set
                .Include(u => u.Company)
                .Include(u => u.Login)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken)
                .ConfigureAwait(false);
    }
}


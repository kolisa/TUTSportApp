using Microsoft.EntityFrameworkCore;
using TUTSportApp.Domain.Common.Interfaces;
using TUTSportApp.Domain.Entities;
using TUTSportApp.Infrastructure.Data.Context;

namespace TUTSportApp.Infrastructure.Data.Repositories
{
    public class LoginRepository : GenericRepository<Login>, ILoginRepository
    {
        // Pick a case-insensitive collation that matches your DB (example below is common on SQL Server)
        private const string CiCollation = "SQL_Latin1_General_CP1_CI_AS";

        public LoginRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Login?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or whitespace.", nameof(username));
            }

            return await Set
                .AsNoTracking()
                // Server-side, case-insensitive comparison; no culture warnings and fully translatable
                .FirstOrDefaultAsync(l => EF.Functions.Collate(l.Username, CiCollation) == username)
                .ConfigureAwait(false);
        }

        public async Task<Login?> GetByUserIdAsync(Guid userId)
        {
            return await Set
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.UserId == userId)
                .ConfigureAwait(false);
        }

        public async Task UpdateFailedAttemptsAsync(Guid loginId, int attempts)
        {
            var login = await Set.FindAsync(loginId).ConfigureAwait(false);

            if (login != null)
            {
                login.FailedAttempts = attempts;
                login.IsLocked = attempts >= 5;

                await UpdateAsync(login).ConfigureAwait(false);
            }
        }

        public async Task<bool> IsUsernameUniqueAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or whitespace.", nameof(username));
            }

            return !await Set
                .AsNoTracking()
                .AnyAsync(l => EF.Functions.Collate(l.Username, CiCollation) == username)
                .ConfigureAwait(false);
        }
    }
}


using Microsoft.EntityFrameworkCore;
using TUTSportApp.Domain.Common.Interfaces;
using TUTSportApp.Infrastructure.Data.Context;

namespace TUTSportApp.Infrastructure.Data.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        // Auto-properties (no visible fields) — satisfies "Use auto property"
        protected ApplicationDbContext Context { get; }
        protected DbSet<T> Set { get; }

        public GenericRepository(ApplicationDbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Set = Context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
            => await Set.FindAsync(id).AsTask().ConfigureAwait(false);

        public virtual async Task<IReadOnlyList<T>> GetAllAsync()
            => await Set.AsNoTracking().ToListAsync().ConfigureAwait(false);

        public virtual async Task<T> AddAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            await Set.AddAsync(entity).AsTask().ConfigureAwait(false);
            return entity;
        }

        public virtual Task UpdateAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            Context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public virtual Task DeleteAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            Set.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual async Task<bool> ExistsAsync(Guid id)
            => await Set.FindAsync(id).AsTask().ConfigureAwait(false) is not null;
    }
}


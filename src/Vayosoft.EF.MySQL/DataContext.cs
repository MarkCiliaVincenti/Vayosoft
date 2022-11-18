﻿using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vayosoft.Commons.Entities;
using Vayosoft.Persistence;
using Vayosoft.Persistence.Criterias;
using Vayosoft.Persistence.Specifications;

namespace Vayosoft.EF.MySQL
{
    public class DataContext : DbContext, ILinqProvider, IDataProvider, IUnitOfWork
    {
        public DataContext(DbContextOptions options)
            : base(options) { }

        public TEntity Find<TEntity>(object id) 
            where TEntity : class, IEntity
        {
            return Set<TEntity>()
                .AsTracking()
                .SingleOrDefault(x => x.Id == id);
        }

        public Task<TEntity> FindAsync<TEntity>(object id, CancellationToken cancellationToken) 
            where TEntity : class, IEntity
        {
            return Set<TEntity>()
                .AsTracking()
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);
        }


        public new void Add<TEntity>(TEntity entity)
            where TEntity : class, IEntity {
           base.Add(entity);
        }

        public new async ValueTask AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken)
            where TEntity : class, IEntity {
            await base.AddAsync(entity, cancellationToken);
        }

        public new void Update<TEntity>(TEntity entity)
            where TEntity : class, IEntity {
            base.Update(entity);
        }

        public void Delete<TEntity>(TEntity entity)
            where TEntity : class, IEntity {
            base.Remove(entity);
        }

        public void Commit() {
            SaveChanges();
        }

        public async Task CommitAsync() {
            await SaveChangesAsync();
        }


        public Task<TEntity> SingleAsync<TEntity>(ICriteria<TEntity> criteria, CancellationToken cancellationToken = default) 
            where TEntity : class, IEntity
        {
            return Set<TEntity>()
                //.AsNoTracking()
                .Where(criteria.ToExpression())
                .SingleOrDefaultAsync(cancellationToken);
        }

        public Task<List<TEntity>> ListAsync<TEntity>(ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            return Set<TEntity>()
                //.AsNoTracking()
                .Evaluate(spec)
                .ToListAsync(cancellationToken);
        }

        public IAsyncEnumerable<TEntity> StreamAsync<TEntity>(ISpecification<TEntity> spec)
            where TEntity : class, IEntity
        {
            return Set<TEntity>()
                //.AsNoTracking()
                .Evaluate(spec)
                .AsAsyncEnumerable();
        }


        public IQueryable<TEntity> AsQueryable<TEntity>()
            where TEntity : class, IEntity
        {
            return Set<TEntity>()
                .AsNoTracking();
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var typesToRegister = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(type => !string.IsNullOrEmpty(type.Namespace))
                .Where(type => type.BaseType is { IsGenericType: true } && type.BaseType.GetGenericTypeDefinition() == typeof(EntityConfigurationMapper<>));

            foreach (var type in typesToRegister)
            {
                dynamic configInstance = Activator.CreateInstance(type)!;
                modelBuilder.ApplyConfiguration(configInstance);
            }
        }
    }
}

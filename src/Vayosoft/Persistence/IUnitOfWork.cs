﻿using Vayosoft.Commons.Entities;

namespace Vayosoft.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        TEntity Find<TEntity>(object id)
            where TEntity : class, IEntity;

        Task<TEntity> FindAsync<TEntity>(object id, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity;


        TEntity Add<TEntity>(TEntity entity)
            where TEntity : class, IEntity;

        ValueTask<TEntity> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity;


        void Update<TEntity>(TEntity entity)
            where TEntity : class, IEntity;

        void Delete<TEntity>(TEntity entity)
            where TEntity : class, IEntity;


        void Commit();

        Task CommitAsync();
    }
}
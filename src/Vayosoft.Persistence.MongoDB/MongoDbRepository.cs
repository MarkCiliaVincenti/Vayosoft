﻿using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Vayosoft.Commons;
using Vayosoft.Commons.Aggregates;
using Vayosoft.Commons.Models.Pagination;
using Vayosoft.Persistence.MongoDB.Extensions;
using Vayosoft.Persistence.Specifications;

namespace Vayosoft.Persistence.MongoDB
{
    public class MongoDbRepository<T> : IRepository<T> where T : class, IAggregateRoot
    {
        protected readonly IMapper Mapper;
        protected readonly IMongoDbConnection Connection;

        protected readonly IMongoCollection<T> Collection;

        public MongoDbRepository(IMongoDbConnection connection, IMapper mapper)
        {
            Mapper = mapper;
            Connection = connection;
            Collection = connection.Collection<T>(CollectionName.For<T>());
        }

        protected IMongoQueryable<TEntity> Set<TEntity>() => 
            Connection.Collection<TEntity>(CollectionName.For<TEntity>()).AsQueryable();

        public IQueryable<T> AsQueryable()
            => Collection.AsQueryable();

        public Task<T> FindAsync<TId>(TId id, CancellationToken cancellationToken = default) =>
            Collection.Find(q => q.Id.Equals(id)).FirstOrDefaultAsync(cancellationToken);
        public Task<TResult> FindAsync<TId, TResult>(TId id, CancellationToken cancellationToken = default) =>
            Collection.Find(q => q.Id.Equals(id)).Project(e => Mapper.Map<TResult>(e)).FirstOrDefaultAsync(cancellationToken);


        public virtual Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
            Collection.InsertOneAsync(entity, cancellationToken: cancellationToken);

        public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default) =>
            Collection.ReplaceOneAsync(Builders<T>.Filter.Eq(e => e.Id, entity.Id), entity, cancellationToken: cancellationToken);

        public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default) =>
            Collection.DeleteOneAsync(Builders<T>.Filter.Eq(e => e.Id, entity.Id), cancellationToken: cancellationToken);
        public virtual Task DeleteAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull =>
            Collection.DeleteOneAsync(Builders<T>.Filter.Eq(e => e.Id, id), cancellationToken: cancellationToken);


        public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> criteria, CancellationToken cancellationToken = default) =>
            Collection.Find(criteria).FirstOrDefaultAsync(cancellationToken);

        public Task<T> FirstOrDefaultAsync(ILinqSpecification<T> spec, CancellationToken cancellationToken = default) =>
            Collection.AsQueryable().Apply(spec).FirstOrDefaultAsync(cancellationToken);


        public Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> criteria, CancellationToken cancellationToken = default) =>
            Collection.Find(criteria).SingleOrDefaultAsync(cancellationToken);


        public Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default) {
            return Collection.AsQueryable().Apply(spec).ToListAsync(cancellationToken);
        }
        
        public IAsyncEnumerable<T> StreamAsync(ISpecification<T> spec, CancellationToken cancellationToken = default) {
            return Collection.AsQueryable().Apply(spec).ToAsyncEnumerable(cancellationToken);
        }

        public Task<IPagedEnumerable<T>> PageAsync(ILinqSpecification<T> spec, int page = 1, int pageSize = IPagingModel.DefaultSize, CancellationToken cancellationToken = default) {
            return Collection.AsQueryable().Apply(spec).ToPagedEnumerableAsync(page, pageSize, cancellationToken: cancellationToken);
        }

        //public Task<IPagedEnumerable<T>> PagedListAsync(IPagingModel<T, object> model, Expression<Func<T, bool>> criteria, CancellationToken cancellationToken) =>
        //    Collection.AggregateByPage(model, Builders<T>.Filter.Where(criteria), cancellationToken);
    }
}

using System;
using System.Linq.Expressions;
using Data.Sorting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Data.Extensions;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        private readonly DbContext _dbContext;
        private readonly ILogger<Repository<TEntity>> _logger;

        public Repository(ApplicationDbContext dbContext, ILogger<Repository<TEntity>> logger)
        {
            this._dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Create(TEntity entity)
        {
            _logger.LogDebug("Creating entity of type {Type}", typeof(TEntity).Name);
            this._dbContext.Set<TEntity>().Add(entity);
            try
            {
                this._dbContext.SaveChanges();
                _logger.LogInformation("Entity of type {Type} created successfully.", typeof(TEntity).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving created entity of type {Type}", typeof(TEntity).Name);
                throw;
            }
        }

        public void Update(TEntity entity)
        {
            _logger.LogDebug("Updating entity of type {Type}", typeof(TEntity).Name);
            this._dbContext.Set<TEntity>().Update(entity);
            try
            {
                this._dbContext.SaveChanges();
                _logger.LogInformation("Entity of type {Type} updated successfully.", typeof(TEntity).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving updated entity of type {Type}", typeof(TEntity).Name);
                throw;
            }
        }

        public void Delete(TEntity entity)
        {
            _logger.LogDebug("Deleting entity of type {Type}", typeof(TEntity).Name);
            this._dbContext.Set<TEntity>().Remove(entity);
            try
            {
                this._dbContext.SaveChanges();
                _logger.LogInformation("Entity of type {Type} deleted successfully.", typeof(TEntity).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity of type {Type}", typeof(TEntity).Name);
                throw;
            }
        }

        public TEntity? Get(Expression<Func<TEntity, bool>> filter)
        {
            return this._dbContext.Set<TEntity>().Where(filter).FirstOrDefault();
        }

        public TProjection? Get<TProjection>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TProjection>> projection)
        {
            return this._dbContext.Set<TEntity>().Where(filter).Select(projection).FirstOrDefault();
        }

        public TEntity? GetComplete(Expression<Func<TEntity, bool>> filter)
        {
            IEntityType? entityType = this._dbContext.Model.FindEntityType(typeof(TEntity));
            if (entityType is null) throw new InvalidOperationException("Invalid entity type.");

            IEnumerable<string> navigations = entityType.GetNavigations().Select(x => x.Name)
                .Concat(entityType.GetSkipNavigations().Select(x => x.Name));

            return this.GetWithNavigations(filter, navigations);
        }

        public TEntity? GetWithNavigations(Expression<Func<TEntity, bool>> filter, IEnumerable<string> navigations)
        {
            IQueryable<TEntity> query = this._dbContext.Set<TEntity>().Where(filter);

            foreach (string navigation in navigations)
                query = query.Include(navigation);

            return query.FirstOrDefault();
        }

        public IEnumerable<TEntity> GetMany(Expression<Func<TEntity, bool>> filter)
        {
            return this._dbContext.Set<TEntity>().Where(filter).ToList();
        }

        public IEnumerable<TProjection> GetMany<TProjection>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TProjection>> projection)
        {
            return this._dbContext.Set<TEntity>().Where(filter).Select(projection).ToList();
        }

        public IEnumerable<TProjection> GetMany<TProjection>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TProjection>> projection, IEnumerable<IOrderClause<TEntity>> orderClauses)
        {
            return this._dbContext.Set<TEntity>().Where(filter).OrderBy(orderClauses).Select(projection).ToList();
        }
    }

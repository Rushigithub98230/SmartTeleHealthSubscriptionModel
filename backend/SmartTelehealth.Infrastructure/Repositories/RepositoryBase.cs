using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;
using System.Linq.Expressions;

namespace SmartTelehealth.Infrastructure.Repositories;

public class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    private readonly ApplicationDbContext context;
    private DbSet<T> entities;
    string errorMessage = string.Empty;
    
    public RepositoryBase(ApplicationDbContext _context)
    {
        this.context = _context;
        entities = context.Set<T>();
    }
   
    /// <summary>
    /// Gets the first entity found or default value.
    /// </summary>
    /// <param name="filter">Filter expression for filtering the entities.</param>
    /// <param name="include">Include for eager-loading.</param>
    /// <returns></returns>
    public virtual T GetFirstOrDefault(Expression<Func<T, bool>> filter,
                                      params Expression<Func<T, object>>[] include)
    {
        IQueryable<T> dbQuery = SelectQuery(filter, include);
        return dbQuery.AsNoTracking().FirstOrDefault();
    }

    /// <summary>
    /// Creates the specified entity/entities.
    /// </summary>
    /// <param name="entity">Single entity.</param>
    /// <param name="entities">Multiple entities.</param>
    public virtual void Create(T entity, params T[] entities)
    {
        EntityState state = EntityState.Added;
        SetEntityState(state, entity, entities);
    }

    /// <summary>
    /// Creates the specified entity/entities.
    /// </summary>
    /// <param name="entities">Multiple entities.</param>
    public void Create(T[] entities)
    {
        EntityState state = EntityState.Added;
        SetEntityStateForArray(state, entities);
    }

    /// <summary>
    /// Updates the specified entity/entities.
    /// </summary>
    /// <param name="entity">Single entity.</param>
    /// <param name="entities">Multiple entities.</param>
    public virtual void Update(T entity, params T[] entities)
    {
        EntityState state = EntityState.Modified;
        SetEntityState(state, entity, entities);
    }

    /// <summary>
    /// Updates the specified entity/entities.
    /// </summary>
    /// <param name="entities">Multiple entities.</param>
    public virtual void Update(T[] entities)
    {
        EntityState state = EntityState.Modified;
        SetEntityStateForArray(state, entities);
    }

    /// <summary>
    /// Deletes the specified entity/entities.
    /// </summary>
    /// <param name="entity">Single entity.</param>
    /// <param name="entities">Multiple entities.</param>
    public virtual void Delete(T entity, params T[] entities)
    {
        EntityState state = EntityState.Deleted;
        SetEntityState(state, entity, entities);
    }

    /// <summary>
    /// Deletes the entity by identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    public virtual void Delete(object id)
    {
        T entity = CreateDbSet<T>().Find(id);
        EntityState state = EntityState.Deleted;
        SetEntityState(state, entity);
    }

    /// <summary>
    /// Deletes multiple entities which are found using filter.
    /// </summary>
    /// <param name="filter">Filter expression for filtering the entities.</param>
    public virtual void Delete(Expression<Func<T, bool>> filter)
    {
        IQueryable<T> dbQuery = SelectQuery(filter);
        dbQuery.AsNoTracking().ToList().ForEach(item => context.Entry(item).State = EntityState.Deleted);
    }

    /// <summary>
    /// Saves the changes to the database.
    /// </summary>
    /// <returns>Number of rows affected.</returns>
    public int SaveChanges(int? userId = null,bool isAuditRequired=true)
    {
        int recordsAffected = context.SaveChanges();
        //this.Dispose();  // uncommented by kundan for memeory release 
        return recordsAffected;
    }

    /// <summary>
    /// Fetch all records .
    /// </summary>
    /// <returns></returns>
    /// 
    public IQueryable<T> FetchAll()
    {
        return GetAll();
    }

    public IQueryable<T> GetAll()
    {
        return context.Set<T>();
    }

    public IQueryable<T> GetAll(Expression<Func<T, bool>> exp)
    {
        return context.Set<T>().Where(exp);
    }
    
    public T Get(Expression<Func<T, bool>> exp)
    {
        return context.Set<T>().Where(exp).FirstOrDefault(); ;
    }

    public virtual T GetByID(object id)
    {
        return CreateDbSet<T>().Find(id);
    }

    #region Stored Procedures Factory
    //When you expect a model back (async)
    public async Task<IList<T>> ExecWithStoreProcedureAsync(string query, params object[] parameters)
    {
        // EF 6
        //context.Database.SqlQuery<T>(query, parameters).ToListAsync();
        // EF Core
        return await entities.FromSqlRaw(query, parameters).ToListAsync();
    }

    //When you expect a model back
    public IEnumerable<T> ExecWithStoreProcedure(string query)
    {
        // EF 6
        //_context.Database.SqlQuery<T>(query);
        // EF Core
        return entities.FromSqlRaw(query);
    }

    //When you expect a model back
    public IEnumerable<T> ExecWithStoreProcedureWithParameters(string query, params object[] parameters)
    {
        // EF 6
        //_context.Database.SqlQuery<T>(query, parameters);
        // EF Core
        return entities.FromSqlRaw(query, parameters);
    }

    //When you expect a model back
    public T ExecWithStoreProcedureWithParametersForModel(string query, params object[] parameters)
    {
        // EF 6
        //IEnumerable<TResult> dbQuery = _context.Database.SqlQuery<TResult>(query, parameters);
        //return dbQuery.FirstOrDefault();
        // EF Core
        IEnumerable<T> dbQuery = entities.FromSqlRaw(query, parameters);
        return dbQuery.FirstOrDefault();
    }

    // Fire and forget (async)
    public async Task ExecuteWithStoreProcedureAsync(string query, params object[] parameters)
    {
        // EF 6
        //await _context.Database.ExecuteSqlCommandAsync(query, parameters);
        // EF Core
        await context.Database.ExecuteSqlRawAsync(query, parameters, default(CancellationToken));
    }

    // Fire and get no. of row inserted
    public int ExecuteWithStoreProcedure(string query, params object[] parameters)
    {
        return context.Database.ExecuteSqlRaw(query, parameters);
    }
    #endregion

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="RepositoryBase{TEntity}"/> class.
    /// </summary>
    ~RepositoryBase()
    {
        Dispose(false);
    }
    
    protected DbSet<TEntity> CreateDbSet<TEntity>() where TEntity : class
    {
        return context.Set<TEntity>();
    }

    #region Private Methods
    private IQueryable<T> SelectQuery(Expression<Func<T, bool>> filter, Expression<Func<T, object>>[] include = null)
    {
        IQueryable<T> dbQuery = CreateDbSet<T>();

        if (filter != null)
        {
            dbQuery = dbQuery.Where(filter);
        }

        if (include != null)
        {
            dbQuery = include.Aggregate(dbQuery, (a, b) => a.Include(b));
        }
        return dbQuery;
    }

    private void SetEntityState(EntityState state, T entity, params T[] entities)
    {
        try
        {
            context.Entry(entity).State = state;
            foreach (T item in entities)
            {
                context.Entry(item).State = state;
            }
        }
        catch (Exception)
        {
        }
    }

    private void SetEntityStateForArray(EntityState state,T[] entities)
    {
        try
        {
            foreach (T item in entities)
            {
                context.Entry(item).State = state;
            }
        }
        catch (Exception)
        {
        }
    }
    
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (context != null)
            {
                context.Dispose();
            }
        }
    }

    public Task SaveChangesAsync()
    {
        return context.SaveChangesAsync();
    }
    
    public Task<int> SaveChangesReturnAsync()
    {
        return context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Gets the entity by identifier asynchronously.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    public virtual async Task<T?> GetByIdAsync(object id)
    {
        return await CreateDbSet<T>().FindAsync(id);
    }
    
    /// <summary>
    /// Gets all entities asynchronously.
    /// </summary>
    /// <returns></returns>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await CreateDbSet<T>().ToListAsync();
    }
    
    /// <summary>
    /// Creates the entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <returns></returns>
    public virtual async Task<T> CreateAsync(T entity)
    {
        var entry = await CreateDbSet<T>().AddAsync(entity);
        await context.SaveChangesAsync();
        return entry.Entity;
    }
    
    /// <summary>
    /// Updates the entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns></returns>
    public virtual async Task<T> UpdateAsync(T entity)
    {
        context.Entry(entity).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return entity;
    }
    
    /// <summary>
    /// Deletes the entity by identifier asynchronously.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    public virtual async Task<bool> DeleteAsync(object id)
    {
        T? entity = await CreateDbSet<T>().FindAsync(id);
        if (entity != null)
        {
            context.Entry(entity).State = EntityState.Deleted;
            await context.SaveChangesAsync();
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Checks if entity exists by identifier asynchronously.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    public virtual async Task<bool> ExistsAsync(object id)
    {
        var entity = await CreateDbSet<T>().FindAsync(id);
        return entity != null;
    }
    
    #endregion Private Methods
}

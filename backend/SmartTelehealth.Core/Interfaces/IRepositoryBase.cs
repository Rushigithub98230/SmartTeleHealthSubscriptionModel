using System.Linq.Expressions;

namespace SmartTelehealth.Core.Interfaces;

public interface IRepositoryBase<T> : IDisposable where T : class
{   
    /// <summary>
    /// Gets the first entity found or default value.
    /// </summary>
    /// <param name="filter">Filter expression for filtering the entities.</param>
    /// <param name="include">Include for eager-loading.</param>
    /// <returns></returns>
    T GetFirstOrDefault(Expression<Func<T, bool>> filter,
                                      params Expression<Func<T, object>>[] include);
    /// <summary>
    /// Creates the specified entity/entities.
    /// </summary>
    /// <param name="entity">Single entity.</param>
    /// <param name="entities">Multiple entities.</param>
    void Create(T entity, params T[] entities);

    /// <summary>
    /// Creates the specified entity/entities.
    /// </summary>
    /// <param name="entities">Multiple entities.</param>
    void Create(T[] entities);

    /// <summary>
    /// Updates the specified entity/entities.
    /// </summary>
    /// <param name="entity">Single entity.</param>
    /// <param name="entities">Multiple entities.</param>
    void Update(T entity, params T[] entities);

    /// <summary>
    /// Updates the specified entity/entities.
    /// </summary>
    /// <param name="entities">Multiple entities.</param>
    void Update(T[] entities);
    /// <summary>
    /// Deletes the specified entity/entities.
    /// </summary>
    /// <param name="entity">Single entity.</param>
    /// <param name="entities">Multiple entities.</param>
    void Delete(T entity, params T[] entities);
    /// <summary>
    /// Deletes the entity by identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    void Delete(object id);
    /// <summary>
    /// Deletes multiple entities which are found using filter.
    /// </summary>
    /// <param name="filter">Filter expression for filtering the entities.</param>
    void Delete(Expression<Func<T, bool>> filter);
    /// <summary>
    /// Saves the changes to the database.
    /// </summary>
    /// <returns>Number of rows affected.</returns>
    int SaveChanges(int? userId = null,bool isAuditRequired=true);

    /// <summary>
    /// Saves the changes to the database.
    /// </summary>
    /// <returns>Number of rows affected.</returns>
    Task SaveChangesAsync();

    /// <summary>
    /// Fetch all records.
    /// </summary>
    /// <returns></returns>
    /// 
    IQueryable<T> FetchAll();
    /// <summary>
    /// Get all records.
    /// </summary>
    /// <returns></returns>
    IQueryable<T> GetAll();
    IQueryable<T> GetAll(Expression<Func<T, bool>> exp);
    /// <summary>
    /// Get single record.
    /// </summary>
    /// <param name="exp"></param>
    /// <returns></returns>
    T Get(Expression<Func<T, bool>> exp);
    /// <summary>
    /// Gets the entity by identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    T GetByID(object id);
    
    /// <summary>
    /// Gets the entity by identifier asynchronously.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    Task<T?> GetByIdAsync(object id);
    
    /// <summary>
    /// Gets all entities asynchronously.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<T>> GetAllAsync();
    
    /// <summary>
    /// Creates the entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <returns></returns>
    Task<T> CreateAsync(T entity);
    
    /// <summary>
    /// Updates the entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns></returns>
    Task<T> UpdateAsync(T entity);
    
    /// <summary>
    /// Deletes the entity by identifier asynchronously.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    Task<bool> DeleteAsync(object id);
    
    /// <summary>
    /// Checks if entity exists by identifier asynchronously.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    Task<bool> ExistsAsync(object id);

    #region StoredProceduresFactory
    Task<IList<T>> ExecWithStoreProcedureAsync(string query, params object[] parameters);
    IEnumerable<T> ExecWithStoreProcedure(string query);
    IEnumerable<T> ExecWithStoreProcedureWithParameters(string query, params object[] parameters);
    T ExecWithStoreProcedureWithParametersForModel(string query, params object[] parameters);
    Task ExecuteWithStoreProcedureAsync(string query, params object[] parameters);
    int ExecuteWithStoreProcedure(string query, params object[] parameters);
    #endregion
    Task<int> SaveChangesReturnAsync();
}

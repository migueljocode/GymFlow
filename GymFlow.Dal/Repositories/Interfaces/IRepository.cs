namespace GymFlow.Dal.Repositories.Interfaces;

/// <summary>
/// Generic repository interface for basic CRUD operations with soft delete support.
/// </summary>
/// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
/// <remarks>
/// This interface provides a set of common database operations that can be performed on any entity.
/// All query methods use expressions that are translated to SQL by Entity Framework Core,
/// ensuring optimal performance with database-side filtering.
/// </remarks>
public interface IRepository<T> where T : class
{
    // ========== Query Methods - Single Items ==========
    
    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the entity if found; otherwise, <c>null</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// var user = await userRepository.GetByIdAsync(5);
    /// if (user != null) {
    ///     Console.WriteLine($"Found: {user.FirstName}");
    /// }
    /// </code>
    /// </example>
    Task<T?> GetByIdAsync(int id);
    
    /// <summary>
    /// Retrieves the first entity that matches the specified predicate, or <c>null</c> if none found.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the first matching entity, or <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method executes as a SQL query on the database, not in memory.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activePlan = await planRepository.FirstOrDefaultAsync(p => p.IsActive &amp;&amp; p.UserId == userId);
    /// </code>
    /// </example>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// Retrieves the only entity that matches the specified predicate, or <c>null</c> if none found.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the single matching entity, or <c>null</c>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when more than one entity matches the predicate.
    /// </exception>
    /// <remarks>
    /// Use this method when you expect exactly zero or one result. If multiple results exist,
    /// an exception is thrown, which helps catch data integrity issues early.
    /// </remarks>
    /// <example>
    /// <code>
    /// var uniqueUser = await userRepository.SingleOrDefaultAsync(u => u.Email == "john@example.com");
    /// </code>
    /// </example>
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);
    
    // ========== Query Methods - Collections ==========
    
    /// <summary>
    /// Retrieves all entities from the database that are not soft-deleted.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains a collection of all entities.
    /// </returns>
    /// <remarks>
    /// ⚠️ Warning: This method loads ALL entities into memory. Use with caution on large datasets.
    /// Consider using <see cref="FindAsync"/> or paging for large tables.
    /// </remarks>
    /// <example>
    /// <code>
    /// var allUsers = await userRepository.GetAllAsync();
    /// foreach (var user in allUsers) {
    ///     Console.WriteLine($"{user.FirstName} {user.LastName}");
    /// }
    /// </code>
    /// </example>
    Task<IEnumerable<T>> GetAllAsync();
    
    /// <summary>
    /// Retrieves all entities that match the specified predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains a collection of matching entities.
    /// </returns>
    /// <remarks>
    /// The filtering is performed on the database server for optimal performance.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activeUsers = await userRepository.FindAsync(u => u.IsActive);
    /// var recentLogs = await logRepository.FindAsync(l => l.LogDate >= DateOnly.FromDateTime(DateTime.Now.AddDays(-7)));
    /// </code>
    /// </example>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    
    // ========== Query Methods - Aggregation ==========
    
    /// <summary>
    /// Determines whether any entity in the database matches the specified predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains <c>true</c> if any matching entity exists; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method is optimized to return as soon as a match is found,
    /// without loading the actual entities into memory.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool hasWorkouts = await workoutPlanRepository.AnyAsync(wp => wp.UserId == userId);
    /// if (!hasWorkouts) {
    ///     Console.WriteLine("User has no workout plans yet.");
    /// }
    /// </code>
    /// </example>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// Determines whether all entities in the database match the specified predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains <c>true</c> if all entities match; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method is executed on the database server for optimal performance.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool allActive = await userRepository.AllAsync(u => u.IsActive);
    /// </code>
    /// </example>
    Task<bool> AllAsync(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// Counts the number of entities that match the specified predicate.
    /// </summary>
    /// <param name="predicate">
    /// A function to test each element for a condition.
    /// If <c>null</c>, counts all entities.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the number of matching entities.
    /// </returns>
    /// <remarks>
    /// This method executes a COUNT query on the database without loading entities into memory.
    /// </remarks>
    /// <example>
    /// <code>
    /// int totalUsers = await userRepository.CountAsync();
    /// int activeUsers = await userRepository.CountAsync(u => u.IsActive);
    /// </code>
    /// </example>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    
    // ========== Command Methods - Single ==========
    
    /// <summary>
    /// Adds a new entity to the database.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the added entity (including generated ID).
    /// </returns>
    /// <remarks>
    /// The entity's <c>CreatedAt</c> timestamp is automatically set to the current UTC time.
    /// This method saves changes immediately to the database.
    /// </remarks>
    /// <example>
    /// <code>
    /// var newUser = new User { FirstName = "John", LastName = "Doe" };
    /// var added = await userRepository.AddAsync(newUser);
    /// Console.WriteLine($"User created with ID: {added.Id}");
    /// </code>
    /// </example>
    Task<T> AddAsync(T entity);
    
    /// <summary>
    /// Updates an existing entity in the database.
    /// </summary>
    /// <param name="entity">The entity with updated values.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the updated entity.
    /// </returns>
    /// <remarks>
    /// The entity's <c>UpdatedAt</c> timestamp is automatically set to the current UTC time.
    /// This method saves changes immediately to the database.
    /// </remarks>
    /// <example>
    /// <code>
    /// var user = await userRepository.GetByIdAsync(5);
    /// user.FirstName = "Jonathan";
    /// var updated = await userRepository.UpdateAsync(user);
    /// </code>
    /// </example>
    Task<T> UpdateAsync(T entity);
    
    /// <summary>
    /// Permanently deletes an entity from the database (hard delete).
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains <c>true</c> if deletion succeeded; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// ⚠️ Warning: This performs a hard delete, completely removing the entity from the database.
    /// Consider using <see cref="SoftDeleteAsync"/> for recoverable deletions.
    /// </remarks>
    Task<bool> DeleteAsync(T entity);
    
    /// <summary>
    /// Permanently deletes an entity by its ID (hard delete).
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains <c>true</c> if deletion succeeded; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// ⚠️ Warning: This performs a hard delete. Use <see cref="SoftDeleteAsync"/> when you want
    /// to preserve data for historical records.
    /// </remarks>
    Task<bool> DeleteByIdAsync(int id);
    
    /// <summary>
    /// Soft deletes an entity by marking it as deleted without removing from database.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to soft delete.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains <c>true</c> if soft deletion succeeded; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method sets <c>IsDeleted = true</c> and records the <c>DeletedAt</c> timestamp.
    /// Soft-deleted entities are automatically excluded from all queries via global query filter.
    /// This is preferred over hard delete for preserving audit trails and historical data.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Archive a user instead of permanently deleting
    /// var deleted = await userRepository.SoftDeleteAsync(userId);
    /// </code>
    /// </example>
    Task<bool> SoftDeleteAsync(int id);
    
    // ========== Command Methods - Range ==========
    
    /// <summary>
    /// Adds multiple entities to the database in a single batch operation.
    /// </summary>
    /// <param name="entities">The collection of entities to add.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the added entities (including generated IDs).
    /// </returns>
    /// <remarks>
    /// This method is more efficient than calling <see cref="AddAsync"/> multiple times
    /// as it saves changes only once after adding all entities.
    /// </remarks>
    /// <example>
    /// <code>
    /// var newExercises = new List&lt;Exercise&gt; {
    ///     new Exercise { Name = "Bench Press" },
    ///     new Exercise { Name = "Squat" }
    /// };
    /// var added = await exerciseRepository.AddRangeAsync(newExercises);
    /// </code>
    /// </example>
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    
    /// <summary>
    /// Permanently deletes multiple entities from the database (hard delete).
    /// </summary>
    /// <param name="entities">The collection of entities to delete.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains <c>true</c> if deletions succeeded; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method performs a hard delete on all specified entities in a single batch operation.
    /// Consider using individual <see cref="SoftDeleteAsync"/> for recoverable deletions.
    /// </remarks>
    Task<bool> DeleteRangeAsync(IEnumerable<T> entities);
    
    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the number of state entries written to the database.
    /// </returns>
    /// <remarks>
    /// This method is automatically called after Add/Update/Delete operations,
    /// but is exposed for scenarios where multiple changes are batched together.
    /// </remarks>
    Task<int> SaveChangesAsync();
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace Bam.AI.Orchestration.Memory
{
    /// <summary>
    /// Manages agent memory and knowledge persistence across conversations.
    /// </summary>
    /// <remarks>
    /// The memory manager provides different scopes of persistent storage:
    /// 
    /// Session Memory: Valid only for the current conversation
    /// - Temporary context and state
    /// - Cleared when conversation ends
    /// - Fast in-memory storage
    /// 
    /// User Memory: Persists across all conversations for a user
    /// - User preferences and settings
    /// - Learned patterns and behaviors
    /// - Personal information (with consent)
    /// 
    /// Global Memory: Shared across all users
    /// - Common knowledge and facts
    /// - Shared resources and configurations
    /// - System-wide state
    /// 
    /// Tool Memory: Specific to tool implementations
    /// - Tool-specific caches
    /// - API tokens and credentials
    /// - Tool configuration
    /// 
    /// This implementation uses in-memory storage. For production, integrate:
    /// - Redis for distributed caching
    /// - PostgreSQL for structured data
    /// - Azure Cosmos DB for global distribution
    /// - Vector databases (Pinecone, Weaviate) for semantic search
    /// 
    /// Thread Safety: This class is thread-safe using concurrent collections.
    /// </remarks>
    /// <example>
    /// <code>
    /// var memory = new MemoryManager(logger);
    /// 
    /// // Store user preference
    /// await memory.StoreAsync("language", "es", MemoryScope.User);
    /// 
    /// // Retrieve later
    /// var language = await memory.RetrieveAsync&lt;string&gt;("language", MemoryScope.User);
    /// 
    /// // Search for related items
    /// var results = await memory.SearchAsync("preference", MemoryScope.User);
    /// </code>
    /// </example>
    public class MemoryManager : IMemoryManager
    {
        private readonly ILogger<MemoryManager> _logger;

        // Separate storage for each scope
        private readonly ConcurrentDictionary<string, MemoryItem> _sessionMemory;
        private readonly ConcurrentDictionary<string, MemoryItem> _userMemory;
        private readonly ConcurrentDictionary<string, MemoryItem> _globalMemory;
        private readonly ConcurrentDictionary<string, MemoryItem> _toolMemory;


        /// <summary>
        /// Default expiration time for session memory.
        /// </summary>
        /// <remarks>
        /// Session memory expires after 1 hour of inactivity. This prevents
        /// memory leaks from abandoned sessions while keeping data available
        /// for active conversations.
        /// </remarks>
        private static readonly TimeSpan SessionExpiration = TimeSpan.FromHours(1);


        /// <summary>
        /// Default expiration time for user memory.
        /// </summary>
        /// <remarks>
        /// User memory expires after 30 days of inactivity. This balances
        /// personalization with privacy and storage costs.
        /// </remarks>
        private static readonly TimeSpan UserExpiration = TimeSpan.FromDays(30);


        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryManager"/> class.
        /// </summary>
        /// <param name="logger">Logger instance for diagnostics.</param>
        public MemoryManager(ILogger<MemoryManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sessionMemory = new ConcurrentDictionary<string, MemoryItem>();
            _userMemory = new ConcurrentDictionary<string, MemoryItem>();
            _globalMemory = new ConcurrentDictionary<string, MemoryItem>();
            _toolMemory = new ConcurrentDictionary<string, MemoryItem>();


            // Start background task to clean up expired items
            StartExpirationCleanup();
        }


        /// <summary>
        /// Stores a value in memory.
        /// </summary>
        /// <param name="key">The storage key.</param>
        /// <param name="value">The value to store.</param>
        /// <param name="scope">The memory scope.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> or <paramref name="value"/> is null.
        /// </exception>
        /// <remarks>
        /// Storage behavior by scope:
        /// - Session: Expires after 1 hour
        /// - User: Expires after 30 days
        /// - Global: Never expires (use carefully)
        /// - Tool: Never expires (managed by tool)
        /// 
        /// Keys should be descriptive and scoped appropriately:
        /// - Good: "user:preferences:language"
        /// - Bad: "data"
        /// 
        /// Consider key naming conventions:
        /// - Use colons for hierarchy: "category:subcategory:item"
        /// - Include IDs: "user:123:preferences"
        /// - Be specific: "openai:api:last_call_time"
        /// </remarks>
        public Task StoreAsync(string key, object value, MemoryScope scope)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));


            var storage = GetStorage(scope);
            var expiresAt = GetExpirationTime(scope);


            var item = new MemoryItem
            {
                Key = key,
                Value = value,
                StoredAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                Tags = new Dictionary<string, string>()
            };


            storage[key] = item;


            _logger.LogDebug(
                "Stored item in {Scope} memory: {Key} (expires: {Expires})",
                scope,
                key,
                expiresAt?.ToString() ?? "never");


            return Task.CompletedTask;
        }


        /// <summary>
        /// Retrieves a value from memory.
        /// </summary>
        /// <typeparam name="T">The type of value to retrieve.</typeparam>
        /// <param name="key">The storage key.</param>
        /// <param name="scope">The memory scope.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the retrieved value, or default(T) if not found or expired.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is null.
        /// </exception>
        /// <remarks>
        /// Retrieval checks expiration automatically. Expired items are treated
        /// as not found and are removed from storage.
        /// 
        /// Type safety: The method attempts to cast the stored value to T.
        /// If the cast fails, default(T) is returned and a warning is logged.
        /// 
        /// Performance: O(1) lookup time using hash table storage.
        /// </remarks>
        public Task<T> RetrieveAsync<T>(string key, MemoryScope scope)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));


            var storage = GetStorage(scope);


            if (!storage.TryGetValue(key, out var item))
            {
                _logger.LogDebug("Item not found in {Scope} memory: {Key}", scope, key);
                return Task.FromResult(default(T));
            }


            // Check expiration
            if (item.ExpiresAt.HasValue && item.ExpiresAt.Value < DateTime.UtcNow)
            {
                _logger.LogDebug("Item expired in {Scope} memory: {Key}", scope, key);
                storage.TryRemove(key, out _);
                return Task.FromResult(default(T));
            }


            // Try to cast to requested type
            try
            {
                var value = (T)item.Value;
                _logger.LogDebug("Retrieved item from {Scope} memory: {Key}", scope, key);
                return Task.FromResult(value);
            }
            catch (InvalidCastException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to cast stored value to {Type} for key {Key} in {Scope} memory",
                    typeof(T).Name,
                    key,
                    scope);
                return Task.FromResult(default(T));
            }
        }


        /// <summary>
        /// Searches for items in memory by query string.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <param name="scope">The memory scope to search.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// a collection of matching memory items.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="query"/> is null.
        /// </exception>
        /// <remarks>
        /// This simple implementation performs substring matching on keys.
        /// For production use, consider:
        /// - Full-text search with Elasticsearch
        /// - Semantic search with vector embeddings
        /// - Tag-based search
        /// - Fuzzy matching
        /// 
        /// The search is case-insensitive and matches anywhere in the key.
        /// 
        /// Performance: O(n) where n is the number of items in the scope.
        /// For large datasets, implement indexing.
        /// </remarks>
        public Task<IEnumerable<IMemoryItem>> SearchAsync(string query, MemoryScope scope)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException(nameof(query));


            var storage = GetStorage(scope);
            var now = DateTime.UtcNow;


            var results = storage.Values
                .Where(item =>
                    // Match query in key
                    item.Key.Contains(query, StringComparison.OrdinalIgnoreCase) &&
                    // Not expired
                    (!item.ExpiresAt.HasValue || item.ExpiresAt.Value >= now))
                .Cast<IMemoryItem>()
                .ToList();


            _logger.LogDebug(
                "Search in {Scope} memory for '{Query}' found {Count} results",
                scope,
                query,
                results.Count);


            return Task.FromResult<IEnumerable<IMemoryItem>>(results);
        }


        /// <summary>
        /// Deletes an item from memory.
        /// </summary>
        /// <param name="key">The storage key.</param>
        /// <param name="scope">The memory scope.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is null.
        /// </exception>
        /// <remarks>
        /// Deletion is idempotent - deleting a non-existent key succeeds silently.
        /// 
        /// Use deletion for:
        /// - Removing sensitive data
        /// - Clearing user preferences on logout
        /// - Cleaning up after operations
        /// - Implementing "forget me" functionality
        /// </remarks>
        public Task DeleteAsync(string key, MemoryScope scope)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));


            var storage = GetStorage(scope);

            if (storage.TryRemove(key, out _))
            {
                _logger.LogDebug("Deleted item from {Scope} memory: {Key}", scope, key);
            }
            else
            {
                _logger.LogDebug("Item not found for deletion in {Scope} memory: {Key}", scope, key);
            }


            return Task.CompletedTask;
        }


        /// <summary>
        /// Gets the storage dictionary for a scope.
        /// </summary>
        private ConcurrentDictionary<string, MemoryItem> GetStorage(MemoryScope scope)
        {
            return scope switch
            {
                MemoryScope.Session => _sessionMemory,
                MemoryScope.User => _userMemory,
                MemoryScope.Global => _globalMemory,
                MemoryScope.Tool => _toolMemory,
                _ => throw new ArgumentException($"Unknown memory scope: {scope}", nameof(scope))
            };
        }


        /// <summary>
        /// Gets the expiration time for a scope.
        /// </summary>
        private DateTime? GetExpirationTime(MemoryScope scope)
        {
            return scope switch
            {
                MemoryScope.Session => DateTime.UtcNow.Add(SessionExpiration),
                MemoryScope.User => DateTime.UtcNow.Add(UserExpiration),
                MemoryScope.Global => null, // Never expires
                MemoryScope.Tool => null, // Managed by tool
                _ => null
            };
        }


        /// <summary>
        /// Starts a background task to periodically clean up expired items.
        /// </summary>
        /// <remarks>
        /// Runs every 5 minutes to remove expired items and free memory.
        /// This prevents memory leaks from accumulated expired data.
        /// 
        /// In production, consider:
        /// - Using a hosted service or background worker
        /// - Implementing more sophisticated cleanup strategies
        /// - Monitoring memory usage and adjusting cleanup frequency
        /// </remarks>
        private void StartExpirationCleanup()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromMinutes(5));
                        CleanupExpiredItems();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during memory cleanup");
                    }
                }
            });
        }


        /// <summary>
        /// Removes expired items from all memory scopes.
        /// </summary>
        private void CleanupExpiredItems()
        {
            var now = DateTime.UtcNow;
            var totalRemoved = 0;


            foreach (var scope in new[] { MemoryScope.Session, MemoryScope.User })
            {
                var storage = GetStorage(scope);
                var expiredKeys = storage
                    .Where(kvp => kvp.Value.ExpiresAt.HasValue && kvp.Value.ExpiresAt.Value < now)
                    .Select(kvp => kvp.Key)
                    .ToList();


                foreach (var key in expiredKeys)
                {
                    if (storage.TryRemove(key, out _))
                    {
                        totalRemoved++;
                    }
                }
            }


            if (totalRemoved > 0)
            {
                _logger.LogInformation("Cleaned up {Count} expired memory items", totalRemoved);
            }
        }
    }


}
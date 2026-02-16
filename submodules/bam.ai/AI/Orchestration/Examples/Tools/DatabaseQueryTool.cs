using Bam.AI.Orchestration.Tools;
using Microsoft.Extensions.Logging;


namespace Bam.AI.Orchestration.Examples
{
    /// <summary>
    /// Example tool implementation for database queries.
    /// </summary>
    /// <remarks>
    /// This is a mock implementation with in-memory data.
    /// In production, this would:
    /// - Connect to real databases (SQL Server, PostgreSQL, etc.)
    /// - Use parameterized queries to prevent SQL injection
    /// - Implement connection pooling
    /// - Enforce read-only access for safety
    /// - Support multiple database types
    /// - Handle timeouts and retries
    /// </remarks>
    public class DatabaseQueryTool : IToolImplementation
    {
        private readonly ILogger<DatabaseQueryTool> _logger;


        // Mock database tables
        private static readonly List<Dictionary<string, object>> CustomersTable = new()
        {
            new() { ["id"] = 1, ["name"] = "Alice Johnson", ["email"] = "alice@example.com", ["country"] = "USA" },
            new() { ["id"] = 2, ["name"] = "Bob Smith", ["email"] = "bob@example.com", ["country"] = "Canada" },
            new() { ["id"] = 3, ["name"] = "Charlie Brown", ["email"] = "charlie@example.com", ["country"] = "UK" },
            new() { ["id"] = 4, ["name"] = "Diana Prince", ["email"] = "diana@example.com", ["country"] = "USA" },
        };


        private static readonly List<Dictionary<string, object>> OrdersTable = new()
        {
            new() { ["id"] = 101, ["customer_id"] = 1, ["product"] = "Laptop", ["amount"] = 1200.00 },
            new() { ["id"] = 102, ["customer_id"] = 1, ["product"] = "Mouse", ["amount"] = 25.00 },
            new() { ["id"] = 103, ["customer_id"] = 2, ["product"] = "Keyboard", ["amount"] = 75.00 },
            new() { ["id"] = 104, ["customer_id"] = 3, ["product"] = "Monitor", ["amount"] = 300.00 },
        };


        public DatabaseQueryTool(ILogger<DatabaseQueryTool> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <summary>
        /// Executes the database query.
        /// </summary>
        public async Task<object> ExecuteAsync(
            IDictionary<string, object> parameters,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing database query tool");


            // Extract parameters
            if (!parameters.TryGetValue("query", out var queryObj))
            {
                throw new ToolExecutionException(
                    "database_query",
                    "Missing required parameter: query");
            }


            if (!parameters.TryGetValue("database", out var databaseObj))
            {
                throw new ToolExecutionException(
                    "database_query",
                    "Missing required parameter: database");
            }


            var query = queryObj.ToString();
            var database = databaseObj.ToString();


            _logger.LogDebug("Executing query on {Database}: {Query}", database, query);


            // Validate query is SELECT only (read-only)
            if (!query.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                throw new ToolExecutionException(
                    "database_query",
                    "Only SELECT queries are allowed for safety");
            }


            // Simulate database delay
            await Task.Delay(200, cancellationToken);


            try
            {
                // Simple mock query execution
                var results = ExecuteMockQuery(query, database);


                _logger.LogInformation(
                    "Query executed successfully: {RowCount} rows returned",
                    results.Count);


                return new
                {
                    database = database,
                    query = query,
                    rows = results,
                    count = results.Count,
                    timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Query execution failed");
                throw new ToolExecutionException(
                    "database_query",
                    $"Query execution failed: {ex.Message}",
                    ex);
            }
        }


        /// <summary>
        /// Executes a mock query against in-memory data.
        /// </summary>
        /// <remarks>
        /// This is a very simplified query parser for demonstration.
        /// Production code should use a real database connection.
        /// </remarks>
        private List<Dictionary<string, object>> ExecuteMockQuery(string query, string database)
        {
            var queryLower = query.ToLowerInvariant();


            // Determine which table to query
            List<Dictionary<string, object>> table;
            if (queryLower.Contains("customers"))
            {
                table = CustomersTable;
            }
            else if (queryLower.Contains("orders"))
            {
                table = OrdersTable;
            }
            else
            {
                throw new ToolExecutionException(
                    "database_query",
                    $"Unknown table in query");
            }


            // Simple filtering (very basic, just for demo)
            var results = table.ToList();


            if (queryLower.Contains("where"))
            {
                // Extract simple WHERE conditions
                if (queryLower.Contains("country = 'usa'"))
                {
                    results = results.Where(r => r["country"].ToString() == "USA").ToList();
                }
                else if (queryLower.Contains("amount >"))
                {
                    results = results.Where(r =>
                        r.ContainsKey("amount") && (double)r["amount"] > 100).ToList();
                }
            }


            // Limit results
            if (queryLower.Contains("limit"))
            {
                var limitMatch = System.Text.RegularExpressions.Regex.Match(
                    queryLower,
                    @"limit\s+(\d+)");
                if (limitMatch.Success)
                {
                    var limit = int.Parse(limitMatch.Groups[1].Value);
                    results = results.Take(limit).ToList();
                }
            }


            return results;
        }
    }
}


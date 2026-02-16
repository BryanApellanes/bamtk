using Bam.AI.Orchestration.Tools;
using Microsoft.Extensions.Logging;
using System.Data;


namespace Bam.AI.Orchestration.Examples
{
    /// <summary>
    /// Example tool implementation for mathematical calculations.
    /// </summary>
    /// <remarks>
    /// This tool evaluates mathematical expressions safely.
    /// It uses DataTable.Compute for simplicity, but production code should:
    /// - Use a proper expression parser (e.g., NCalc, MathNet)
    /// - Implement timeout protection
    /// - Validate expressions for security
    /// - Support more advanced math functions
    /// </remarks>
    public class CalculatorTool : IToolImplementation
    {
        private readonly ILogger<CalculatorTool> _logger;


        public CalculatorTool(ILogger<CalculatorTool> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <summary>
        /// Executes the calculation.
        /// </summary>
        public Task<object> ExecuteAsync(
            IDictionary<string, object> parameters,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing calculator tool");


            // Extract expression
            if (!parameters.TryGetValue("expression", out var expressionObj))
            {
                throw new ToolExecutionException(
                    "calculator",
                    "Missing required parameter: expression");
            }


            var expression = expressionObj.ToString();


            _logger.LogDebug("Evaluating expression: {Expression}", expression);


            try
            {
                // Validate expression (basic validation)
                if (string.IsNullOrWhiteSpace(expression))
                {
                    throw new ArgumentException("Expression cannot be empty");
                }


                // Check for potentially dangerous operations
                var dangerousPatterns = new[] { "exec", "eval", "system", "process", "file" };
                if (dangerousPatterns.Any(p => expression.Contains(p, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ToolExecutionException(
                        "calculator",
                        "Expression contains potentially unsafe operations");
                }


                // Evaluate expression using DataTable.Compute
                // Note: This is simple but limited. Production code should use a proper parser.
                var table = new DataTable();
                var result = table.Compute(expression, string.Empty);


                _logger.LogInformation(
                    "Calculation successful: {Expression} = {Result}",
                    expression,
                    result);


                return Task.FromResult<object>(new
                {
                    expression = expression,
                    result = Convert.ToDouble(result),
                    timestamp = DateTime.UtcNow
                });
            }
            catch (DivideByZeroException)
            {
                throw new ToolExecutionException(
                    "calculator",
                    "Division by zero is not allowed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Calculation failed for expression: {Expression}", expression);
                throw new ToolExecutionException(
                    "calculator",
                    $"Failed to evaluate expression: {ex.Message}",
                    ex);
            }
        }
    }
}

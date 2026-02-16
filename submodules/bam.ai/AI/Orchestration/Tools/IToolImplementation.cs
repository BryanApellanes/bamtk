namespace Bam.AI.Orchestration.Tools
{

    /// <summary>
    /// Interface that all tool implementations must implement.
    /// </summary>
    /// <remarks>
    /// Tool implementations are the actual executable code that performs the
    /// tool's function. They are separate from tool descriptors (metadata) to
    /// allow for:
    /// - Dynamic loading and unloading
    /// - Testing with mock implementations
    /// - Version management
    /// - Hot-swapping implementations
    /// </remarks>
    public interface IToolImplementation
    {
        /// <summary>
        /// Executes the tool with the given parameters.
        /// </summary>
        /// <param name="parameters">The input parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result
        /// contains the tool's output data.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is cancelled.
        /// </exception>
        /// <exception cref="ToolExecutionException">
        /// Thrown when the tool encounters an error during execution.
        /// </exception>
        Task<object> ExecuteAsync(
            IDictionary<string, object> parameters,
            CancellationToken cancellationToken);
    }
}
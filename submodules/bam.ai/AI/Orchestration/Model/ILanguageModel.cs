using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bam.AI.Orchestration.Model
{
    /// <summary>
    /// Interface to the underlying LLM provider
    /// </summary>
    public interface ILanguageModel
    {
        Task<IModelResponse> GenerateAsync(
            IModelRequest request,
            CancellationToken cancellationToken = default);

        Task<IModelResponse> GenerateStreamingAsync(
            IModelRequest request,
            IProgress<string> progress,
            CancellationToken cancellationToken = default);

        IModelCapabilities Capabilities { get; }
    }
}
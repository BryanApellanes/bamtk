using Bam.AI.Orchestration.Tools;

namespace Bam.AI.Orchestration.Model
{
    /// <summary>
    /// Concrete implementation of a model request.
    /// </summary>
    /// <remarks>
    /// Encapsulates all information needed to make a request to a language model,
    /// including the conversation history, system prompt, available tools, and
    /// generation parameters.
    /// 
    /// Thread Safety: This class is immutable after construction and is thread-safe.
    /// </remarks>
    public class ModelRequest : IModelRequest
    {
        /// <summary>
        /// Gets the system prompt that defines the model's behavior.
        /// </summary>
        /// <value>
        /// Instructions for the model about its role, capabilities, and constraints.
        /// This is typically set once for the entire conversation.
        /// </value>
        public string SystemPrompt { get; }


        /// <summary>
        /// Gets the conversation messages.
        /// </summary>
        /// <value>
        /// An ordered sequence of messages representing the conversation history
        /// and current user input.
        /// </value>
        public IEnumerable<IMessage> Messages { get; }


        /// <summary>
        /// Gets the tools available for the model to use.
        /// </summary>
        /// <value>
        /// Tool descriptors that the model can invoke during generation.
        /// Empty if no tools are available.
        /// </value>
        public IEnumerable<IToolDescriptor> AvailableTools { get; }


        /// <summary>
        /// Gets the generation parameters.
        /// </summary>
        /// <value>
        /// Parameters controlling the model's output such as temperature,
        /// max tokens, and sampling settings.
        /// </value>
        public IModelParameters Parameters { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ModelRequest"/> class.
        /// </summary>
        /// <param name="systemPrompt">The system prompt.</param>
        /// <param name="messages">The conversation messages.</param>
        /// <param name="availableTools">The available tools.</param>
        /// <param name="parameters">The generation parameters.</param>
        public ModelRequest(
            string systemPrompt,
            IEnumerable<IMessage> messages,
            IEnumerable<IToolDescriptor> availableTools,
            IModelParameters parameters)
        {
            SystemPrompt = systemPrompt ?? string.Empty;
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
            AvailableTools = availableTools ?? Enumerable.Empty<IToolDescriptor>();
            Parameters = parameters ?? new ModelParameters();
        }
    }
}



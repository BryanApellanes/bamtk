namespace Bam.AI.Orchestration.Tools
{

    /// <summary>
    /// Concrete implementation of a tool descriptor.
    /// </summary>
    /// <remarks>
    /// Describes a tool's interface, capabilities, and requirements.
    /// This is metadata about a tool, not the tool implementation itself.
    /// 
    /// Tool implementations are separate and injected at runtime based on
    /// the tool ID. This allows for:
    /// - Dynamic tool loading
    /// - Hot-swapping implementations
    /// - Mock implementations for testing
    /// </remarks>
    public class ToolDescriptor : IToolDescriptor
    {
        /// <summary>
        /// Gets the unique identifier for this tool.
        /// </summary>
        /// <value>
        /// A unique string ID (e.g., "weather_api", "database_query").
        /// Should be lowercase with underscores for consistency.
        /// </value>
        public string Id { get; }


        /// <summary>
        /// Gets the human-readable name of the tool.
        /// </summary>
        /// <value>
        /// A display name for the tool (e.g., "Weather API", "Database Query").
        /// </value>
        public string Name { get; }


        /// <summary>
        /// Gets a description of what the tool does.
        /// </summary>
        /// <value>
        /// A clear, concise description that helps the agent understand when
        /// to use this tool. Should include:
        /// - What the tool does
        /// - What inputs it expects
        /// - What outputs it produces
        /// - Any important constraints or limitations
        /// </value>
        public string Description { get; }


        /// <summary>
        /// Gets the schema defining the tool's input parameters.
        /// </summary>
        public IToolSchema InputSchema { get; }


        /// <summary>
        /// Gets the schema defining the tool's output format.
        /// </summary>
        public IToolSchema OutputSchema { get; }


        /// <summary>
        /// Gets the capabilities provided by this tool.
        /// </summary>
        /// <value>
        /// A flags enum indicating what operations this tool can perform.
        /// Multiple capabilities can be combined (e.g., Read | Search).
        /// </value>
        public ToolCapability Capabilities { get; }


        /// <summary>
        /// Gets whether this tool requires user authorization.
        /// </summary>
        /// <value>
        /// True if the tool accesses protected resources or performs sensitive
        /// operations requiring explicit user permission; otherwise, false.
        /// </value>
        public bool RequiresAuthorization { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ToolDescriptor"/> class.
        /// </summary>
        /// <param name="id">The unique tool identifier.</param>
        /// <param name="name">The display name.</param>
        /// <param name="description">The tool description.</param>
        /// <param name="inputSchema">The input schema.</param>
        /// <param name="outputSchema">The output schema.</param>
        /// <param name="capabilities">The tool capabilities.</param>
        /// <param name="requiresAuthorization">Whether authorization is required.</param>
        public ToolDescriptor(
            string id,
            string name,
            string description,
            IToolSchema inputSchema,
            IToolSchema outputSchema,
            ToolCapability capabilities,
            bool requiresAuthorization = false)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id cannot be null or empty", nameof(id));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));


            Id = id;
            Name = name;
            Description = description ?? string.Empty;
            InputSchema = inputSchema ?? throw new ArgumentNullException(nameof(inputSchema));
            OutputSchema = outputSchema ?? throw new ArgumentNullException(nameof(outputSchema));
            Capabilities = capabilities;
            RequiresAuthorization = requiresAuthorization;
        }
    }
}
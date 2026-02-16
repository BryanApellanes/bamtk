namespace Bam.AI.Orchestration.Tools
{

    /// <summary>
    /// Concrete implementation of a tool schema.
    /// </summary>
    /// <remarks>
    /// Defines the structure of tool inputs or outputs using a schema language.
    /// Supports JSON Schema by default, but can be extended to support other
    /// schema formats (XML Schema, Protocol Buffers, etc.).
    /// </remarks>
    public class ToolSchema : IToolSchema
    {
        /// <summary>
        /// Gets the schema type identifier.
        /// </summary>
        /// <value>
        /// The schema language used (e.g., "json-schema", "xml-schema").
        /// Defaults to "json-schema" for most tools.
        /// </value>
        public string SchemaType { get; }


        /// <summary>
        /// Gets the schema definition as a string.
        /// </summary>
        /// <value>
        /// The complete schema in the format specified by <see cref="SchemaType"/>.
        /// For JSON Schema, this should be valid JSON.
        /// </value>
        public string SchemaDefinition { get; }


        /// <summary>
        /// Gets the parameters defined in the schema.
        /// </summary>
        /// <value>
        /// A parsed representation of the schema's parameters for easier
        /// programmatic access without parsing the schema definition.
        /// </value>
        public IEnumerable<IParameter> Parameters { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ToolSchema"/> class.
        /// </summary>
        /// <param name="schemaType">The schema type.</param>
        /// <param name="schemaDefinition">The schema definition.</param>
        /// <param name="parameters">The parameters.</param>
        public ToolSchema(
            string schemaType,
            string schemaDefinition,
            IEnumerable<IParameter> parameters)
        {
            SchemaType = schemaType ?? "json-schema";
            SchemaDefinition = schemaDefinition ?? throw new ArgumentNullException(nameof(schemaDefinition));
            Parameters = parameters ?? Enumerable.Empty<IParameter>();
        }
    }
}
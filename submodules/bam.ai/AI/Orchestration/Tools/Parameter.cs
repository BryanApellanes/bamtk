namespace Bam.AI.Orchestration.Tools
{

    /// <summary>
    /// Concrete implementation of a parameter definition.
    /// </summary>
    public class Parameter : IParameter
    {
        /// <summary>
        /// Gets the parameter name.
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// Gets the parameter type.
        /// </summary>
        /// <value>
        /// The data type (e.g., "string", "number", "boolean", "object", "array").
        /// </value>
        public string Type { get; }


        /// <summary>
        /// Gets whether this parameter is required.
        /// </summary>
        public bool Required { get; }


        /// <summary>
        /// Gets the default value for this parameter.
        /// </summary>
        /// <value>
        /// The default value used if the parameter is not provided.
        /// Null if no default is specified.
        /// </value>
        public object DefaultValue { get; }


        /// <summary>
        /// Gets the parameter description.
        /// </summary>
        public string Description { get; }


        /// <summary>
        /// Gets the allowed values for this parameter.
        /// </summary>
        /// <value>
        /// An enumeration of valid values, or empty if any value is allowed.
        /// Useful for enum-like parameters.
        /// </value>
        public IEnumerable<string> AllowedValues { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="Parameter"/> class.
        /// </summary>
        public Parameter(
            string name,
            string type,
            bool required,
            object defaultValue = null,
            string description = null,
            IEnumerable<string> allowedValues = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Required = required;
            DefaultValue = defaultValue;
            Description = description ?? string.Empty;
            AllowedValues = allowedValues ?? Enumerable.Empty<string>();
        }
    }
}
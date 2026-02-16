namespace Bam.AI.Orchestration.Memory
{

    /// <summary>
    /// Concrete implementation of a memory item.
    /// </summary>
    public class MemoryItem : IMemoryItem
    {
        /// <summary>
        /// Gets the storage key.
        /// </summary>
        public string Key { get; set; }


        /// <summary>
        /// Gets the stored value.
        /// </summary>
        public object Value { get; set; }


        /// <summary>
        /// Gets when the item was stored.
        /// </summary>
        public DateTime StoredAt { get; set; }


        /// <summary>
        /// Gets when the item expires.
        /// </summary>
        /// <value>
        /// Null if the item never expires (Global and Tool scopes).
        /// </value>
        public DateTime? ExpiresAt { get; set; }


        /// <summary>
        /// Gets tags for categorization and search.
        /// </summary>
        /// <value>
        /// A dictionary of tag names to values. Useful for:
        /// - Categorization: {"category": "user_preference"}
        /// - Ownership: {"user_id": "123"}
        /// - Purpose: {"purpose": "caching"}
        /// - Custom metadata: {"source": "api_response"}
        /// </value>
        public IDictionary<string, string> Tags { get; set; }
    }
}
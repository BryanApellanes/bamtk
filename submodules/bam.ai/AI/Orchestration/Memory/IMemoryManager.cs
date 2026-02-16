using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bam.AI.Orchestration.Memory
{
    /// <summary>
    /// Manages agent memory and knowledge persistence
    /// </summary>
    public interface IMemoryManager
    {
        Task StoreAsync(string key, object value, MemoryScope scope);
        Task<T> RetrieveAsync<T>(string key, MemoryScope scope);
        Task<IEnumerable<IMemoryItem>> SearchAsync(string query, MemoryScope scope);
        Task DeleteAsync(string key, MemoryScope scope);
    }
}
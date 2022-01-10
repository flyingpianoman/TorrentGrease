using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Crosscutting
{
    public static class AsyncLinqExtensions
    {
        public static async Task<IEnumerable<T>> AwaitAllAsync<T>(this IEnumerable<Task<T>> tasks)
        {
            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public static async Task<List<T>> AwaitAllToListAsync<T>(this IEnumerable<Task<T>> tasks)
        {
            return (await tasks.AwaitAllAsync().ConfigureAwait(false))
                .ToList();
        }
    }
}

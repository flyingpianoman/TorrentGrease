using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Pages.Components
{
    internal static class ComponentExtensions
    {
        internal static async ValueTask InitializeAsync<T>(this IEnumerable<IComponent<T>> components)
        {
            foreach (var component in components)
            {
                await component.InitializeAsync().ConfigureAwait(false);
            }
        }
    }
}

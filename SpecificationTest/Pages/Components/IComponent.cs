using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Pages.Components
{
    interface IComponent
    {
        Task InitializeAsync();
    }
}

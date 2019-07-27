using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Pages
{
    internal interface IPage
    {
        Task InitializeAsync();
    }
}

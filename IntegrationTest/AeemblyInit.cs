using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestUtils;

namespace IntegrationTest
{
    [TestClass]
    public class AeemblyInit
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            var services = DIContainer.Default;
            services.RegisterDockerClient();
        }
    }
}

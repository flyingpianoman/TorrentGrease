using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace TorrentGrease.Shared.ServiceContracts
{
    public static class ReflectionHelper
    {
        public static IEnumerable<Type> GetGrpcServiceInterfaceTypes()
        {
            return typeof(ReflectionHelper).Assembly
                .GetTypes()
                .Where(t => t.Namespace == typeof(ReflectionHelper).Namespace
                         && t.CustomAttributes.Any(attr => attr.AttributeType == typeof(ServiceContractAttribute)))
                .ToArray();
        }
    }
}

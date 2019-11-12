using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TorrentGrease.Shared.ServiceContracts
{
    [ServiceContract(Name = nameof(IPolicyService))]
    public interface IPolicyService
    {
        ValueTask<IEnumerable<Shared.Policy>> GetAllPoliciesAsync();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TorrentGrease.Data.Repositories;
using TorrentGrease.Shared;
using TorrentGrease.Shared.ServiceContracts;

namespace TorrentGrease.Server.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly IPolicyRepository _policyRepository;

        public PolicyService(IPolicyRepository policyRepository)
        {
            _policyRepository = policyRepository ?? throw new ArgumentNullException(nameof(policyRepository));
        }

        public async ValueTask<IEnumerable<Policy>> GetAllPoliciesAsync()
        {
            return await _policyRepository.GetAllAsync().ConfigureAwait(false);
        }

        public ValueTask Test()
        {
            Console.WriteLine("============================================== test ==================================");
            Console.WriteLine("============================================== test ==================================");
            Console.WriteLine("============================================== test ==================================");
            Console.WriteLine("============================================== test ==================================");
            Console.WriteLine("============================================== test ==================================");
            return new ValueTask(Task.CompletedTask);
        }
    }
}

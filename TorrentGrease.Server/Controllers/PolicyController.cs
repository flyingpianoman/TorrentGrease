using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TorrentGrease.Data.Repositories;

namespace TorrentGrease.Server.Controllers
{
    [Route("api/[controller]")]
    public class PolicyController : Controller
    {
        private readonly IPolicyRepository _policyRepository;

        public PolicyController(IPolicyRepository policyRepository)
        {
            _policyRepository = policyRepository ?? throw new ArgumentNullException(nameof(policyRepository));
        }

        [HttpGet("[action]")]
        public async Task<IEnumerable<Shared.Policy>> All()
        {
            return await _policyRepository.GetAllAsync().ConfigureAwait(false);
        }
    }
}

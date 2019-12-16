using System.Collections.Generic;
using System.Threading.Tasks;
using TorrentGrease.Shared;

namespace TorrentGrease.Data.Repositories
{
    public interface IPolicyRepository : IRepository
    {
        Task<ICollection<Policy>> GetAllAsync();
    }
}
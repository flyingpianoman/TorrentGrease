using System;
using System.Threading.Tasks;

namespace TorrentGrease.Hangfire
{
    public interface IAsyncJob
    {
        Task ExecuteAsync();
    }
}

using System;

namespace TorrentGrease.Hangfire
{
    public interface IJob
    {
        void Execute();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TorrentGrease.Shared;

namespace TorrentGrease.Data
{
    public interface ITorrentGreaseDbContext
    {
        DatabaseFacade Database { get; }
        DbSet<Action> Actions { get; set; }
        DbSet<Condition> Conditions { get; set; }
        DbSet<Policy> Policies { get; set; }
        DbSet<Tracker> Trackers { get; set; }
    }
}
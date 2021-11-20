using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TorrentGrease.Shared.ServiceContracts.FileManagement;

namespace TorrentGrease.Client.Models
{
    public class MappedDirectoryRow : MappedDirectory
    {
        public string TorrentClientDirsCsv { get; set; }
    }
}

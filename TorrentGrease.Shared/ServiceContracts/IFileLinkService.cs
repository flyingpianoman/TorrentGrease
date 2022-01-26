using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TorrentGrease.Shared.ServiceContracts.FileLink;

namespace TorrentGrease.Shared.ServiceContracts
{
    [ServiceContract(Name = nameof(IFileLinkService))]
    public interface IFileLinkService
    {
        Task<IEnumerable<FileLinkCandidate>> ScanForFilesToLinkAsync(ScanForPossibleFileLinksRequest request);

        ValueTask CreateFileLinksAsync(IEnumerable<FileLinkToCreate> fileLinksToCreate);
    }
}

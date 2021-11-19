using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TorrentGrease.Shared.ServiceContracts.FileManagement;

namespace TorrentGrease.Shared.ServiceContracts
{
    [ServiceContract(Name = nameof(IFileManagementService))]
    public interface IFileManagementService
    {
        Task<IEnumerable<FileRemovalCandidate>> ScanForFilesToRemoveAsync(ScanForFilesToRemoveRequest request);

        ValueTask RemoveFilesAsync(IEnumerable<string> filesToRemove);
    }
}

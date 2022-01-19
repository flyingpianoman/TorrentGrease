using MonoTorrent;
using MonoTorrent.BEncoding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TestUtils.Torrent
{
    public class TorrentFileHelper
    {
        public async ValueTask CreateTextFileAsync(string fileLoc, int bytes, char charToUser)
        {
            using var sw = File.CreateText(fileLoc);
            var remainingBytes = bytes; //Each char = 1 byte

            while (remainingBytes > 0)
            {
                var charsToWriteThisIteration = remainingBytes < 1024
                    ? remainingBytes
                    : 1024; //Write 1 KB at a time

                await sw.WriteAsync(new string(charToUser, charsToWriteThisIteration));
                remainingBytes -= charsToWriteThisIteration;
            }
        }

        public ValueTask<ExistingTorrentFile> ReadTorrentAsync(string torrentFileLocation)
        {
            var torrent = MonoTorrent.Torrent.Load(torrentFileLocation);
            var tf = new ExistingTorrentFile
            {
                IsPrivate = torrent.IsPrivate,
                Name = torrent.Name,
                InnerTorrentFiles = torrent.Files.Select(f => new InnerTorrentFileInfo { FileLocInTorrent = f.Path, FileSizeInBytes = f.Length }).ToArray(),
                TrackerAnnounceUrl = torrent.AnnounceUrls.Single().Single() //only 1 announce url supported atm
            };

            return new ValueTask<ExistingTorrentFile>(tf);
        }

        public async ValueTask CreateTorrentAsync(string torrentFileLocation, NewTorrentFile torrentFile)
        {
            var tc = new TorrentCreator
            {
                CreatedBy = "TorrentGrease",
                Private = torrentFile.IsPrivate
            };

            foreach (var announceUrl in torrentFile.TrackerAnnounceUrls)
            {
                tc.Announces.Add(new List<string> { announceUrl });
            }

            await tc.CreateAsync(new CustomTorrentFileSource(torrentFile.Name, torrentFile.FileMappings), torrentFileLocation);
        }

        private class CustomTorrentFileSource : ITorrentFileSource
        {
            public CustomTorrentFileSource(string torrentName, IEnumerable<CreateTorrentFileMapping> fileMappings)
            {
                TorrentName = torrentName;
                Files = fileMappings
                    .Select(f => new MonoTorrent.FileMapping(f.FileLocOnDisk, f.FileLocInTorrent))
                    .ToArray();
            }

            public IEnumerable<MonoTorrent.FileMapping> Files { get; set; }

            public string TorrentName { get; set; }
        }
    }
}

using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpecificationTest.Crosscutting
{
    static class ArchiveHelper
    {
        public static void ExtractSingleFileFromTar(Stream tarStream, string filePath)
        {
            var first = true;
            tarStream.Position = 0;
            using var reader = ReaderFactory.Open(tarStream, new ReaderOptions { LeaveStreamOpen = true });
            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory)
                {
                    if (!first)
                    {
                        throw new InvalidOperationException("More than one file in tar");
                    }
                    first = false;
                    reader.WriteEntryToFile(filePath, new ExtractionOptions { Overwrite = true });
                }
            }
        }

        public static MemoryStream CreateTarStream(string sourceFile, string fileNameInTar)
        {
            var tarStream = new MemoryStream();
            using var writer = WriterFactory.Open(tarStream, ArchiveType.Tar, new WriterOptions(CompressionType.None)
            {
                LeaveStreamOpen = true
            });

            writer.Write(fileNameInTar, sourceFile);
            return tarStream;
        }
    }
}

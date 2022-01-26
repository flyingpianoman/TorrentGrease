using System.IO;

namespace TorrentGrease.Server.CrossCutting
{
    public static class PathHelper
    {
        public static string EnsurePathEndsWithASeperator(string torrentClientDir)
        {
            if (!torrentClientDir.EndsWith(Path.DirectorySeparatorChar) &&
                !torrentClientDir.EndsWith(Path.AltDirectorySeparatorChar))
            {
                return torrentClientDir + Path.DirectorySeparatorChar;
            }

            return torrentClientDir;
        }
    }
}

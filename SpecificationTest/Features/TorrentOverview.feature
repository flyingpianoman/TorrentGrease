Feature: Torrent overview
	In order to see all torrents
	As a torrent maintainer
	I want to see all torrents in my torrentclient

Scenario: View empty torrents page
	When I navigate to the torrent overview
	Then I see an overview containing 0 torrents

Scenario: View filled torrents page
	Given the following torrents are staged
		| Name                | Location             | TrackerAnnounceUrl1             | TrackerAnnounceUrl2                      | TorrentFile1Path | TorrentFile1SizeInKB | TorrentFile2Path | TorrentFile2SizeInKB |
		| TorrentA            | /downloads/completed | http://my-tracker:6969/announce |                                          | file1.txt        | 10240                |                  |                      |
		| TorrentWithTwoFiles | /downloads/completed | http://my-tracker:6969/announce | http://nonexisting-tracker:12345/annouce | file1.txt        | 10240                | file2.txt        | 10240                |
	When I navigate to the torrent overview
	Then I see an overview of the following torrents
		| Name                | GBsOnDisk | TotalSizeInGB | TotalUploadedInGB | LocationOnDisk                           | TrackerAnnounceUrl1 | TrackerAnnounceUrl2       |
		| TorrentA            | 0         | 0.01            | 0                 | /downloads/completed                     | my-tracker:6969     |                           |
		| TorrentWithTwoFiles | 0         | 0.02            | 0                 | /downloads/completed/TorrentWithTwoFiles | my-tracker:6969     | nonexisting-tracker:12345 |

#| Name                               | Location             | TrackerAnnounceUrl1             | TorrentFile1Path    | TorrentFile1SizeInKB | TorrentFile2Path    | TorrentFile2SizeInKB |
#| TorrentWithOneFile                 | /downloads/completed | http://my-tracker:6969/announce | file1.txt           | 128                  |                     |                      |
#| TorrentWithTwoFiles                | /downloads/completed | http://my-tracker:6969/announce | file1.txt           | 128                  | file2.txt           | 256                  |
#| TorrentWithTwoFilesInADir          | /downloads/completed | http://my-tracker:6969/announce | dir1/dir2/file1.txt | 128                  | dir1/dir2/file2.txt | 256                  |
#| TorrentWithTwoFilesInDifferentDirs | /downloads/completed | http://my-tracker:6969/announce | dir1/file1.txt      | 128                  | dir2/file2.txt      | 256                  |
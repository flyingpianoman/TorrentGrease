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
		| TorrentA            | /downloads/complete | http://my-tracker:6969/announce |                                          | file1.txt        | 10240                |                  |                      |
		| TorrentWithTwoFiles | /downloads/complete | http://my-tracker:6969/announce | http://nonexisting-tracker:12345/annouce | file1.txt        | 10240                | file2.txt        | 10240                |
	When I navigate to the torrent overview
	Then I see an overview of the following torrents
		| Name                | GBsOnDisk | TotalSizeInGB | TotalUploadedInGB | LocationOnDisk                           | TrackerAnnounceUrl1 | TrackerAnnounceUrl2       |
		| TorrentA            | 0         | 0.01          | 0                 | /downloads/complete                     | my-tracker:6969     |                           |
		| TorrentWithTwoFiles | 0         | 0.02          | 0                 | /downloads/complete/TorrentWithTwoFiles | my-tracker:6969     | nonexisting-tracker:12345 |
        
Scenario: Relocate torrent data
	Given the following torrents are staged
		| Name               | Location             | TrackerAnnounceUrl1             | TorrentFile1Path | TorrentFile1SizeInKB | TorrentFile2Path | TorrentFile2SizeInKB |
		| TorrentWithOneFile | /downloads/complete | http://my-tracker:6969/announce | file1.txt        | 128                  |                  |                      |
	And the data of the following torrents is sent to the torrent client
		| TorrentName        | TargetLocation      |
		| TorrentWithOneFile | /downloads/unmapped |
	And I navigate to the torrent overview
	When I open the relocate data dialog for the following torrents
		| Name               |
		| TorrentWithOneFile |
	And I scan for torrent data relocate candidates with the following paths
		| Path                 |
		| /downloads/unmapped/ |
	And I relocate the data of the following torrents and verify them afterwards
		| TorrentName        |
		| TorrentWithOneFile |
	Then I see an overview of the following torrents
		| Name               | LocationOnDisk      | GBsOnDisk |
		| TorrentWithOneFile | /downloads/unmapped | 0         |
        
Scenario: Relocate torrent data with different extensions
	Given the following torrents are staged
		| Name               | Location             | TrackerAnnounceUrl1             | TorrentFile1Path | TorrentFile1SizeInKB | TorrentFile2Path | TorrentFile2SizeInKB |
		| TorrentWithOneFile | /downloads/complete | http://my-tracker:6969/announce | file1.txt        | 128                  |                  |                      |
		| TorrentWithDifferentExtension | /downloads/complete | http://my-tracker:6969/announce | file1.pdf        | 128                  |                  |                      |
	And the data of the following torrents is sent to the torrent client
		| TorrentName        | TargetLocation      |
		| TorrentWithOneFile | /downloads/unmapped |
		| TorrentWithDifferentExtension | /downloads/unmapped |
	And I navigate to the torrent overview
	When I open the relocate data dialog for the following torrents
		| Name               |
		| TorrentWithOneFile |
		| TorrentWithDifferentExtension |
	And I scan for torrent data relocate candidates with the following paths
		| Path                 |
		| /downloads/unmapped/ |
	And I relocate the data of the following torrents and verify them afterwards
		| TorrentName        |
		| TorrentWithOneFile |
		| TorrentWithDifferentExtension |
	Then I see an overview of the following torrents
		| Name               | LocationOnDisk      | GBsOnDisk |
		| TorrentWithOneFile | /downloads/unmapped | 0         |
		| TorrentWithDifferentExtension | /downloads/unmapped | 0         |
        
Scenario: Relocate torrent data - no data found
	Given the following torrents are staged
		| Name               | Location             | TrackerAnnounceUrl1             | TorrentFile1Path | TorrentFile1SizeInKB | TorrentFile2Path | TorrentFile2SizeInKB |
		| TorrentWithOneFile | /downloads/complete | http://my-tracker:6969/announce | filenotfound.txt        | 128                  |                  |                      |
	And I navigate to the torrent overview
	When I open the relocate data dialog for the following torrents
		| Name               |
		| TorrentWithOneFile |
	And I scan for torrent data relocate candidates with the following paths
		| Path                 |
		| /downloads/ |
    Then the following torrents have no relocation options
		| TorrentName        |
		| TorrentWithOneFile |

Scenario: Relocate torrent data with multi dir torrents
	Given the following torrents are staged
		| Name                               | Location             | TrackerAnnounceUrl1             | TorrentFile1Path    | TorrentFile1SizeInKB | TorrentFile2Path    | TorrentFile2SizeInKB |
		| TorrentWithOneFile                 | /downloads/complete | http://my-tracker:6969/announce | file1.txt           | 128                  |                     |                      |
		| TorrentWithTwoFiles                | /downloads/complete | http://my-tracker:6969/announce | file1.txt           | 128                  | file2.txt           | 256                  |
		| TorrentWithTwoFilesInADir          | /downloads/complete | http://my-tracker:6969/announce | dir1/dir2/file1.txt | 128                  | dir1/dir2/file2.txt | 256                  |
		| TorrentWithTwoFilesInDifferentDirs | /downloads/complete | http://my-tracker:6969/announce | dir1/file1.txt      | 128                  | dir2/file2.txt      | 256                  |
	And the data of the following torrents is sent to the torrent client
		| TorrentName                        | TargetLocation       |
		| TorrentWithOneFile                 | /downloads/unmapped  |
		| TorrentWithTwoFiles                | /downloads/unmapped2 |
		| TorrentWithTwoFilesInADir          | /downloads/unmapped  |
		| TorrentWithTwoFilesInDifferentDirs | /downloads/unmapped  |
	And I navigate to the torrent overview
	When I open the relocate data dialog for the following torrents
		| Name                               |
		| TorrentWithOneFile                 |
		| TorrentWithTwoFiles                |
		| TorrentWithTwoFilesInADir          |
		| TorrentWithTwoFilesInDifferentDirs |
	And I scan for torrent data relocate candidates with the following paths
		| Path                  |
		| /downloads/unmapped/  |
		| /downloads/unmapped2/ |
	And I relocate the data of the following torrents and verify them afterwards
		| TorrentName                        |
		| TorrentWithOneFile                 |
		| TorrentWithTwoFiles                |
		| TorrentWithTwoFilesInADir          |
		| TorrentWithTwoFilesInDifferentDirs |
	Then I see an overview of the following torrents
		| Name                               | LocationOnDisk                                         | GBsOnDisk |
		| TorrentWithOneFile                 | /downloads/unmapped                                    | 0         |
		| TorrentWithTwoFiles                | /downloads/unmapped2/TorrentWithTwoFiles               | 0         |
		| TorrentWithTwoFilesInADir          | /downloads/unmapped/TorrentWithTwoFilesInADir          | 0         |
		| TorrentWithTwoFilesInDifferentDirs | /downloads/unmapped/TorrentWithTwoFilesInDifferentDirs | 0         |
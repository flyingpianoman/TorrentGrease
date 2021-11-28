Feature: Remove orphaned files
	In order to save on disk space
	As a torrent maintainer
	I want to be able to remove files that aren't linked to any torrents (anymore)

Background:
	Given the following torrents are staged
		| Name     | Location            | TrackerAnnounceUrl1             | TorrentFile1Path | TorrentFile1SizeInKB |
		| TorrentA | /downloads/complete | http://my-tracker:6969/announce | file1.txt        | 1                    |
	And the data of the following torrents is sent to the torrent client
		| TorrentName | TargetLocation      | VerifyTorrent |
		| TorrentA    | /downloads/complete | true          |
	And the following data is sent to the torrent client
		| FilePath                    | FileSizeInKB |
		| /downloads/orphan-file1.txt | 1            |

Scenario: Find a orphan file
	Given I navigate to file management
	And I set a minimal filesize of 0 KB
	And I set the following torrent dir mapping
		| Torrent client dir csv | Torrent grease dir  |
		| /downloads/complete    | /downloads/complete |
	When I scan for orphanized files
	Then I see an overview of the following files
		| Filepath                    | Size |
		| /downloads/orphan-file1.txt | 1kb  |
#todo find with multiple files in a single torrent,
#todo find with same file name but different location,
#todo find with multiple mappings,
#todo find with difference in file mapping
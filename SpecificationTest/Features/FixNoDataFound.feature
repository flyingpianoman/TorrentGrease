Feature: Fix no data found
	In order to recover from a disk that ran out of space
	As a torrent maintainer
	I want to be able to resume the torrents after creating space

Scenario: View no data found error on torrents page
	Given the following torrents are staged
		| Name     | Location             | TrackerAnnounceUrl1             | TorrentFile1Path | TorrentFile1SizeInKB |
		| TorrentA | /downloads/completed | http://my-tracker:6969/announce | file1.txt        | 10240                |
	And the data of the following torrents is sent to the torrent client
		| TorrentName | TargetLocation       | VerifyTorrent |
		| TorrentA    | /downloads/completed | true          |
	And the following torrent data files are moved
		| From                                    | To                                          |
		| /downloads/completed/TorrentA/file1.txt | /downloads/completed/TorrentA/file1.txt.bak |
    And the following torrents are (re)verified by the torrent client
        | TorrentName |
        | TorrentA    |
	When I navigate to the torrent overview
	Then I see an overview of the following torrents
		| Name     | Error                                                                                                                    |
		| TorrentA | No data found! Ensure your drives are connected or use "Set Location". To re-download, remove the torrent and re-add it. |
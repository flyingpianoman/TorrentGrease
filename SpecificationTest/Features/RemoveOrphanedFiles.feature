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
        | FilePath                                    | FileSizeInKB |
        | /downloads/complete/orphan-file1.txt        | 1            |
        | /downloads/complete/subdir/orphan-file2.txt | 2            |
        | /downloads/complete/orphan-file3.txt        | 3            |
        
Scenario: Find orphan files
    Given I navigate to file management
    And I set a minimal filesize of 0 KB
    And I set the following torrent dir mapping
        | Torrent client dir csv | Torrent grease dir  |
        | /downloads/complete    | /downloads/complete |
    When I scan for orphanized files
    Then I see an overview of the following files
        | Filepath                                    | Size |
        | /downloads/complete/orphan-file1.txt        | 1 KB |
        | /downloads/complete/orphan-file3.txt        | 3 KB |
        | /downloads/complete/subdir/orphan-file2.txt | 2 KB |

Scenario: Find orphan files of certain size
    Given I navigate to file management
    And I set a minimal filesize of 2 KB
    And I set the following torrent dir mapping
        | Torrent client dir csv | Torrent grease dir  |
        | /downloads/complete    | /downloads/complete |
    When I scan for orphanized files
    Then I see an overview of the following files
        | Filepath                                    | Size |
        | /downloads/complete/orphan-file3.txt        | 3 KB |
        | /downloads/complete/subdir/orphan-file2.txt | 2 KB |

Scenario: Remove selected orphan files
    Given I navigate to file management
    And I set a minimal filesize of 0 KB
    And I set the following torrent dir mapping
        | Torrent client dir csv | Torrent grease dir  |
        | /downloads/complete    | /downloads/complete |
    And I scan for orphanized files
    And I see an overview of the following files
        | Filepath                                    | Size |
        | /downloads/complete/orphan-file1.txt        | 1 KB |
        | /downloads/complete/orphan-file3.txt        | 3 KB |
        | /downloads/complete/subdir/orphan-file2.txt | 2 KB |
    And I select the following orphan files
        | Filepath                                    | 
        | /downloads/complete/orphan-file1.txt        | 
        | /downloads/complete/subdir/orphan-file2.txt | 
    When I remove the selected orphan files
    And I scan for orphanized files
    Then I see an overview of the following files
        | Filepath                                    | Size |
        | /downloads/complete/orphan-file3.txt        | 3 KB |

#todo find with multiple files in a single torrent,
#todo find with same file name but different location,
#todo find with multiple mappings,
#todo find with difference in file mapping
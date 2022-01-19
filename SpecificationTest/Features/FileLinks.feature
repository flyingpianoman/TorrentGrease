Feature: File links
	In order to save on disk space
	As a torrent maintainer
	I want to be able to replace duplicate files with hard links

Background:
    Given the following data is sent to the torrent client
        | FilePath                                                | FileSizeInKB | Char |
        | /downloads/complete/dir1/file-with-a.txt                | 1            | a    |
        | /downloads/complete/dir1/file-with-a-different-name.txt | 1            | a    |
        | /downloads/complete/dir1/file-with-b1.txt               | 2            | b    |
        | /downloads/complete/dir2/file-with-a.txt                | 1            | a    |
        | /downloads/complete/dir1/file-with-b2.txt               | 2            | b    |
        | /downloads/complete/dir1/file-with-c.txt                | 1            | c    |
    And I navigate to file links
        
Scenario: Find file link candidates
    Given I set a minimal filesize of 0 KB
    And I set the following directories to scan
        | Directory  |
        | /downloads |
    When I scan for possible file links
    Then I see an overview of the following file link candidates
        | FilePath1                                 | FilePath2                                               | FilePath3                                | Size |
        | /downloads/complete/dir1/file-with-a.txt  | /downloads/complete/dir1/file-with-a-different-name.txt | /downloads/complete/dir2/file-with-a.txt | 1 KB  |
        | /downloads/complete/dir1/file-with-b1.txt | /downloads/complete/dir1/file-with-b2.txt               |                                          | 2 KB  |
#todo
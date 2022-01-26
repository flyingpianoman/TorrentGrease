Feature: Replace duplicate files with file links
	In order to save on disk space
	As a torrent maintainer
	I want to replace duplicate files with file links

Background:
    Given I navigate to file links
        
Scenario: Replace duplicate file with a file link
    Given the following data is sent to the torrent client
        | FilePath                                                               | FileSizeInKB | Char |
        | /downloads/complete/dir1/file-with-a.txt                               | 1            | a    |
        | /downloads/complete/dir1/file-with-a-different-name.txt                | 1            | a    |
        | /downloads/complete/dir1/file-with-b1.txt                              | 2            | b    |
        | /downloads/complete/dir2/file-with-a.txt                               | 1            | a    |
        | /downloads/complete/dir1/file-with-b2.txt                              | 2            | b    |
        | /downloads/complete/dir1/file-without-candidate-but-same-size-as-a.txt | 1            | c    |
    And I set the following directories to scan
        | Directory  |
        | /downloads |
    And I scan for possible file links
    And I see an overview of the following file link candidates
        | FilePath1                                 | FilePath2                                               | FilePath3                                | Size |
        | /downloads/complete/dir1/file-with-a.txt  | /downloads/complete/dir1/file-with-a-different-name.txt | /downloads/complete/dir2/file-with-a.txt | 1 KB |
        | /downloads/complete/dir1/file-with-b1.txt | /downloads/complete/dir1/file-with-b2.txt               |                                          | 2 KB |
    When I ensure that only the candidates containing the following paths are selected
        | FilePath                                 |
        | /downloads/complete/dir1/file-with-a.txt |
    And I create file links for the selected candidates
    Then when I scan for possible file links again
    And I see an overview of the following file link candidates
        | FilePath1                                 | FilePath2                                 | FilePath3 | Size |
        | /downloads/complete/dir1/file-with-b1.txt | /downloads/complete/dir1/file-with-b2.txt |           | 2 KB |
    And the following files are all linked to the same device and inode
        | FilePath                                                |
        | /downloads/complete/dir1/file-with-a.txt                |
        | /downloads/complete/dir1/file-with-a-different-name.txt |
        | /downloads/complete/dir2/file-with-a.txt                |


        
#todo
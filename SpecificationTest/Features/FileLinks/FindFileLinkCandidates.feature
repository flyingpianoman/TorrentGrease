Feature: Find File link candidates
	In order to save on disk space
	As a torrent maintainer
	I want to be able to find file duplicates (aka file link candidates)
    So that I can replace them with file links in another feature

Background:
    Given I navigate to file links
        
Scenario: Find file link candidates that are equal
    Given the following data is sent to the torrent client
        | FilePath                                                               | FileSizeInKB | Char |
        | /downloads/complete/dir1/file-with-a.txt                               | 1            | a    |
        | /downloads/complete/dir1/file-with-a-different-name.txt                | 1            | a    |
        | /downloads/complete/dir1/file-with-b1.txt                              | 2            | b    |
        | /downloads/complete/dir2/file-with-a.txt                               | 1            | a    |
        | /downloads/complete/dir1/file-with-b2.txt                              | 2            | b    |
        | /downloads/complete/dir1/file-without-candidate-but-same-size-as-a.txt | 1            | c    |
    And I set a minimal filesize of 0 KB
    And I set the following directories to scan
        | Directory  |
        | /downloads |
    When I scan for possible file links
    Then I see an overview of the following file link candidates
        | FilePath1                                 | FilePath2                                               | FilePath3                                | Size |
        | /downloads/complete/dir1/file-with-a.txt  | /downloads/complete/dir1/file-with-a-different-name.txt | /downloads/complete/dir2/file-with-a.txt | 1 KB |
        | /downloads/complete/dir1/file-with-b1.txt | /downloads/complete/dir1/file-with-b2.txt               |                                          | 2 KB |
        
Scenario: Find file link candidates, show hard links if there are also non linked files
    Given the following data is sent to the torrent client
        | FilePath                                                | FileSizeInKB | Char |
        | /downloads/complete/dir1/file-with-a.txt                | 1            | a    |
        | /downloads/complete/dir1/file-with-a-different-name.txt | 1            | a    |
    Given I create a hard link for the following files
        | FilePath                                 | Hard link target path                               |
        | /downloads/complete/dir1/file-with-a.txt | /downloads/complete/dir1/file-with-a-hardlinked.txt |
    And I set a minimal filesize of 0 KB
    And I set the following directories to scan
        | Directory  |
        | /downloads |
    When I scan for possible file links
    Then I see an overview of the following file link candidates
        | FilePath1                                | FilePath2                                               | FilePath3                                           | Size |
        | /downloads/complete/dir1/file-with-a.txt | /downloads/complete/dir1/file-with-a-different-name.txt | /downloads/complete/dir1/file-with-a-hardlinked.txt | 1 KB |
        
Scenario: Find file link candidates, don't show hard links if there are only linked files
    Given the following data is sent to the torrent client
        | FilePath                                 | FileSizeInKB | Char |
        | /downloads/complete/dir1/file-with-a.txt | 1            | a    |
    Given I create a hard link for the following files
        | FilePath                                 | Hard link target path                               |
        | /downloads/complete/dir1/file-with-a.txt | /downloads/complete/dir1/file-with-a-hardlinked.txt |
    And I set a minimal filesize of 0 KB
    And I set the following directories to scan
        | Directory  |
        | /downloads |
    When I scan for possible file links
    Then I see an empty file link candidates overview 
    

#todo
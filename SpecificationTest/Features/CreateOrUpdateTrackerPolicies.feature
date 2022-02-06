Feature: CreateOrUpdateTrackerPolicies
	In order to manage torrent per tracker
	As a torrent maintainer
	I want to have the current tracker urls for my tracker policies

Scenario: Get current tracker url collections when 0 torrents in client
    When I get the current tracker url collections
    Then I get 0 current tracker url collections
    
Scenario: Get 2 current tracker url collections
    Given the following torrents are staged
        | Name     | Location            | TrackerAnnounceUrl1                      | TrackerAnnounceUrl2                      | TorrentFile1Path | TorrentFile1SizeInKB |
        | TorrentA | /downloads/complete | http://first-tracker:6969/announce       |                                          | file1.txt        | 1                    |
        | TorrentB | /downloads/complete | http://second-tracker-url1:6969/announce | http://second-tracker-url2:12345/annouce | file1.txt        | 1                    |
    When I get the current tracker url collections
    Then I get the following current tracker url collections
        | TrackerAnnounceUrl1      | TrackerAnnounceUrl2       |
        | first-tracker:6969       |                           |
        | second-tracker-url1:6969 | second-tracker-url2:12345 |
        
Scenario: tracker url collections match case insensitive
    Given the following torrents are staged
        | Name     | Location            | TrackerAnnounceUrl1                | TrackerAnnounceUrl2 | TorrentFile1Path | TorrentFile1SizeInKB |
        | TorrentA | /downloads/complete | http://first-tracker:6969/announce |                     | file1.txt        | 1                    |
        | TorrentB | /downloads/complete | http://FIRST-TRACKER:6969/announce |                     | file1.txt        | 1                    |
    When I get the current tracker url collections
    Then I get the following current tracker url collections
        | TrackerAnnounceUrl1 | TrackerAnnounceUrl2 |
        | first-tracker:6969  |                     |
        
Scenario: tracker url collections match ignore http scheme
    Given the following torrents are staged
        | Name     | Location            | TrackerAnnounceUrl1                  | TrackerAnnounceUrl2 | TorrentFile1Path | TorrentFile1SizeInKB |
        | TorrentA | /downloads/complete | https://first-tracker:6969/announce1 |                     | file1.txt        | 1                    |
        | TorrentB | /downloads/complete | http://first-tracker:6969/announce1  |                     | file1.txt        | 1                    |
    When I get the current tracker url collections
    Then I get the following current tracker url collections
        | TrackerAnnounceUrl1 | TrackerAnnounceUrl2 |
        | first-tracker:6969  |                     |

Scenario: tracker url collections match ignore after base address
    Given the following torrents are staged
        | Name     | Location            | TrackerAnnounceUrl1                 | TrackerAnnounceUrl2 | TorrentFile1Path | TorrentFile1SizeInKB |
        | TorrentA | /downloads/complete | http://first-tracker:6969/announce1 |                     | file1.txt        | 1                    |
        | TorrentB | /downloads/complete | http://first-tracker:6969/announce2 |                     | file1.txt        | 1                    |
    When I get the current tracker url collections
    Then I get the following current tracker url collections
        | TrackerAnnounceUrl1 | TrackerAnnounceUrl2 |
        | first-tracker:6969  |                     |
        
Scenario: tracker url collections combine when urls overlap
    Given the following torrents are staged
        | Name     | Location            | TrackerAnnounceUrl1                     | TrackerAnnounceUrl2                     | TorrentFile1Path | TorrentFile1SizeInKB |
        | TorrentA | /downloads/complete | http://first-tracker-url1:6969/announce | http://first-tracker-url2:6969/announce | file1.txt        | 1                    |
        | TorrentB | /downloads/complete | http://first-tracker-url1:6969/announce | http://first-tracker-url3:6969/announce | file1.txt        | 1                    |
    When I get the current tracker url collections
    Then I get the following current tracker url collections
        | TrackerAnnounceUrl1     | TrackerAnnounceUrl2     | TrackerAnnounceUrl3     |
        | first-tracker-url1:6969 | first-tracker-url2:6969 | first-tracker-url3:6969 |

Scenario: tracker url collections match unordered
    Given the following torrents are staged
        | Name     | Location            | TrackerAnnounceUrl1                     | TrackerAnnounceUrl2                     | TorrentFile1Path | TorrentFile1SizeInKB |
        | TorrentA | /downloads/complete | http://first-tracker-url1:6969/announce | http://first-tracker-url2:6969/announce | file1.txt        | 1                    |
        | TorrentB | /downloads/complete | http://first-tracker-url2:6969/announce | http://first-tracker-url1:6969/announce | file1.txt        | 1                    |
    When I get the current tracker url collections
    Then I get the following current tracker url collections
        | TrackerAnnounceUrl1     | TrackerAnnounceUrl2     |
        | first-tracker-url1:6969 | first-tracker-url2:6969 |

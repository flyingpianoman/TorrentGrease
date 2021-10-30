Feature: Fix no data found
	In order to recover from a disk that ran out of space
	As a torrent maintainer
	I want to be able to resume the torrents after creating space

Background:
	Given the following torrents are staged
		| Name     | Location             | TrackerAnnounceUrl1             | TorrentFile1Path | TorrentFile1SizeInKB |
		| TorrentA | /downloads/completed | http://my-tracker:6969/announce | file1.txt        | 10240                |
		| TorrentB | /downloads/completed | http://my-tracker:6969/announce | file2.txt        | 10480                |
		| TorrentC | /downloads/completed | http://my-tracker:6969/announce | file3.txt        | 20480                |
	And the data of the following torrents is sent to the torrent client
		| TorrentName | TargetLocation       | VerifyTorrent |
		| TorrentA    | /downloads/completed | true          |
		| TorrentB    | /downloads/completed | true          |
	And the following torrent data files are moved
		| From                                    | To                                          |
		| /downloads/completed/TorrentA/file1.txt | /downloads/completed/TorrentA/file1.txt.bak |
		| /downloads/completed/TorrentB/file2.txt | /downloads/completed/TorrentB/file2.txt.bak |
	And the following torrents are (re)verified by the torrent client
		| TorrentName |
		| TorrentA    |
		| TorrentB    |
	And the following torrent data files are moved
		| From                                        | To                                      |
		| /downloads/completed/TorrentA/file1.txt.bak | /downloads/completed/TorrentA/file1.txt |
		| /downloads/completed/TorrentB/file2.txt.bak | /downloads/completed/TorrentB/file2.txt |

Scenario: View no data found error on torrents page
	When I navigate to the torrent overview
	Then I see an overview of the following torrents
		| Name     | Error                                                                                                                    |
		| TorrentA | No data found! Ensure your drives are connected or use "Set Location". To re-download, remove the torrent and re-add it. |
		| TorrentB | No data found! Ensure your drives are connected or use "Set Location". To re-download, remove the torrent and re-add it. |
		| TorrentC |                                                                                                                          |

Scenario: Can single out torrents by filtering on error
	Given I navigate to the torrent overview
	And I see the following error filters
		| Error filter                                                                                                             |
		| No data found! Ensure your drives are connected or use "Set Location". To re-download, remove the torrent and re-add it. |
	When I toggle the following error filter
		| Error filter                                                                                                             |
		| No data found! Ensure your drives are connected or use "Set Location". To re-download, remove the torrent and re-add it. |
	Then I see an overview of the following torrents
		| Name     | Error                                                                                                                    |
		| TorrentA | No data found! Ensure your drives are connected or use "Set Location". To re-download, remove the torrent and re-add it. |
		| TorrentB | No data found! Ensure your drives are connected or use "Set Location". To re-download, remove the torrent and re-add it. |

Scenario: Can fix single torrents missing data error by re-adding
	Given I navigate to the torrent overview
	And I see the following error filters
		| Error filter                                                                                                             |
		| No data found! Ensure your drives are connected or use "Set Location". To re-download, remove the torrent and re-add it. |
	When I select the following torrents
		| Name     |
		| TorrentA |
	And I readd the selected torrents
	Then I see an overview of the following torrents
		| Name     | Error                                                                                                                    |
		| TorrentA |                                                                                                                          |
		| TorrentB | No data found! Ensure your drives are connected or use "Set Location". To re-download, remove the torrent and re-add it. |
		| TorrentC |                                                                                                                          |

Scenario: Can fix multiple torrents' missing data error by re-adding
	Given I navigate to the torrent overview
	And I see the following error filters
		| Error filter                                                                                                             |
		| No data found! Ensure your drives are connected or use "Set Location". To re-download, remove the torrent and re-add it. |
	When I select the following torrents
		| Name     |
		| TorrentA |
		| TorrentB |
	And I readd the selected torrents
	Then I see an overview of the following torrents
		| Name     | Error |
		| TorrentA |       |
		| TorrentB |       |
		| TorrentC |       |
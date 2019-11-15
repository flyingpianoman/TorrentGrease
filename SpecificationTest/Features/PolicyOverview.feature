Feature: Policy overview
	In order to see all policies
	As a policy maintainer
	I want to see all policies I have defined

Background:
	Given the following trackers are staged
		| Name                 | TrackerUrl1                  | TrackerUrl2                 |
		| LinuxTracker         | http://linuxtracker.org:2710 |                             |
		| TorrentGreaseTracker | http://torrentgrease.local   | http://torrentgrease2.local |
	And the following policies are staged
		| Name      |
		| Longterm  |
		| Shortterm |
	And the following tracker policies are staged
		| Policy    | Tracker              |
		| Longterm  | LinuxTracker         |
		| Shortterm | LinuxTracker         |
		| Longterm  | TorrentGreaseTracker |
	And the staged data is uploaded to torrent grease

Scenario: View policies
	When I navigate to the policy overview
	Then I see an overview of the following policies
		| Name      | Tracker1     | Tracker2             |
		| Longterm  | LinuxTracker | TorrentGreaseTracker |
		| Shortterm | LinuxTracker |                      |
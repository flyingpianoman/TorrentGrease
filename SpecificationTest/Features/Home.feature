Feature: Home
	In order to have a landing page
	As a user
	I want to see the home page when navigating to the root url

	- On opening the root url the home page is shown
		- This page contains the navigation menu

Scenario: Open the application
	When I navigate to the root url
	Then I see a no policies created message
	And I can see the navigation menu

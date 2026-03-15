@social @accountability
Feature: Accountability Partners
  As a Waypoint user
  I want to pair with another person for mutual progress visibility
  So that we keep each other motivated through shared accountability

  Background:
    Given I am an authenticated user
    And I have reached at least level 7

  Rule: Users can pair with one accountability partner at a time

    @done
    Scenario: Send an accountability partner request
      When I send an accountability partner request to user "Jordan"
      Then "Jordan" should receive a partner request notification
      And the request should be in a "Pending" state

    @done
    Scenario: Accept a partner request
      Given I have a pending partner request from "Casey"
      When I accept the request
      Then "Casey" and I should be linked as accountability partners
      And we should both see each other's daily summary

    Scenario: Decline a partner request
      Given I have a pending partner request from "Casey"
      When I decline the request
      Then the request should be removed
      And "Casey" should be notified that the request was declined

    @done
    Scenario: Only one active partner at a time
      Given I already have an accountability partner "Jordan"
      When I attempt to send a partner request to "Alex"
      Then I should see a message that I already have an active partner
      And I should be offered the option to end my current partnership first

    @done
    Scenario: End an accountability partnership
      Given I have an accountability partner "Jordan"
      When I choose to end the partnership
      Then the partnership should be dissolved
      And "Jordan" should be notified
      And both our past shared summaries should remain in our individual histories

  Rule: Partners see daily summaries, not task-level detail

    Scenario: View partner's daily summary
      Given I have an accountability partner "Jordan"
      When I view my partner's daily summary
      Then I should see Jordan's task completion count for today
      And I should see Jordan's current streak status
      And I should see Jordan's active quest count
      And I should not see individual task titles or descriptions

    Scenario: Partner sees my summary
      Given I have an accountability partner "Jordan"
      And I have completed 5 tasks today and my streak is at 12 days
      When "Jordan" views my daily summary
      Then they should see "5 tasks completed today"
      And they should see "12-day streak"

    Scenario: Send a check-in message to partner
      Given I have an accountability partner "Jordan"
      When I send a check-in message "Great streak, keep it going!"
      Then "Jordan" should receive the message in their partner view
      And the message should appear in our shared message history

    Scenario: Partner check-in messages are limited scope
      Given I have an accountability partner "Jordan"
      When I view the messaging interface
      Then I should only be able to send encouragement messages up to 280 characters
      And the messaging should not function as a full chat system

    Scenario: Partner account is deactivated
      Given I have an accountability partner "Jordan"
      When "Jordan" deactivates their account
      Then the partnership should be automatically dissolved
      And I should be notified that my partner is no longer available
      And I should be able to send a new partner request to someone else

    Scenario: Re-pair with a former partner
      Given I previously had a partnership with "Jordan" that was ended
      When I send a new accountability partner request to "Jordan"
      Then the request should be sent successfully
      And our previous shared history should remain separate from the new partnership

    Scenario: Existing partnership persists regardless of level changes
      Given I have an accountability partner "Jordan"
      And I was level 7 when the partnership was formed
      When my level calculation is adjusted and I am now below level 7
      Then the existing partnership should remain active
      And I should not be able to form new partnerships until I return to level 7

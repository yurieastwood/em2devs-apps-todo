@social @accountability
Feature: Accountability Partners
  As a Waypoint user
  I want to pair with another person for mutual progress visibility
  So that we keep each other motivated through shared accountability

  Background:
    Given I am an authenticated user
    And I have reached at least level 7

  # ───────────────────────────────────────────
  # Pairing
  # ───────────────────────────────────────────

  Rule: Users can pair with one accountability partner at a time

    Scenario: Send an accountability partner request
      When I send an accountability partner request to user "Jordan"
      Then "Jordan" should receive a partner request notification
      And the request should be in a "Pending" state

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

    Scenario: Only one active partner at a time
      Given I already have an accountability partner "Jordan"
      When I attempt to send a partner request to "Alex"
      Then I should see a message that I already have an active partner
      And I should be offered the option to end my current partnership first

    Scenario: End an accountability partnership
      Given I have an accountability partner "Jordan"
      When I choose to end the partnership
      Then the partnership should be dissolved
      And "Jordan" should be notified
      And both our past shared summaries should remain in our individual histories

  # ───────────────────────────────────────────
  # Shared Visibility
  # ───────────────────────────────────────────

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
      Then I should only be able to send short encouragement messages
      And the messaging should not function as a full chat system

@reflection @weekly-review
Feature: Weekly Review Ritual
  As a Waypoint user
  I want a guided weekly review that surfaces insights about my productivity
  So that I build a habit of reflection and continuous improvement

  Background:
    Given I am an authenticated user

  Rule: The weekly review is prompted at a consistent user-chosen time

    Scenario: Weekly review prompt at scheduled time
      Given I have configured my weekly review for Sunday at 7 PM
      When it is Sunday at 7 PM
      Then I should receive a notification prompting me to start my weekly review
      And the notification should indicate the estimated time of 5 minutes

    Scenario: Configure weekly review schedule
      When I navigate to my review settings
      And I set my weekly review to "Saturday at 10 AM"
      Then future review prompts should arrive Saturday at 10 AM

    Scenario: Start review manually at any time
      When I navigate to the weekly review section
      And I choose to start a review now
      Then the review flow should begin regardless of scheduled time

    Scenario: Default review schedule when no preference is set
      Given I have not configured a weekly review schedule
      Then my weekly review should default to Sunday at 6 PM in my local timezone
      And I should receive a notification at the default time

    Scenario: Dismiss weekly review prompt
      Given I receive the weekly review notification
      When I dismiss the notification
      Then the review should remain available in the review section
      And I should receive one follow-up reminder 24 hours later in my local timezone
      And no further reminders should be sent for this week's review

  Rule: Free-tier users get a streamlined review covering essential retrospection

    Scenario: Complete a basic weekly review
      Given I am a free-tier user
      When I start the weekly review
      Then I should see a summary of the week:
        | Metric                 | Example   |
        | Tasks completed        | 24        |
        | Tasks created          | 30        |
        | Quests completed       | 2         |
        | Current streak         | 11 days   |
        | XP earned              | 420       |
      And I should be prompted with "What went well this week?"
      And I enter my reflection text for "What went well this week?"
      And I should be prompted with "What could go better next week?"
      And I enter my reflection text for "What could go better next week?"
      Then the review should be saved
      And I should receive weekly review XP
      And the review should appear in my review history

    Scenario: View past weekly reviews
      Given I have completed 6 weekly reviews
      When I navigate to my review history
      Then I should see all 6 reviews in reverse chronological order
      And each review should show the week's summary metrics
      And each review should show my reflection notes

  @premium
  Rule: Premium users get data-rich retrospectives with visual insights

    Scenario: Complete an advanced weekly review
      Given I have a premium subscription
      When I start the weekly review
      Then I should see the basic summary metrics
      And I should see a productivity chart comparing this week to the last 4 weeks showing completed tasks, XP earned, and streak status
      And I should see my most productive day and time window
      And I should see tasks I avoided or rescheduled repeatedly
      And I should see estimation accuracy for the week
      And I should see quest progress updates
      And I should be prompted for reflection questions
      When I complete all reflection prompts
      Then the review should be saved with all data and reflections

    Scenario: Review surfaces patterns across weeks
      Given I have completed 8 weekly reviews
      When I start this week's review
      Then I should see trend analysis such as:
        | Insight                                                          |
        | Your Tuesday productivity has increased 30% over the last month  |
        | You complete more creative tasks in the morning                  |
        | Your estimation accuracy has improved from 55% to 72%            |

  Rule: Completing weekly reviews earns XP and maintains a review streak

    Scenario: Earn XP for completing weekly review
      When I complete my weekly review
      Then I should receive weekly review XP
      And my review streak should increment

    Scenario: Review streak builds over weeks
      Given I have completed weekly reviews for 11 consecutive weeks
      When I complete this week's review
      Then my review streak should be 12 weeks
      And I should be notified of my progress toward the "Consistent Planner" title

    Scenario: Missed review does not break streak harshly
      Given I have a review streak of 8 weeks
      And I miss one week's review
      Then my streak should be paused
      And I should have a 1-week grace period to complete the missed review
      And if I complete next week's review, my streak should continue from 8

    Scenario: Complete two missed weeks during the grace period
      Given I have a review streak of 5 weeks
      And I missed the last two weeks' reviews
      And I am within the 1-week grace period
      When I complete both the missed week's review and the current week's review
      Then both reviews should be saved and counted
      And my review streak should continue from 7 weeks

    Scenario: Progress is saved as draft when user logs out mid-review
      Given I have started my weekly review
      And I have entered reflection text for "What went well this week?"
      When I log out before completing the review
      Then my in-progress review should be saved as a draft
      And when I log back in and navigate to the weekly review section
      Then I should see the option to resume my draft review
      And my previously entered reflection text should be preserved

@reflection @weekly-review
Feature: Weekly Review Ritual
  As a Waypoint user
  I want a guided weekly review that surfaces insights about my productivity
  So that I build a habit of reflection and continuous improvement

  Background:
    Given I am an authenticated user

  # ───────────────────────────────────────────
  # Review Trigger
  # ───────────────────────────────────────────

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

    Scenario: Dismiss weekly review prompt
      Given I receive the weekly review notification
      When I dismiss the notification
      Then the review should remain available in the review section
      And I should receive one follow-up reminder 24 hours later
      And no further reminders should be sent for this week's review

  # ───────────────────────────────────────────
  # Basic Review Flow (Free Tier)
  # ───────────────────────────────────────────

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
      When I enter my reflection text
      And I am prompted with "What could go better next week?"
      When I enter my reflection text
      Then the review should be saved
      And I should receive weekly review XP
      And the review should appear in my review history

    Scenario: View past weekly reviews
      Given I have completed 6 weekly reviews
      When I navigate to my review history
      Then I should see all 6 reviews in reverse chronological order
      And each review should show the week's summary metrics
      And each review should show my reflection notes

  # ───────────────────────────────────────────
  # Advanced Review Flow (Premium Tier)
  # ───────────────────────────────────────────

  @premium
  Rule: Premium users get data-rich retrospectives with visual insights

    Scenario: Complete an advanced weekly review
      Given I have a premium subscription
      When I start the weekly review
      Then I should see the basic summary metrics
      And I should see a productivity chart comparing this week to the last 4 weeks
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

  # ───────────────────────────────────────────
  # Review Streaks and XP
  # ───────────────────────────────────────────

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

@progression @streaks
Feature: Streaks and Grace Days
  As a Waypoint user
  I want my streaks to be celebrated without punishing inevitable off-days
  So that I stay motivated by consistency without developing streak anxiety

  Background:
    Given I am an authenticated user

  # ───────────────────────────────────────────
  # Streak Tracking
  # ───────────────────────────────────────────

  Rule: Streaks track consecutive days of completing at least one task

    Scenario: Streak increments on daily completion
      Given my current streak is 5 days
      And I have not completed any tasks today
      When I complete a task today
      Then my streak should increment to 6 days
      And only the first completion should trigger the increment

    Scenario: Streak persists through multiple completions
      Given my current streak is 10 days
      And I have already completed 3 tasks today
      When I complete a 4th task
      Then my streak should remain at 10 days (already counted today)

    Scenario: Streak milestone celebration
      Given my current streak is 6 days
      When I complete a task and my streak reaches 7 days
      Then I should see a streak milestone celebration
      And "7-day streak" should appear on my journey timeline

    Scenario Outline: Streak milestones are celebrated at key thresholds
      Given my streak is at <streak_days> minus 1
      When my streak reaches <streak_days>
      Then I should see a milestone celebration for "<label>"

      Examples:
        | streak_days | label             |
        | 7           | One Week          |
        | 14          | Two Weeks         |
        | 30          | One Month         |
        | 60          | Two Months        |
        | 100         | The Century       |
        | 365         | The Full Year     |

  # ───────────────────────────────────────────
  # Grace Days
  # ───────────────────────────────────────────

  Rule: Grace days protect streaks from occasional missed days

    Scenario: Grace day preserves streak on a missed day
      Given my current streak is 15 days
      And I have 1 grace day available
      And I complete no tasks today
      When the day ends
      Then my streak should remain at 15 days
      And 1 grace day should be consumed
      And I should be notified that a grace day was used

    Scenario: Grace day not consumed on an active day
      Given my current streak is 15 days
      And I have 2 grace days available
      And I complete 3 tasks today
      When the day ends
      Then my streak should be 16 days
      And I should still have 2 grace days available

    Scenario: Grace days accumulate over time
      Given I have 0 grace days
      And I complete my weekly review this week
      Then I should earn 1 grace day
      And I can hold a maximum of 3 grace days at once

    Scenario: Streak broken when no grace days available
      Given my current streak is 20 days
      And I have 0 grace days available
      And I complete no tasks today
      When the day ends
      Then my streak should reset to 0
      And I should see an encouraging message, not a punishment
      And I should see my previous streak of 20 days recorded in my history
      And the message should say something like "Your 20-day streak was impressive. Let us start the next one."

    Scenario: No negative consequences for broken streak
      Given my streak just reset from 20 to 0
      Then no XP should be deducted
      And no titles should be revoked
      And no skill tree progress should be lost
      And my past streak should remain on my journey timeline as an achievement

  # ───────────────────────────────────────────
  # Streak Freeze
  # ───────────────────────────────────────────

  Rule: Users can manually freeze their streak when they know they will be unavailable

    Scenario: Activate a streak freeze
      Given my current streak is 30 days
      And I am going on holiday for 5 days
      When I activate a streak freeze for 5 days
      Then my streak should be frozen at 30 days
      And the 5 frozen days should not count against my streak
      And I should not receive task reminders during the freeze

    Scenario: Streak freeze has a maximum duration
      When I attempt to freeze my streak for 15 days
      Then I should see a message that the maximum freeze duration is 7 days
      And I should be offered to set a 7-day freeze instead

    Scenario: Streak resumes after freeze ends
      Given my streak is frozen at 30 days for 5 days
      When the freeze period ends
      And I complete a task the next day
      Then my streak should continue from 31 days

@progression @streaks
Feature: Streaks and Grace Days
  As a Waypoint user
  I want my streaks to be celebrated without punishing inevitable off-days
  So that I stay motivated by consistency without developing streak anxiety

  Background:
    Given I am an authenticated user

  Rule: Streaks track consecutive days of completing at least one task

    @done
    Scenario: Streak increments on daily completion
      Given my current streak is 5 days
      And I have not completed any tasks today
      When I complete a task today
      Then my streak should increment to 6 days
      And only the first completion should trigger the increment

    @done
    Scenario: Streak persists through multiple completions
      Given my current streak is 10 days
      And I have already completed 3 tasks today
      When I complete a 4th task
      Then my streak should remain at 10 days (already counted today)

    @wip
    Scenario: Streak milestone celebration
      Given my current streak is 6 days
      When I complete a task and my streak reaches 7 days
      Then I should see a streak milestone celebration
      And "7-day streak" should appear on my journey timeline

    @wip
    Scenario Outline: Streak milestones are celebrated at key thresholds
      Given my current streak is <previous_days> days
      When I complete a task and my streak reaches <streak_days> days
      Then I should see a milestone celebration for "<label>"

      Examples:
        | previous_days | streak_days | label             |
        | 6             | 7           | One Week          |
        | 13            | 14          | Two Weeks         |
        | 29            | 30          | One Month         |
        | 59            | 60          | Two Months        |
        | 99            | 100         | The Century       |
        | 364           | 365         | The Full Year     |

  Rule: Grace days protect streaks from occasional missed days

    @done
    Scenario: Grace day preserves streak on a missed day
      Given my current streak is 15 days
      And I have 1 grace day available
      And I complete no tasks today
      When the day ends
      Then my streak should remain at 15 days
      And 1 grace day should be consumed
      And I should be notified that a grace day was used

    @done
    Scenario: Grace day not consumed on an active day
      Given my current streak is 15 days
      And I have 2 grace days available
      And I complete 3 tasks today
      When the day ends
      Then my streak should be 16 days
      And I should still have 2 grace days available

    @wip
    Scenario: Grace days accumulate over time
      Given I have 0 grace days
      And I complete my weekly review this week
      Then I should earn 1 grace day
      And I can hold a maximum of 3 grace days at once

    @done
    Scenario: Streak broken when no grace days available
      Given my current streak is 20 days
      And I have 0 grace days available
      And I complete no tasks today
      When the day ends
      Then my streak should reset to 0
      And I should see an encouraging restart message mentioning my previous 20-day streak
      And I should see my previous streak of 20 days recorded in my history

    @wip
    Scenario: No negative consequences for broken streak
      Given my streak just reset from 20 to 0
      Then no XP should be deducted
      And no titles should be revoked
      And no skill tree progress should be lost
      And my past streak should remain on my journey timeline as an achievement

  Rule: Users can manually freeze their streak when they know they will be unavailable

    @wip
    Scenario: Activate a streak freeze
      Given my current streak is 30 days
      And I am going on holiday for 5 days
      When I activate a streak freeze for 5 days
      Then my streak should be frozen at 30 days
      And the 5 frozen days should not count against my streak
      And I should not receive task reminders during the freeze

    @wip
    Scenario: Streak freeze has a maximum duration
      When I attempt to freeze my streak for 15 days
      Then I should see a message that the maximum freeze duration is 7 days
      And I should be offered to set a 7-day freeze instead

    @wip
    Scenario: Streak resumes after freeze ends
      Given my streak is frozen at 30 days for 5 days
      When the freeze period ends
      And I complete a task the next day
      Then my streak should continue from 31 days

  Rule: Streak day boundaries are determined by the user's configured timezone

    @wip
    Scenario: Streak day boundary respects user timezone
      Given my timezone is set to "Australia/Sydney" (UTC+11)
      And my current streak is 5 days
      And it is 11:30 PM in my timezone
      When I complete a task
      Then it should count toward today's streak in my timezone
      And my streak should remain at 5 days if I already completed a task today

    @wip
    Scenario: Completing tasks during a streak freeze does not end the freeze early
      Given my streak is frozen at 30 days for 5 days
      And I am on day 2 of the freeze
      When I complete a task
      Then the freeze should remain active for the remaining 3 days
      And the task completion should be recorded normally
      And the streak should remain frozen at 30 days

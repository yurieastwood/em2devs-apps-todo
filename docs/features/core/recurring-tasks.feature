@core @recurring
Feature: Recurring Tasks and Quest Chains
  As a Waypoint user
  I want to set up recurring tasks and quest chains
  So that repetitive workflows are automated and tracked consistently

  Background:
    Given I am an authenticated user

  Rule: Recurring tasks regenerate on a defined schedule

    @done
    Scenario: Create a daily recurring task
      When I create a recurring task with the following details:
        | Field      | Value                |
        | Title      | Morning standup prep |
        | Recurrence | Daily                |
        | Time       | 08:30                |
      Then the task should appear in my Today view each day at 08:30
      And each instance should be a separate completable task

    @done
    Scenario: Create a weekly recurring task
      When I create a recurring task with the following details:
        | Field      | Value              |
        | Title      | Weekly meal prep   |
        | Recurrence | Weekly on Sunday   |
      Then the task should appear every Sunday
      And completing one instance should not affect future instances

    @done
    Scenario: Create a monthly recurring task
      When I create a recurring task with the following details:
        | Field      | Value                      |
        | Title      | Submit expense report      |
        | Recurrence | Monthly on the last Friday |
      Then the task should appear on the last Friday of each month

    @wip
    Scenario: Create a recurring task with an end date
      When I create a recurring task with the following details:
        | Field      | Value                |
        | Title      | Sprint retrospective |
        | Recurrence | Weekly on Friday     |
        | End Date   | 2026-06-30           |
      Then the task should appear every Friday until 2026-06-30
      And no instances should be generated after the end date

    @done
    Scenario: Complete a recurring task instance
      Given I have a daily recurring task "Morning standup prep"
      And today's instance is open
      When I complete today's instance
      Then today's instance should be marked as completed
      And I should receive XP for the completion
      And tomorrow's instance should be generated
      And the recurring task streak should increment by 1

    @wip
    Scenario: Complete a recurring task instance late
      Given I have a daily recurring task "Morning standup prep"
      And yesterday's instance is still open
      When I complete yesterday's instance
      Then the instance should be marked as completed
      And I should receive XP with an overdue penalty applied
      And my streak should be broken

    @done
    Scenario: Skip a recurring task instance
      Given I have a daily recurring task "Morning standup prep"
      And today's instance is open
      When I skip today's instance
      Then today's instance should be marked as "Skipped"
      And no XP should be awarded or deducted
      And the streak counter should freeze at its current value
      And the skip should appear in my recurring task history

    @done
    Scenario: Pause a recurring task
      Given I have a weekly recurring task "Team retrospective"
      When I pause the recurring task
      Then no new instances should be generated
      And existing uncompleted instances should remain
      And the task should show a "Paused" status

    @done
    Scenario: Resume a paused recurring task
      Given I have a paused recurring task "Team retrospective"
      When I resume the recurring task
      Then new instances should begin generating again
      And the streak counter should resume from where it was paused

    @done
    Scenario: Edit all future instances of a recurring task
      Given I have a daily recurring task "Check email" at 09:00
      When I edit the recurring task time to 08:00
      And I choose to apply changes to all future instances
      Then all future instances should be scheduled at 08:00
      And past instances should remain unchanged

    @done
    Scenario: Delete a recurring task
      Given I have a weekly recurring task "Water plants"
      When I delete the recurring task
      And I confirm the deletion
      Then no future instances should be generated
      And completed past instances should remain in my history
      And the XP earned from past instances should be retained

    @done
    Scenario: Handle overlapping recurring task instances
      Given I have a daily recurring task "Morning standup prep"
      And yesterday's instance is still open
      When today's instance is generated
      Then both yesterday's and today's instances should be visible
      And yesterday's instance should be marked as overdue

  Rule: Quest chains auto-generate recurring quest structures from patterns

    @wip
    Scenario: User receives a suggestion for a recurring quest pattern
      Given I have completed the following quests in the last 3 weeks:
        | Quest Title       | Completed On |
        | Weekly meal prep  | 2026-03-01   |
        | Weekly meal prep  | 2026-03-08   |
        | Weekly meal prep  | 2026-03-15   |
      When I view my quest insights
      Then I should see a suggestion to create a quest chain for "Weekly meal prep"
      And the suggestion should include the detected cadence of "Weekly"

    @wip
    Scenario: Create a quest chain from a template
      When I create a quest chain with the following details:
        | Field     | Value                          |
        | Title     | Weekly Meal Prep               |
        | Cadence   | Weekly on Saturday             |
        | Tasks     | Plan meals, Write shopping list, Buy ingredients, Prep ingredients |
      Then a new quest should be auto-generated every Saturday
      And each quest should contain the 4 specified tasks
      And each quest should have a 24-hour default deadline

    @wip
    Scenario: Quest chain adapts task list over time
      Given I have a quest chain "Weekly Meal Prep" running for 4 weeks
      And I have consistently added an extra task "Clean kitchen" to each instance
      When the next instance is generated
      Then the system should suggest adding "Clean kitchen" to the chain template
      When I accept the suggestion
      Then all future instances should include "Clean kitchen"

    @wip
    Scenario: View quest chain history and stats
      Given I have a quest chain "Weekly Meal Prep" running for 8 weeks
      When I view the quest chain details
      Then I should see the completion rate across all instances
      And I should see the average time to complete each instance
      And I should see the streak of consecutive completions
      And I should see the total XP earned from the chain

    @wip
    Scenario: Quest chain generates bonus XP for consistency
      Given I have a quest chain "Weekly Meal Prep" with a 4-week streak
      When the 5th consecutive instance is completed
      Then I should receive the standard quest completion XP
      And I should receive a chain consistency bonus multiplier
      And the bonus should increase with longer streaks

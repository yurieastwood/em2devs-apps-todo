@progression @xp
Feature: Experience Points
  As a Waypoint user
  I want to earn XP for completing tasks that reflects the genuine effort involved
  So that my progression feels earned and honest

  Background:
    Given I am an authenticated user

  Rule: XP is weighted by difficulty, timeliness, and consistency

    Scenario Outline: XP awarded based on task difficulty
      Given I have a task with difficulty "<difficulty>"
      When I complete the task on time
      Then I should receive between <min_xp> and <max_xp> XP

      Examples:
        | difficulty | min_xp | max_xp |
        | Trivial    | 5      | 10     |
        | Easy       | 10     | 20     |
        | Normal     | 20     | 40     |
        | Hard       | 40     | 80     |
        | Epic       | 80     | 150    |

    Scenario: XP bonus for completing a task before the deadline
      Given I have a task due in 3 days with difficulty "Normal"
      When I complete the task 2 days before the deadline
      Then I should receive the base XP for the difficulty
      And I should receive an early completion bonus

    Scenario: Reduced XP for completing a task after the deadline
      Given I have a task that was due yesterday with difficulty "Normal"
      When I complete the task 1 day late
      Then I should receive reduced XP compared to on-time completion
      And the XP should still be greater than zero

    Scenario: XP is never negative
      Given I have a task that is 30 days overdue
      When I complete the task
      Then I should receive a minimum positive XP amount
      And no XP should be deducted from my total

    Scenario: Consistency multiplier for daily streaks
      Given I have completed at least one task each day for 7 consecutive days
      When I complete a task today
      Then I should receive the base XP for the task
      And I should receive a streak consistency multiplier bonus
      And the multiplier should be displayed in the XP breakdown

    Scenario: XP correctly attributed to parent quest
      Given I have a quest "Sprint work" containing the task "Fix login bug"
      When I complete the task "Fix login bug"
      Then the XP should be counted toward my total
      And the XP should also be reflected in the quest's XP tally

  Rule: Users see transparent XP breakdowns for every completion

    Scenario: View XP breakdown after task completion
      Given I have just completed a task "Write unit tests" with difficulty "Hard"
      When I view the XP award details
      Then I should see a breakdown showing:
        | Component          |
        | Base XP            |
        | Early completion   |
        | Streak bonus       |
        | Total              |
      And each component should display its calculated value
      And the total should equal the sum of all components

    Scenario: View cumulative XP on profile
      When I view my profile
      Then I should see my total lifetime XP
      And I should see my current level
      And I should see the XP required for the next level
      And I should see a progress bar toward the next level

    Scenario: View XP history over time
      When I navigate to my XP history
      Then I should see a chart showing XP earned per day over the last 30 days
      And I should see the total XP earned this week
      And I should see the total XP earned this season

  Rule: The system detects and discourages XP inflation through trivial tasks

    Scenario: Detect burst of trivial task creation
      Given I create 20 tasks in 5 minutes with no descriptions or due dates
      And I immediately complete all 20 tasks
      Then the XP awarded should be at the reduced trivial-task rate
      And I should receive a gentle notification explaining the adjustment

    Scenario: Repeated trivial tasks earn diminishing returns
      Given I have completed 10 tasks with difficulty "Trivial" today
      When I complete an 11th trivial task
      Then the XP awarded should be less than the first trivial task
      And the diminishing rate should be visible in the XP breakdown

    Scenario: Difficulty rating auto-adjusts for repeated identical tasks
      Given I have a recurring task "Check email" rated as "Normal"
      And I have consistently completed it in under 2 minutes
      When I view the task details
      Then I should see a suggestion to adjust the difficulty to "Easy" or "Trivial"
      And I should see an explanation of why the adjustment is recommended

    Scenario: XP awarded with default difficulty when none is set
      Given I have a task with no difficulty set
      When I complete the task on time
      Then the task should be treated as "Normal" difficulty for XP purposes
      And I should receive the corresponding XP for "Normal" difficulty
      And I should see a prompt suggesting I set a difficulty for more accurate XP

    Scenario: XP for recurring task completions
      Given I have a recurring task "Morning standup" with difficulty "Easy"
      And I have completed this recurring task 5 times this week
      When I complete it again
      Then I should receive the base XP for "Easy" difficulty
      And the XP should reflect any applicable diminishing returns for repeated tasks

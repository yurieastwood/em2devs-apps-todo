@progression @xp
Feature: Experience Points
  As a Waypoint user
  I want to earn XP for completing tasks that reflects the genuine effort involved
  So that my progression feels earned and honest

  Background:
    Given I am an authenticated user

  # ───────────────────────────────────────────
  # XP Calculation
  # ───────────────────────────────────────────

  Rule: XP is weighted by difficulty, timeliness, and consistency

    Scenario Outline: XP awarded based on task difficulty
      Given I have a task with difficulty "<difficulty>"
      When I complete the task on time
      Then I should receive approximately <xp_range> XP

      Examples:
        | difficulty | xp_range   |
        | Trivial    | 5-10       |
        | Easy       | 10-20      |
        | Normal     | 20-40      |
        | Hard       | 40-80      |
        | Epic       | 80-150     |

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

  # ───────────────────────────────────────────
  # XP Display
  # ───────────────────────────────────────────

  Rule: Users see transparent XP breakdowns for every completion

    Scenario: View XP breakdown after task completion
      Given I have just completed a task "Write unit tests" with difficulty "Hard"
      When I view the XP award details
      Then I should see a breakdown including:
        | Component            | Value    |
        | Base XP (Hard)       | 60       |
        | Early completion     | +12      |
        | Streak bonus (x1.2)  | +14      |
        | Total                | 86       |

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

  # ───────────────────────────────────────────
  # Anti-Gaming
  # ───────────────────────────────────────────

  Rule: The system detects and discourages XP inflation through trivial tasks

    Scenario: Detect burst of trivial task creation
      Given I create 20 tasks in 5 minutes with no descriptions or due dates
      And I immediately complete all 20 tasks
      When the system evaluates the activity
      Then the tasks should be flagged for anomaly review
      And XP should be awarded at a reduced trivial-task rate
      And I should receive a gentle notification explaining the adjustment

    Scenario: Repeated trivial tasks earn diminishing returns
      Given I have completed 10 tasks with difficulty "Trivial" today
      When I complete an 11th trivial task
      Then the XP awarded should be less than the first trivial task
      And the diminishing rate should be visible in the XP breakdown

    Scenario: Difficulty rating auto-adjusts for repeated identical tasks
      Given I have a recurring task "Check email" rated as "Normal"
      And I have consistently completed it in under 2 minutes
      When the system recalibrates difficulty ratings
      Then the task difficulty should be adjusted to "Easy" or "Trivial"
      And I should be notified of the recalibration with an explanation

@intelligence @procrastination
Feature: Procrastination Detection
  As a Waypoint user
  I want the system to detect when I am avoiding tasks
  So that I receive helpful interventions rather than accumulating guilt

  Background:
    Given I am an authenticated user

  # ───────────────────────────────────────────
  # Detection Signals
  # ───────────────────────────────────────────

  Rule: The system identifies procrastination through multiple signals

    Scenario: Task rescheduled multiple times
      Given I have a task "Update resume"
      And I have rescheduled it 3 times in the last 2 weeks
      When the system evaluates my task list
      Then the task should be flagged as a procrastination candidate
      And I should receive a gentle intervention prompt

    Scenario: Task viewed repeatedly without action
      Given I have a task "Call accountant"
      And I have opened the task details 5 times in the last week
      And I have not started, completed, or rescheduled it
      When the system evaluates my task behaviour
      Then the task should be flagged as a procrastination candidate

    Scenario: High-priority task consistently skipped
      Given I have a task "Prepare investor pitch" with priority "Critical"
      And the task has been in my Today view for 4 consecutive days
      And I have completed other lower-priority tasks during those days
      When the system evaluates my completion patterns
      Then the task should be flagged as being avoided

    Scenario: Task open well past its due date with no progress
      Given I have a task "File insurance claim" that was due 10 days ago
      And no subtasks have been completed
      And the task has not been rescheduled
      When the system evaluates my task list
      Then the task should be flagged as a procrastination candidate

  # ───────────────────────────────────────────
  # Intervention Flow
  # ───────────────────────────────────────────

  Rule: Interventions are helpful, never punitive, and offer multiple paths forward

    Scenario: View procrastination intervention options
      Given the task "Update resume" has been flagged for procrastination
      When I open the intervention for this task
      Then I should see the following options:
        | Option               | Description                                          |
        | Break it down        | Split into smaller, less intimidating subtasks        |
        | Delegate it          | Convert to a shared quest or assign to someone        |
        | Re-evaluate          | Decide if this task still matters                     |
        | Boss Task it         | Promote to Boss Task for focused attack with bonus XP |
        | Reschedule with intent | Set a specific date with a commitment note           |

    Scenario: Choose to break down a procrastinated task
      Given I am viewing the intervention for "Prepare investor pitch"
      When I choose "Break it down"
      Then the system should suggest subtasks based on similar completed tasks
      And I should be able to accept, modify, or create my own subtasks
      When I confirm the breakdown into 4 subtasks
      Then the original task should become a parent task
      And each subtask should appear in my task list
      And the first subtask should be highlighted as the starting point

    Scenario: Choose to re-evaluate a procrastinated task
      Given I am viewing the intervention for "Reorganise garage"
      When I choose "Re-evaluate"
      Then I should see prompts helping me assess the task:
        | Prompt                                                    |
        | Does this still need to happen?                           |
        | What would the consequence be if you never did this?      |
        | Is someone else depending on this?                        |
        | Would you add this task today if it were not already here? |
      When I decide the task no longer matters
      And I choose to archive it
      Then the task should be archived without penalty
      And no XP should be deducted

    Scenario: Promote procrastinated task to Boss Task
      Given I am viewing the intervention for "Write thesis chapter"
      When I choose "Boss Task it"
      Then the task should be promoted to Boss Task status
      And I should be offered the full Boss Task intervention flow
      And the Boss Task bonus XP should be highlighted as motivation

    Scenario: Reschedule with commitment note
      Given I am viewing the intervention for "Schedule dentist appointment"
      When I choose "Reschedule with intent"
      And I set the new date to next Monday
      And I add the note "Will call during lunch break"
      Then the task should be rescheduled to next Monday
      And the commitment note should be visible on the task
      And on Monday, the task should appear with the note prominently displayed

  # ───────────────────────────────────────────
  # Procrastination Insights
  # ───────────────────────────────────────────

  Rule: Users can learn about their procrastination patterns over time

    @premium
    Scenario: View procrastination patterns
      Given I have 3 months of task history
      When I navigate to my procrastination insights
      Then I should see which task categories I most often avoid
      And I should see the average delay before intervention
      And I should see my intervention success rate
      And I should see tips based on my specific patterns

    Scenario: Intervention tone is always supportive
      Given a task has been flagged for procrastination
      When the system presents an intervention
      Then the language should be encouraging, not shaming
      And the message should normalise the experience
      And the focus should be on moving forward, not dwelling on delay

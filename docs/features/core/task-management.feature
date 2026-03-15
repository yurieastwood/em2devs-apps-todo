@core @tasks
Feature: Task Management
  As a Waypoint user
  I want to create, organise, and complete tasks
  So that I can track and accomplish my work effectively

  Background:
    Given I am an authenticated user
    And I am on the task management screen

  # ───────────────────────────────────────────
  # Task Creation
  # ───────────────────────────────────────────

  Rule: Users can create tasks with minimal friction

    Scenario: Create a task with only a title
      When I create a task with the title "Buy groceries"
      Then the task "Buy groceries" should appear in my inbox
      And the task should have no due date
      And the task should have no quest assignment
      And the task should have a default difficulty of "Normal"
      And the task should have a status of "Open"

    Scenario: Create a task with full details
      When I create a task with the following details:
        | Field          | Value                    |
        | Title          | Write Q2 report          |
        | Description    | Quarterly financial summary for stakeholders |
        | Due Date       | 2026-04-15               |
        | Estimated Time | 2 hours                  |
        | Priority       | High                     |
        | Tags           | work, reporting           |
      Then the task "Write Q2 report" should appear in my inbox
      And the task should have all specified details saved

    Scenario: Create a task with natural language date parsing
      When I create a task with the title "Call dentist"
      And I set the due date to "next Tuesday"
      Then the due date should resolve to the next occurring Tuesday
      And the task should appear in my upcoming view on that date

    Scenario: Create a task via quick-add
      When I activate the quick-add shortcut
      And I type "Submit tax return #personal !high ^April 15"
      Then a task "Submit tax return" should be created
      And it should be tagged "personal"
      And it should have priority "High"
      And it should have a due date of April 15

  # ───────────────────────────────────────────
  # Task Completion
  # ───────────────────────────────────────────

  Rule: Completing a task triggers progression and records actual effort

    Scenario: Complete a simple task
      Given I have an open task "Buy groceries"
      When I mark the task "Buy groceries" as complete
      Then the task status should change to "Completed"
      And the completion timestamp should be recorded
      And I should receive XP for the task
      And the task should appear in my completed tasks history

    Scenario: Complete a task and record actual time spent
      Given I have an open task "Write Q2 report" with an estimated time of 2 hours
      When I mark the task as complete
      Then I should be prompted to record actual time spent
      When I enter "2 hours 45 minutes" as actual time
      Then the time estimation variance should be recorded as +37.5%
      And the data should feed into my estimation learning model

    Scenario: Complete a task that is overdue
      Given I have a task "Submit proposal" that was due 3 days ago
      When I mark the task as complete
      Then the task should be marked as completed
      And the XP awarded should reflect the overdue penalty
      And the XP awarded should still be greater than zero

    Scenario: Complete the final task in a quest
      Given I have a quest "Prepare presentation" with 5 tasks
      And 4 of the 5 tasks are completed
      When I complete the remaining task "Do final rehearsal"
      Then the quest "Prepare presentation" should be marked as complete
      And I should receive quest completion bonus XP
      And a quest completion event should appear on my journey timeline

  # ───────────────────────────────────────────
  # Task Editing
  # ───────────────────────────────────────────

  Rule: Users can modify any aspect of an existing task

    Scenario: Edit a task title
      Given I have an open task "Buy grocries"
      When I edit the task title to "Buy groceries"
      Then the task title should be updated to "Buy groceries"

    Scenario: Change task priority
      Given I have an open task "Update website" with priority "Low"
      When I change the priority to "High"
      Then the task priority should be "High"
      And the task difficulty rating should be recalculated

    Scenario: Reschedule a task
      Given I have an open task "Team lunch" due on "2026-04-10"
      When I change the due date to "2026-04-17"
      Then the due date should be updated
      And a reschedule event should be recorded against the task

    Scenario: Add a description to an existing task
      Given I have an open task "Research competitors" with no description
      When I add the description "Focus on gamified productivity apps in the market"
      Then the task description should be saved

  # ───────────────────────────────────────────
  # Task Deletion
  # ───────────────────────────────────────────

  Rule: Deleting a task requires confirmation and does not award XP

    Scenario: Delete a task
      Given I have an open task "Cancelled meeting prep"
      When I delete the task "Cancelled meeting prep"
      Then I should be asked to confirm the deletion
      When I confirm the deletion
      Then the task should be removed from my task list
      And no XP should be awarded or deducted

    Scenario: Delete a task that belongs to a quest
      Given I have a quest "Launch campaign" containing 4 tasks
      And one task is "Design flyer"
      When I delete the task "Design flyer"
      And I confirm the deletion
      Then the quest "Launch campaign" should show 3 remaining tasks
      And the quest progress should be recalculated

  # ───────────────────────────────────────────
  # Task Organisation
  # ───────────────────────────────────────────

  Rule: Tasks can be filtered, sorted, and searched

    Scenario: Filter tasks by tag
      Given I have the following tasks:
        | Title             | Tags          |
        | Fix login bug     | work, dev     |
        | Buy birthday gift | personal      |
        | Update API docs   | work, dev     |
        | Book flights      | personal, travel |
      When I filter by the tag "work"
      Then I should see 2 tasks
      And I should see "Fix login bug" and "Update API docs"

    Scenario: Sort tasks by due date
      Given I have multiple tasks with different due dates
      When I sort tasks by "Due Date" ascending
      Then tasks should be ordered from earliest due date to latest
      And tasks with no due date should appear at the end

    Scenario: Sort tasks by priority
      Given I have tasks with priorities "Low", "High", "Medium", and "Critical"
      When I sort tasks by "Priority" descending
      Then tasks should be ordered: Critical, High, Medium, Low

    Scenario: Search tasks by keyword
      Given I have 20 tasks with various titles and descriptions
      When I search for "report"
      Then I should see only tasks whose title or description contains "report"

  # ───────────────────────────────────────────
  # Task Views
  # ───────────────────────────────────────────

  Rule: Multiple views provide different perspectives on tasks

    Scenario: View tasks in Inbox
      When I navigate to the Inbox view
      Then I should see all tasks not assigned to a quest
      And tasks should be sorted by creation date descending

    Scenario: View tasks in Today view
      When I navigate to the Today view
      Then I should see all tasks due today
      And I should see all overdue tasks
      And I should see tasks from my Smart Daily Brief if generated

    Scenario: View tasks in Upcoming view
      When I navigate to the Upcoming view
      Then I should see tasks grouped by due date
      And I should see the next 14 days by default
      And days with no tasks should still be visible

    Scenario: View completed tasks history
      When I navigate to the Completed view
      Then I should see all completed tasks
      And tasks should be grouped by completion date
      And each task should show the XP that was earned

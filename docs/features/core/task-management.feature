@core @tasks
Feature: Task Management
  As a Waypoint user
  I want to create, organise, and complete tasks
  So that I can track and accomplish my work effectively

  Background:
    Given I am an authenticated user

  Rule: Users can create tasks with minimal friction

    @done
    Scenario: Create a task with only a title
      When I create a task with the title "Buy groceries"
      Then the task "Buy groceries" should appear in my inbox
      And the task should have no due date
      And the task should have no quest assignment
      And the task should have a default difficulty of "Normal"
      And the task should have a status of "Open"

    @done
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

    @wip
    Scenario: Create a task with natural language date parsing
      When I create a task with the title "Call dentist"
      And I set the due date to "next Tuesday"
      Then the due date should resolve to the next occurring Tuesday
      And the task should appear in my upcoming view on that date

    @wip
    Scenario: Create a task via quick-add from any screen
      Given I am on any screen in the application
      When I activate the quick-add shortcut
      And I type "Submit tax return #personal !high ^April 15"
      Then a task "Submit tax return" should be created
      And it should be tagged "personal"
      And it should have priority "High"
      And it should have a due date of April 15

    @done
    Scenario: Reject a task with an empty title
      When I attempt to create a task with an empty title
      Then the task should not be created
      And I should see a validation error indicating a title is required

  Rule: Completing a task triggers progression and records actual effort

    @done
    Scenario: Complete a simple task
      Given I have an open task "Buy groceries"
      When I mark the task "Buy groceries" as complete
      Then the task status should change to "Completed"
      And the completion timestamp should be recorded
      And I should receive XP for the task
      And the task should appear in my completed tasks history

    @done
    Scenario: Complete a task with an estimated time
      Given I have an open task "Write Q2 report" with an estimated time of 2 hours
      When I mark the task as complete
      Then the task status should change to "Completed"
      And I should be prompted to record actual time spent

    @wip
    Scenario: Record actual time spent after completing a task
      Given I have just completed the task "Write Q2 report" with an estimated time of 2 hours
      When I record the actual time spent as "2 hours 45 minutes"
      Then the time estimation variance should be recorded as +37.5%
      And the variance should be visible in my estimation history

    @done
    Scenario: Complete a task that is overdue
      Given I have a task "Submit proposal" that was due 3 days ago
      When I mark the task as complete
      Then the task should be marked as completed
      And the XP awarded should reflect the overdue penalty
      And the XP awarded should still be greater than zero

    @wip
    Scenario: Complete the final task in a quest
      Given I have a quest "Prepare presentation" with 5 tasks
      And 4 of the 5 tasks are completed
      When I complete the remaining task "Do final rehearsal"
      Then the quest "Prepare presentation" should be marked as complete
      And I should receive quest completion bonus XP
      And a quest completion event should appear on my journey timeline

    @done
    Scenario: Complete an already-completed task
      Given I have a completed task "Buy groceries"
      When I attempt to mark the task as complete again
      Then the task status should remain "Completed"
      And no additional XP should be awarded

    @done
    Scenario: Re-open a completed task
      Given I have a completed task "Submit report"
      When I re-open the task
      Then the task status should change to "Open"
      And the XP previously earned for completing it should be deducted
      And the task should reappear in my active task list

  Rule: Users can modify any aspect of an existing task

    @done
    Scenario: Edit a task title
      Given I have an open task "Buy grocries"
      When I edit the task title to "Buy groceries"
      Then the task title should be updated to "Buy groceries"

    @done
    Scenario: Change task priority
      Given I have an open task "Update website" with priority "Low"
      When I change the priority to "High"
      Then the task priority should be "High"
      And the task difficulty should remain unchanged

    @done
    Scenario: Reschedule a task
      Given I have an open task "Team lunch" due on "2026-04-10"
      When I change the due date to "2026-04-17"
      Then the due date should be updated
      And a reschedule event should be recorded against the task

    @done
    Scenario: Add a description to an existing task
      Given I have an open task "Research competitors" with no description
      When I add the description "Focus on gamified productivity apps in the market"
      Then the task description should be saved

    @done
    Scenario: Edit a completed task
      Given I have a completed task "Submit report"
      When I edit the task description to "Updated summary"
      Then the task description should be saved
      And the task status should remain "Completed"

  Rule: Deleting a task requires confirmation and does not award XP

    @done
    Scenario: Delete a task
      Given I have an open task "Cancelled meeting prep"
      When I delete the task "Cancelled meeting prep"
      And I confirm the deletion
      Then the task should be removed from my task list
      And no XP should be awarded or deducted

    @done
    Scenario: Cancel a task deletion
      Given I have an open task "Important work"
      When I delete the task "Important work"
      And I cancel the deletion
      Then the task should remain in my task list

    @wip
    Scenario: Delete a task that belongs to a quest
      Given I have a quest "Launch campaign" containing 4 tasks
      And one task is "Design flyer"
      When I delete the task "Design flyer"
      And I confirm the deletion
      Then the quest "Launch campaign" should show 3 remaining tasks
      And the quest progress should be recalculated

    @done
    Scenario: Delete a completed task
      Given I have a completed task "Old report"
      When I delete the task "Old report"
      And I confirm the deletion
      Then the task should be removed from my completed tasks history
      And the XP earned from the task should be retained

  Rule: Tasks can be filtered, sorted, and searched

    @wip
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

    @done
    Scenario: Sort tasks by due date
      Given I have multiple tasks with different due dates
      When I sort tasks by "Due Date" ascending
      Then tasks should be ordered from earliest due date to latest
      And tasks with no due date should appear at the end

    @done
    Scenario: Sort tasks by priority
      Given I have tasks with priorities "Low", "High", "Medium", and "Critical"
      When I sort tasks by "Priority" descending
      Then tasks should be ordered: Critical, High, Medium, Low

    @wip
    Scenario: Search tasks by keyword
      Given I have 20 tasks with various titles and descriptions
      When I search for "report"
      Then I should see only tasks whose title or description contains "report"

  Rule: Multiple views provide different perspectives on tasks

    @wip
    Scenario: View tasks in Inbox
      When I navigate to the Inbox view
      Then I should see all tasks not assigned to a quest
      And tasks should be sorted by creation date descending

    @wip
    Scenario: View tasks in Today view
      When I navigate to the Today view
      Then I should see all tasks due today
      And I should see all overdue tasks
      And I should see tasks from my Smart Daily Brief if generated

    @wip
    Scenario: View tasks in Upcoming view
      When I navigate to the Upcoming view
      Then I should see tasks grouped by due date
      And I should see the next 14 days by default
      And days with no tasks should still be visible

    @wip
    Scenario: View completed tasks history
      When I navigate to the Completed view
      Then I should see all completed tasks
      And tasks should be grouped by completion date
      And each task should show the XP that was earned

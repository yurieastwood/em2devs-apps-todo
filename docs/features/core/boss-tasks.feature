@core @boss-tasks
Feature: Boss Tasks
  As a Waypoint user
  I want difficult or procrastinated tasks to be surfaced as Boss Tasks
  So that I am supported in tackling my hardest work with special tools and rewards

  Background:
    Given I am an authenticated user

  # ───────────────────────────────────────────
  # Boss Task Detection
  # ───────────────────────────────────────────

  Rule: Tasks are promoted to Boss Task status based on procrastination signals

    Scenario: Task promoted after repeated rescheduling
      Given I have a task "Write architecture decision record"
      And I have rescheduled it 3 or more times
      When the system evaluates my task list
      Then the task should be flagged as a Boss Task
      And I should receive a notification about the promotion
      And the task should display a distinct Boss Task visual indicator

    Scenario: Task promoted based on age and priority
      Given I have a task "Refactor authentication module" with priority "High"
      And the task has been open for more than 14 days
      And the task has not been started
      When the system evaluates my task list
      Then the task should be flagged as a Boss Task

    Scenario: Task promoted based on high difficulty and avoidance
      Given I have a task "Prepare annual tax filing" with difficulty "Hard"
      And I have viewed the task 5 or more times without starting it
      When the system evaluates my task list
      Then the task should be flagged as a Boss Task

    Scenario: User manually promotes a task to Boss Task
      Given I have a task "Have difficult conversation with manager"
      When I manually flag the task as a Boss Task
      Then the task should display the Boss Task indicator
      And I should be offered the Boss Task intervention flow

    Scenario: Low-priority task is not promoted despite age
      Given I have a task "Reorganise bookshelf" with priority "Low"
      And the task has been open for 30 days
      When the system evaluates my task list
      Then the task should not be flagged as a Boss Task
      And it may be suggested for deletion or archival instead

  # ───────────────────────────────────────────
  # Boss Task Intervention
  # ───────────────────────────────────────────

  Rule: Boss Tasks trigger a structured intervention flow to support completion

    Scenario: Offer task breakdown
      Given I have a Boss Task "Write architecture decision record"
      When I open the Boss Task intervention flow
      Then I should be offered the option to break it into smaller subtasks
      And the system should suggest a breakdown based on similar tasks

    Scenario: Accept suggested breakdown
      Given I have a Boss Task "Prepare annual tax filing"
      And the system suggests breaking it into 4 subtasks:
        | Subtask                        |
        | Gather all income documents    |
        | Collect deduction receipts     |
        | Fill in tax form sections      |
        | Review and submit              |
      When I accept the suggested breakdown
      Then 4 subtasks should be created under the Boss Task
      And each subtask should have its own difficulty rating
      And the Boss Task becomes a parent task tracking subtask completion

    Scenario: Offer re-evaluation of task necessity
      Given I have a Boss Task "Redesign landing page"
      When I open the Boss Task intervention flow
      Then I should be offered the option to re-evaluate whether the task still matters
      When I choose to re-evaluate
      Then I should see prompts asking about the task's current relevance
      And I should be able to archive the task without penalty if it is no longer needed

    Scenario: Offer delegation suggestion
      Given I have a Boss Task "Create onboarding documentation"
      And I am a member of a guild
      When I open the Boss Task intervention flow
      Then I should be offered the option to convert it to a shared quest
      And I should be able to assign it to a guild member

    Scenario: Trigger focus mode for a Boss Task
      Given I have a Boss Task "Write Q3 strategy document"
      When I choose to enter Focus Mode for the Boss Task
      Then all notifications should be suppressed
      And my task view should show only this task and its subtasks
      And a timer should begin tracking my focused time
      And I should earn a Focus Mode XP bonus upon completion

  # ───────────────────────────────────────────
  # Boss Task Completion
  # ───────────────────────────────────────────

  Rule: Completing a Boss Task awards significantly more XP and recognition

    Scenario: Complete a Boss Task
      Given I have a Boss Task "Write architecture decision record"
      When I mark the Boss Task as complete
      Then I should receive Boss Task bonus XP on top of standard task XP
      And a Boss Task victory event should appear on my journey timeline
      And a celebration animation should be displayed
      And my "Boss Slayer" achievement counter should increment

    Scenario: Complete a Boss Task within Focus Mode
      Given I am in Focus Mode working on the Boss Task "Write Q3 strategy document"
      And I have been in Focus Mode for 45 minutes
      When I complete the Boss Task
      Then I should receive standard task XP
      And I should receive Boss Task bonus XP
      And I should receive Focus Mode bonus XP
      And the total XP should be displayed in a combined breakdown

    Scenario: Boss Task completion contributes to title progression
      Given I have completed 9 Boss Tasks total
      When I complete my 10th Boss Task
      Then I should earn the title "Boss Slayer"
      And the title should be visible on my profile

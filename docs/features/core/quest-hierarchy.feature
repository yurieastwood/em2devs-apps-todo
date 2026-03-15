@core @quests
Feature: Quest Hierarchy
  As a Waypoint user
  I want to organise tasks into quests, epics, and sagas
  So that individual tasks connect to meaningful larger goals

  Background:
    Given I am an authenticated user

  Rule: Quests are meaningful clusters of related tasks with a clear outcome

    @done
    Scenario: Create a quest
      When I create a quest with the following details:
        | Field       | Value                               |
        | Title       | Prepare conference talk              |
        | Description | Write and rehearse DDD talk for NDC  |
        | Due Date    | 2026-06-01                          |
      Then the quest "Prepare conference talk" should be created
      And it should appear in my quest list
      And it should have a progress of 0%

    @done
    Scenario: Add tasks to a quest
      Given I have a quest "Prepare conference talk"
      When I add the following tasks to the quest:
        | Title                    |
        | Write abstract           |
        | Create slide deck        |
        | Build demo project       |
        | First rehearsal          |
        | Final rehearsal          |
      Then the quest should contain 5 tasks
      And the quest progress should be 0%

    @done
    Scenario: Quest progress updates as tasks complete
      Given I have a quest "Prepare conference talk" with 5 tasks
      And 0 tasks are completed
      When I complete the task "Write abstract"
      Then the quest progress should be 20%

    Scenario: Complete a quest
      Given I have a quest "Prepare conference talk" with 5 tasks
      And 4 tasks are completed
      When I complete the remaining task "Final rehearsal"
      Then the quest progress should be 100%
      And the quest status should change to "Completed"
      And I should receive quest completion bonus XP
      And a celebration animation should be displayed

    Scenario: View quest details
      Given I have a quest "Prepare conference talk" with 5 tasks
      And 3 tasks are completed
      When I view the quest details
      Then I should see the quest title and description
      And I should see a progress bar showing 60%
      And I should see all 5 tasks with their statuses
      And I should see the total XP earned so far
      And I should see the estimated remaining effort

    Scenario: Move a task between quests
      Given I have a quest "Work tasks" containing the task "Update docs"
      And I have a quest "Side project" with 2 tasks
      When I move the task "Update docs" to the quest "Side project"
      Then "Work tasks" should no longer contain "Update docs"
      And "Side project" should contain 3 tasks

    Scenario: Remove a task from a quest without deleting it
      Given I have a quest "Sprint work" containing the task "Fix CSS bug"
      When I unassign the task "Fix CSS bug" from the quest
      Then the task should appear in my inbox
      And the quest progress should be recalculated

    Scenario: Delete a quest
      Given I have a quest "Abandoned project" containing 3 tasks
      When I delete the quest "Abandoned project"
      And I confirm the deletion
      Then the quest should be removed from my quest list
      And the 3 tasks should be moved to my inbox
      And the quest XP bonus should not be affected for completed tasks

    Scenario: A quest cannot belong to more than one epic
      Given I have a quest "Build authentication" assigned to the epic "Launch MVP"
      When I attempt to assign the quest to the epic "Side Project"
      Then I should see a message indicating the quest already belongs to an epic
      And I should be offered the option to move it instead

  Rule: Epics are multi-week objectives spanning several quests, with equal quest weighting

    Scenario: Create an epic
      When I create an epic with the following details:
        | Field       | Value                                  |
        | Title       | Launch MVP                             |
        | Description | Ship the first public version of the app |
        | Target Date | 2026-09-01                             |
      Then the epic "Launch MVP" should be created
      And it should appear in my epic list

    Scenario: Assign quests to an epic
      Given I have an epic "Launch MVP"
      And I have the following quests:
        | Quest Title             |
        | Build authentication    |
        | Implement task engine   |
        | Design UI               |
        | Beta testing            |
      When I assign all four quests to the epic "Launch MVP"
      Then the epic should contain 4 quests
      And the epic progress should reflect aggregate quest progress

    Scenario: Epic progress reflects quest completion with equal weighting
      Given I have an epic "Launch MVP" with 4 quests
      And each quest contributes equally to epic progress regardless of task count
      And the quest "Build authentication" is 100% complete
      And the quest "Implement task engine" is 50% complete
      And the other quests are 0% complete
      When I view the epic progress
      Then the epic progress should be 37.5%

    Scenario: Complete an epic
      Given I have an epic "Launch MVP" with 4 quests
      And 3 quests are completed
      When the final quest is completed
      Then the epic status should change to "Completed"
      And I should receive epic completion bonus XP
      And a milestone event should appear on my journey timeline

    Scenario: Delete an epic
      Given I have an epic "Abandoned initiative" containing 3 quests
      When I delete the epic "Abandoned initiative"
      And I confirm the deletion
      Then the epic should be removed from my epic list
      And the 3 quests should remain intact but no longer belong to any epic

    Scenario: Remove a quest from an epic
      Given I have an epic "Launch MVP" with 4 quests
      When I remove the quest "Beta testing" from the epic
      Then the epic should contain 3 quests
      And the epic progress should be recalculated

  Rule: Sagas are life-chapter goals representing major personal ambitions
    
    @premium
    Scenario: Create a saga
      Given I have a premium subscription
      When I create a saga with the following details:
        | Field       | Value                                     |
        | Title       | Launch my SaaS business                   |
        | Description | Go from idea to paying customers           |
        | Vision      | Build a sustainable product that solves a real problem |
      Then the saga "Launch my SaaS business" should be created
      And it should appear in my saga view
      And it should have no target date by default

    @premium
    Scenario: Assign epics to a saga
      Given I have a saga "Launch my SaaS business"
      And I have epics "Launch MVP" and "Acquire first 100 users"
      When I assign both epics to the saga
      Then the saga should contain 2 epics
      And the saga progress should reflect aggregate epic progress

    @premium
    Scenario: View saga timeline
      Given I have a saga "Launch my SaaS business" with 3 epics
      And work has been ongoing for 4 months
      When I view the saga timeline
      Then I should see a visual representation of progress over time
      And I should see completed and in-progress epics
      And I should see a projected completion trajectory

    Scenario: Free-tier user attempts to create a saga
      Given I have a free-tier account
      When I attempt to create a saga
      Then I should see a message explaining sagas are a premium feature
      And I should be offered the option to upgrade
      And I should still be able to create tasks, quests, and epics

    @premium
    Scenario: An epic cannot belong to more than one saga
      Given I have an epic "Launch MVP" assigned to the saga "Launch my SaaS business"
      When I attempt to assign the epic to the saga "Career growth"
      Then I should see a message indicating the epic already belongs to a saga
      And I should be offered the option to move it instead

  Rule: Users can navigate the full hierarchy and see how tasks connect to goals

    Scenario: View task context within hierarchy
      Given I have a task "Write unit tests" in the quest "Build authentication"
      And the quest belongs to the epic "Launch MVP"
      And the epic belongs to the saga "Launch my SaaS business"
      When I view the task "Write unit tests"
      Then I should see the full breadcrumb: Saga > Epic > Quest > Task
      And each level should be clickable for navigation

    Scenario: View all unassigned tasks
      Given I have 10 tasks total
      And 6 tasks are assigned to quests
      And 4 tasks are not assigned to any quest
      When I view unassigned tasks
      Then I should see exactly 4 tasks
      And I should be offered suggestions to group them into quests

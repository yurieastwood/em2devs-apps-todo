@social @shared-quests @premium
Feature: Shared Quests
  As a Waypoint user
  I want to collaborate on quests where multiple people contribute tasks
  So that we can work toward shared goals together

  Background:
    Given I am an authenticated user
    And I have a premium subscription

  # ───────────────────────────────────────────
  # Shared Quest Creation
  # ───────────────────────────────────────────

  Rule: Shared quests allow multiple contributors toward a common outcome

    Scenario: Create a shared quest and invite participants
      When I create a shared quest with the following details:
        | Field       | Value                            |
        | Title       | Plan summer road trip            |
        | Description | Organise the group road trip     |
        | Due Date    | 2026-06-15                       |
      And I invite users "Jordan" and "Alex" to collaborate
      Then the shared quest should be created
      And "Jordan" and "Alex" should receive invitations
      And the quest should appear in all participants' quest lists once accepted

    Scenario: Add tasks to a shared quest
      Given I am a participant in the shared quest "Plan summer road trip"
      When I add a task "Book accommodation" and assign it to "Jordan"
      And I add a task "Create packing list" and assign it to myself
      Then both tasks should appear on the shared quest
      And each participant should see their own assigned tasks highlighted

    Scenario: Any participant can add tasks
      Given "Jordan" is a participant in the shared quest "Plan summer road trip"
      When "Jordan" adds a task "Research restaurants" and assigns it to "Alex"
      Then the task should appear on the shared quest board
      And "Alex" should be notified of the new assignment

    Scenario: View shared quest progress
      Given the shared quest "Plan summer road trip" has 6 tasks across 3 participants
      And 3 tasks are completed
      When any participant views the quest
      Then they should see 50% progress
      And they should see a breakdown of each participant's contributions
      And they should see which tasks are completed, in progress, and pending

  # ───────────────────────────────────────────
  # Shared Quest Completion
  # ───────────────────────────────────────────

  Rule: All participants benefit when a shared quest is completed

    Scenario: Shared quest completed
      Given the shared quest "Plan summer road trip" has 6 tasks
      And 5 tasks are completed by various participants
      When the final task is completed
      Then the shared quest should be marked as complete
      And all participants should receive shared quest completion bonus XP
      And all participants should see the completion on their journey timeline

    Scenario: Participant leaves a shared quest
      Given I am a participant in "Plan summer road trip"
      And I have 2 tasks assigned to me
      When I leave the shared quest
      Then my assigned tasks should become unassigned
      And the remaining participants should be notified
      And I should retain XP for tasks I already completed

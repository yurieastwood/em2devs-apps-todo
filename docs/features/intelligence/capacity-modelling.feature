@intelligence @capacity @premium
Feature: Capacity Modelling
  As a Waypoint user
  I want the system to learn my realistic daily throughput
  So that I am warned when I am overcommitting and can plan more effectively

  Background:
    Given I am an authenticated user
    And I have a premium subscription
    And I have at least 14 days of task completion history

  # ───────────────────────────────────────────
  # Capacity Learning
  # ───────────────────────────────────────────

  Rule: The system builds a personal capacity model from historical data

    Scenario: Capacity model established from history
      Given I have completed tasks for 30 days
      And my average daily completion is 6 tasks on weekdays
      And my average daily completion is 3 tasks on weekends
      When the system builds my capacity model
      Then my weekday capacity should be approximately 6 tasks
      And my weekend capacity should be approximately 3 tasks

    Scenario: Capacity model accounts for task difficulty
      Given my capacity model shows I complete approximately 6 "Normal" tasks per day
      When I have 4 "Hard" tasks and 2 "Normal" tasks scheduled today
      Then the system should calculate this as exceeding my typical capacity
      Because hard tasks consume more capacity than normal tasks

    Scenario: Capacity model updates as behaviour changes
      Given my historical weekday capacity is 6 tasks
      And over the last 3 weeks I have consistently completed 8 tasks on weekdays
      When the system recalibrates my capacity model
      Then my weekday capacity should adjust upward toward 8

  # ───────────────────────────────────────────
  # Overcommitment Warnings
  # ───────────────────────────────────────────

  Rule: Users are warned when scheduled tasks exceed their realistic capacity

    Scenario: Overcommitment warning on daily view
      Given my weekday capacity is 6 tasks
      And I have 10 tasks scheduled for today
      When I view my Today tasks
      Then I should see a capacity warning indicator
      And the warning should state something like "You typically complete 6 tasks on Wednesdays. You have 10 scheduled. Consider reprioritising."

    Scenario: Overcommitment warning when adding tasks
      Given my weekday capacity is 6 tasks
      And I already have 6 tasks scheduled for tomorrow
      When I schedule a 7th task for tomorrow
      Then I should see a gentle warning that tomorrow exceeds my typical capacity
      And I should still be able to add the task

    Scenario: No warning when within capacity
      Given my weekday capacity is 6 tasks
      And I have 4 tasks scheduled for today
      When I view my Today tasks
      Then I should not see any capacity warning

    Scenario: Reprioritisation assistance offered
      Given I have 12 tasks scheduled for today
      And my capacity is 6 tasks
      When I accept the offer to reprioritise
      Then the system should suggest which tasks to defer based on priority and deadlines
      And I should be able to accept, modify, or reject each suggestion
      And deferred tasks should be rescheduled to the next available day within capacity

  # ───────────────────────────────────────────
  # Capacity Insights
  # ───────────────────────────────────────────

  Rule: Users can view their capacity model and trends

    Scenario: View weekly capacity overview
      When I navigate to my capacity insights
      Then I should see my average daily capacity for each day of the week
      And I should see a trend line showing capacity changes over the last 90 days
      And I should see my most and least productive days

    Scenario: Capacity insight informs planning
      Given my capacity model shows Mondays average 8 tasks and Fridays average 4 tasks
      When I am planning tasks for the week
      Then the system should suggest front-loading harder tasks to Monday
      And the system should suggest lighter loads for Friday

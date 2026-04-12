@intelligence @capacity @premium
Feature: Capacity Modelling
  As a Waypoint user
  I want the system to learn my realistic daily throughput
  So that I am warned when I am overcommitting and can plan more effectively

  Background:
    Given I am an authenticated user
    And I have a premium subscription
    And I have at least 14 days of task completion history

  Rule: The system builds a personal capacity model from historical data

    @wip
    Scenario: Capacity model established from history
      Given I have completed tasks for 30 days
      And my average daily completion is 6 tasks on weekdays
      And my average daily completion is 3 tasks on weekends
      When the system builds my capacity model
      Then my weekday capacity should be approximately 6 tasks
      And my weekend capacity should be approximately 3 tasks

    @wip
    Scenario: Capacity model accounts for task difficulty weighting
      Given my capacity model shows I complete approximately 6 "Normal" tasks per day
      When I have 4 "Hard" tasks and 2 "Normal" tasks scheduled today
      Then the system should calculate this as exceeding my typical capacity
      Because hard tasks consume 2 capacity units while normal tasks consume 1

    @wip
    Scenario: Tasks with no difficulty assigned default to Normal
      Given my capacity model shows I complete approximately 6 "Normal" tasks per day
      When I create a task without specifying a difficulty level
      Then the task should default to "Normal" difficulty
      And it should count as 1 capacity unit in my daily plan

    @wip
    Scenario: Capacity model updates gradually as behaviour changes
      Given my historical weekday capacity is 6 tasks
      And over the last 3 weeks I have consistently completed 8 tasks on weekdays
      When the system recalibrates my capacity model
      Then my weekday capacity should adjust upward gradually based on actual completion patterns
      And the adjustment should not exceed 1 task per recalibration cycle

    @wip
    Scenario: Weekend capacity may differ from weekday capacity
      Given I have completed tasks for 30 days
      And my average daily completion is 6 tasks on weekdays
      And my average daily completion is 2 tasks on Saturdays
      And my average daily completion is 3 tasks on Sundays
      When the system builds my capacity model
      Then my Saturday capacity should be approximately 2 tasks
      And my Sunday capacity should be approximately 3 tasks
      And weekend capacity should be evaluated independently from weekday capacity

  Rule: Users are warned when scheduled tasks exceed their realistic capacity

    @wip
    Scenario: Overcommitment warning on daily view
      Given my weekday capacity is 6 tasks
      And I have 10 tasks scheduled for today
      When I view my Today tasks
      Then I should see a capacity warning indicator
      And the warning should state something like "You typically complete 6 tasks on Wednesdays. You have 10 scheduled. Consider reprioritising."

    @wip
    Scenario: Overcommitment warning when adding tasks
      Given my weekday capacity is 6 tasks
      And I already have 6 tasks scheduled for tomorrow
      When I schedule a 7th task for tomorrow
      Then I should see a gentle warning that tomorrow exceeds my typical capacity
      And I should still be able to add the task

    @wip
    Scenario: No warning when within capacity
      Given my weekday capacity is 6 tasks
      And I have 4 tasks scheduled for today
      When I view my Today tasks
      Then I should not see any capacity warning

    @wip
    Scenario: Capacity warnings dismissed repeatedly
      Given my weekday capacity is 6 tasks
      And I have dismissed the capacity warning 3 times this week
      When I exceed my capacity again
      Then the system should still show the capacity indicator in a reduced, non-intrusive form
      And it should not show a modal or interruptive warning
      And the full warning should resume the following week

    @wip
    Scenario: Reprioritisation assistance offered
      Given I have 12 tasks scheduled for today
      And my capacity is 6 tasks
      When I accept the offer to reprioritise
      Then the system should suggest which tasks to defer based on priority and deadlines
      And I should be able to accept, modify, or reject each suggestion
      And deferred tasks should be rescheduled to the next available day within capacity

  Rule: Users can view their capacity model and trends

    @wip
    Scenario: View weekly capacity overview
      When I navigate to my capacity insights
      Then I should see my average daily capacity for each day of the week
      And I should see a trend line showing capacity changes over the last 90 days
      And I should see my most and least productive days

    @wip
    Scenario: Capacity insight informs planning
      Given my capacity model shows Mondays average 8 tasks and Fridays average 4 tasks
      When I am planning tasks for the week
      Then the system should suggest front-loading harder tasks to Monday
      And the system should suggest lighter loads for Friday

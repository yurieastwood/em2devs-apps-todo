@intelligence @energy
Feature: Energy-Aware Scheduling
  As a Waypoint user
  I want tasks surfaced based on my current energy level
  So that I tackle hard work when I am sharp and routine work when I am depleted

  Background:
    Given I am an authenticated user

  Rule: Users can report or have their energy level inferred

    @wip
    Scenario: Manually set energy level at start of session
      When I open Waypoint for my first session of the day
      Then I should see an optional energy check-in prompt
      When I set my energy level to "High"
      Then my current energy should be recorded as "High"
      And my task suggestions should prioritise difficult tasks

    @wip
    Scenario: Skip energy check-in with sufficient history
      Given I have at least 14 days of task completion data
      When I open Waypoint for my first session of the day
      And I dismiss the energy check-in prompt
      Then the system should infer my energy from historical patterns
      And the prompt should not appear again until the next session

    @wip
    Scenario: Skip energy check-in on day 1 with no historical data
      Given I have no task completion history
      When I open Waypoint for my first session of the day
      And I dismiss the energy check-in prompt
      Then the system should default my energy level to "Medium"
      And tasks should be shown in standard priority order
      And the prompt should not appear again until the next session

    @wip
    Scenario Outline: Energy level affects task surfacing
      Given my current energy level is "<energy>"
      When I view my Today tasks
      Then the task ordering should prioritise "<priority_type>" tasks

      Examples:
        | energy  | priority_type                          |
        | High    | Hard and complex tasks                 |
        | Medium  | Normal difficulty tasks                |
        | Low     | Easy, routine, and administrative tasks |

    @wip
    Scenario: New user with insufficient data for pattern inference
      Given I have fewer than 14 days of task completion data
      When I dismiss the energy check-in prompt
      Then the system should display a message "We're still learning your energy patterns — check in daily for personalised suggestions after 14 days"
      And the system should default my energy level to "Medium"
      And tasks should be shown in standard priority order

    @wip
    Scenario: System infers energy from time-of-day patterns
      Given I have at least 14 days of task completion data
      And the system has detected that I complete hard tasks most often between 9 AM and 12 PM
      And it is currently 10 AM
      And I did not provide an energy check-in
      When I view my Today tasks
      Then the system should infer "High" energy
      And difficult tasks should be surfaced first

  Rule: The system learns individual energy patterns over time

    @wip
    Scenario: Energy pattern detected across weeks
      Given I have consistently reported "High" energy on weekday mornings
      And I have consistently reported "Low" energy on Friday afternoons
      When the system analyses my energy patterns
      Then it should build a weekly energy profile for me
      And the profile should be visible in my productivity insights

    @wip
    Scenario: Energy inference improves with data
      Given I have provided energy check-ins for 14 days
      When I skip a check-in on a typical Wednesday morning
      Then the system should infer my energy based on the pattern
      And the confidence of the inference should be moderate
      Given I have provided energy check-ins for 60 days
      When I skip a check-in on a typical Wednesday morning
      Then the confidence of the inference should be high

  Rule: Task recommendations adapt to energy levels throughout the day

    @wip
    Scenario: Mid-day energy shift recommendation
      Given my energy was "High" this morning
      And it is now 2 PM
      And the system detects I typically experience an energy dip at this time
      When I return to my task list
      Then the system should suggest switching to easier tasks
      And a gentle prompt should say something like "Energy usually dips around now — lighter tasks might be a good fit"

    @wip
    Scenario: Mid-day re-check-in after rapid energy fluctuation
      Given my energy was "High" this morning at 9 AM
      And it is now 11 AM
      When I manually re-check-in and set my energy level to "Low"
      Then my current energy should be updated to "Low"
      And my task suggestions should immediately reprioritise to show easier tasks first
      And the system should record the rapid fluctuation for future pattern analysis

    @wip
    Scenario: Energy-aware reordering does not hide tasks
      Given my energy level is "Low"
      When I view my Today tasks
      Then difficult tasks should still be visible and accessible
      But they should be ordered below easier tasks
      And a label should indicate the ordering is energy-aware

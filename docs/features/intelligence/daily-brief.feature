@intelligence @daily-brief
Feature: Smart Daily Brief
  As a Waypoint user
  I want a personalised daily plan each morning
  So that I start my day with clarity and focus on the right tasks

  Background:
    Given I am an authenticated user
    And I have reached at least level 5

  Rule: The daily brief generates a recommended day based on priorities and patterns

    Scenario: Daily brief generated on first session
      Given it is a new day and I have not opened Waypoint yet
      And I have 8 tasks due today or overdue
      And I have energy pattern data available
      When I open Waypoint
      Then a Smart Daily Brief should be generated and displayed
      And it should recommend a prioritised task sequence for the day
      And the sequence should account for my energy patterns
      And the sequence should account for task deadlines and priorities

    @premium
    Scenario: Daily brief factors in calendar blocks
      Given I have a premium subscription with calendar integration
      And I have a 2-hour meeting block from 10 AM to 12 PM
      And I have 6 tasks to schedule today
      When the daily brief is generated
      Then no tasks should be suggested during the 10 AM to 12 PM block
      And harder tasks should be suggested for my peak energy windows outside the meeting

    Scenario: Daily brief without calendar integration
      Given I do not have calendar integration enabled
      And I have 6 tasks to schedule today
      And I have energy pattern data available
      When the daily brief is generated
      Then the brief should be generated from tasks only
      And it should recommend a prioritised task sequence based on energy patterns and deadlines
      And no calendar-related scheduling adjustments should be applied

    Scenario: Daily brief highlights overdue tasks
      Given I have 3 overdue tasks and 5 tasks due today
      When the daily brief is generated
      Then overdue tasks should appear at the top of the brief with a clear indicator
      And the brief should suggest addressing at least 1 overdue task first

    Scenario: Daily brief respects capacity model
      Given my capacity model indicates I typically complete 6 tasks on this day of the week
      And I have 10 tasks due today
      When the daily brief is generated
      Then the brief should recommend 6 priority tasks as the core plan
      And the remaining 4 should be listed as "if time allows"
      And I should see a note about today exceeding typical capacity

  Rule: Users can accept, modify, or dismiss the daily brief

    Scenario: Accept the daily brief as-is
      Given the daily brief recommends 6 tasks in a specific order
      When I accept the daily brief
      Then my Today view should reorder to match the brief
      And a "Following daily brief" indicator should be visible

    Scenario: Modify the daily brief
      Given the daily brief recommends 6 tasks
      When I reorder the tasks in the brief
      And I remove 1 task and add a different one
      And I confirm the modified brief
      Then my Today view should reflect my modifications
      And the system should learn from my modifications for future briefs

    Scenario: User modifies brief to exceed capacity limit
      Given the daily brief recommends 6 tasks matching my capacity model
      When I add additional tasks to the brief beyond my capacity of 6
      Then the system should show a gentle warning "This plan exceeds your typical daily capacity of 6 tasks — you may want to mark some as 'if time allows'"
      And I should still be able to confirm the modified brief

    Scenario: Dismiss the daily brief
      Given the daily brief is displayed
      When I dismiss the daily brief
      Then my Today view should show the default task ordering
      And the brief should not reappear until the next day

    Scenario: Brief not generated when insufficient tasks
      Given I have fewer than 2 tasks due today
      When I open Waypoint
      Then no daily brief should be generated
      And I should see my standard Today view

  Rule: The daily brief improves based on what users actually complete

    Scenario: Brief accuracy improves with feedback
      Given I have used the daily brief for 14 days
      And I consistently move creative tasks earlier and defer admin tasks
      When the next daily brief is generated
      Then creative tasks should be scheduled earlier in the day
      And administrative tasks should be suggested later

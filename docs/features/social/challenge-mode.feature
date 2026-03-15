@social @challenges @premium
Feature: Challenge Mode
  As a Waypoint user
  I want to participate in time-limited competitions
  So that I have extra motivation through friendly competition

  Background:
    Given I am an authenticated user
    And I have a premium subscription
    And I have reached at least level 10

  # ───────────────────────────────────────────
  # Challenge Creation
  # ───────────────────────────────────────────

  Rule: Challenges are opt-in, time-limited competitions

    Scenario: View available challenges
      When I navigate to the challenges section
      Then I should see currently active global challenges
      And I should see guild-specific challenges if I belong to a guild
      And each challenge should show its duration, rules, and reward

    Scenario: Join a global challenge
      Given there is an active challenge "Weekend Warrior: Complete the most tasks this weekend"
      And the challenge runs from Saturday 00:00 to Sunday 23:59
      When I opt into the challenge
      Then I should be registered as a participant
      And my task completions during the window should count toward the challenge
      And I should see a challenge progress tracker

    Scenario: Create a guild challenge
      Given I am a member of "Side Project Squad"
      When I create a guild challenge with the following details:
        | Field       | Value                                        |
        | Title       | Boss Rush: Clear all Boss Tasks before Friday |
        | Duration    | Monday to Friday                             |
        | Objective   | Complete the most Boss Tasks                 |
        | Reward      | Seasonal cosmetic + bonus XP                 |
      Then all guild members should receive an invitation to participate
      And the challenge should appear on the guild board

  # ───────────────────────────────────────────
  # Challenge Participation
  # ───────────────────────────────────────────

  Rule: Challenge progress is tracked in real-time among participants

    Scenario: Track challenge progress
      Given I am participating in "Weekend Warrior"
      And I have completed 8 tasks so far
      When I view the challenge progress
      Then I should see my task count of 8
      And I should see my current rank among participants
      And I should see the top 5 participants and their counts

    Scenario: Challenge ends and results are announced
      Given the "Weekend Warrior" challenge period has ended
      And I completed 12 tasks, ranking 3rd overall
      When the challenge concludes
      Then I should receive a notification with my final rank
      And the top 3 participants should receive seasonal cosmetics
      And all participants should receive participation XP
      And results should be visible in the challenge history

    Scenario: Challenge does not penalise non-participation
      Given there is an active challenge "Weekend Warrior"
      And I choose not to participate
      Then I should not see any penalty or negative indicator
      And my regular task completions should not be affected
      And I should be able to view challenge results as a spectator

  # ───────────────────────────────────────────
  # Challenge Integrity
  # ───────────────────────────────────────────

  Rule: Challenges use anti-gaming measures to ensure fair competition

    Scenario: Trivial task spam during a challenge
      Given I am participating in a "most tasks completed" challenge
      And I create and immediately complete 30 trivial tasks in 10 minutes
      When the system evaluates my challenge activity
      Then only tasks meeting a minimum difficulty threshold should count
      And a notification should explain the quality requirement

    Scenario: Tasks created before challenge do not count
      Given the challenge "Weekend Warrior" starts on Saturday
      And I created 5 tasks on Friday but complete them on Saturday
      When the tasks are evaluated for the challenge
      Then all 5 tasks should count because they were completed during the challenge window
      But tasks completed before Saturday should not count

@social @leaderboards @premium
Feature: Leaderboards
  As a Waypoint user
  I want to see how my productivity compares to similar users
  So that healthy competition keeps me motivated without being demoralising

  Background:
    Given I am an authenticated user
    And I have a premium subscription
    And I have reached at least level 10

  Rule: Leaderboards compare users within similar cohorts only

    @wip
    Scenario: View my leaderboard cohort
      Given I am level 15
      When I view the leaderboard
      Then I should be placed in a cohort of users within 10 levels of my current level
      And I should see my rank within this cohort

    Scenario: Leaderboard ranks by weekly XP
      Given I am in a leaderboard cohort
      When I view the weekly leaderboard
      Then users should be ranked by XP earned in the current week
      And I should see the top 10 users in my cohort
      And I should see my own rank even if outside the top 10

    Scenario: Leaderboard resets weekly
      Given it is the start of a new week
      When I view the leaderboard
      Then all weekly XP totals should be reset to zero
      And last week's final standings should be viewable in history

    Scenario: Cohort assignment when levelling up mid-week
      Given I am level 19 and at the top of my cohort
      When I level up to 20 during the current week
      Then I should remain in my current cohort until the weekly reset
      And my new cohort should take effect at the start of the next week

    Scenario: Weekly leaderboard resets at a consistent time
      Given it is the start of a new week
      When the weekly leaderboard resets
      Then the reset should occur at Monday 00:00 UTC
      And all users should see the new week begin at the same moment regardless of timezone

    Scenario: Level-mismatched users never appear together
      Given I am level 12
      And there is a user at level 45 who earned 500 XP this week
      When I view the leaderboard
      Then the level-45 user should not appear in my cohort

  Rule: Multiple leaderboard types cater to different motivations

    @wip
    Scenario Outline: View a leaderboard by type
      When I select the "<leaderboard>" leaderboard
      Then I should see cohort members ranked by <ranking_metric>

      Examples:
        | leaderboard    | ranking_metric                 |
        | Weekly XP      | XP earned this week            |
        | Longest Streak | current active streak length   |
        | Quest Closer   | quests completed this season   |

    Scenario: View guild leaderboard
      Given I am a member of a guild
      When I select the "Guild" leaderboard
      Then I should see all guild members ranked by contribution this week
      And the guild leaderboard should be separate from the global cohort leaderboard

  Rule: Users control their leaderboard visibility

    @wip
    Scenario: Opt out of leaderboards
      When I navigate to my privacy settings
      And I disable leaderboard participation
      Then my name should not appear on any leaderboard
      And I should still be able to view leaderboards as a spectator
      And I should see a placeholder for my rank position

    Scenario: Anonymous leaderboard participation
      When I enable anonymous leaderboard mode
      Then my profile should appear on leaderboards as "Anonymous Questor"
      And my level and XP should be visible but not my username or title

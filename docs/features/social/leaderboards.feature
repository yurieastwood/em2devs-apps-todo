@social @leaderboards @premium
Feature: Leaderboards
  As a Waypoint user
  I want to see how my productivity compares to similar users
  So that healthy competition keeps me motivated without being demoralising

  Background:
    Given I am an authenticated user
    And I have a premium subscription
    And I have reached at least level 10

  # ───────────────────────────────────────────
  # Cohort-Based Ranking
  # ───────────────────────────────────────────

  Rule: Leaderboards compare users within similar cohorts only

    Scenario: View my leaderboard cohort
      Given I am level 15
      When I view the leaderboard
      Then I should be placed in a cohort of users within a similar level range
      And I should not be compared to users more than 10 levels above or below me
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

    Scenario: Level-mismatched users never appear together
      Given I am level 8
      And there is a user at level 45 who earned 500 XP this week
      When I view the leaderboard
      Then the level-45 user should not appear in my cohort

  # ───────────────────────────────────────────
  # Leaderboard Types
  # ───────────────────────────────────────────

  Rule: Multiple leaderboard types cater to different motivations

    Scenario: View weekly XP leaderboard
      When I select the "Weekly XP" leaderboard
      Then I should see cohort members ranked by XP earned this week

    Scenario: View streak leaderboard
      When I select the "Longest Streak" leaderboard
      Then I should see cohort members ranked by current active streak length

    Scenario: View quest completion leaderboard
      When I select the "Quest Closer" leaderboard
      Then I should see cohort members ranked by quests completed this season

    Scenario: View guild leaderboard
      Given I am a member of a guild
      When I select the "Guild" leaderboard
      Then I should see all guild members ranked by contribution this week
      And the guild leaderboard should be separate from the global cohort leaderboard

  # ───────────────────────────────────────────
  # Privacy and Opt-Out
  # ───────────────────────────────────────────

  Rule: Users control their leaderboard visibility

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

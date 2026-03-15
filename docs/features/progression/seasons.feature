@progression @seasons
Feature: Seasons
  As a Waypoint user
  I want quarterly seasons with themed challenges and refreshed leaderboards
  So that long-term engagement stays fresh without invalidating my permanent progress

  Background:
    Given I am an authenticated user

  # ───────────────────────────────────────────
  # Season Structure
  # ───────────────────────────────────────────

  Rule: Seasons run quarterly and introduce themed content

    Scenario: New season begins
      Given the current season "Season of the Architect" is ending
      When the new season "Season of the Explorer" begins
      Then I should see an announcement for the new season
      And the seasonal leaderboard should reset to zero
      And my permanent level and XP should remain unchanged
      And the new season's themed challenges should be available
      And the new season's cosmetics should be previewed

    Scenario: View current season details
      When I navigate to the seasons view
      Then I should see the current season name and theme
      And I should see the number of days remaining in the season
      And I should see the seasonal quest line with progress
      And I should see the seasonal leaderboard
      And I should see the seasonal cosmetics I can earn

    Scenario: View past season history
      Given I have participated in 3 previous seasons
      When I navigate to past seasons
      Then I should see a summary of each past season
      And each summary should show my final rank
      And each summary should show the cosmetics I earned
      And each summary should show the seasonal XP I accumulated

  # ───────────────────────────────────────────
  # Seasonal Quest Line
  # ───────────────────────────────────────────

  Rule: Each season has a themed quest line that provides guided challenges

    Scenario: Start the seasonal quest line
      Given a new season has begun with a quest line of 8 stages
      When I view the seasonal quest line
      Then I should see stage 1 as available
      And stages 2-8 should be locked
      And each stage should preview its challenge theme

    Scenario: Complete a seasonal quest line stage
      Given I am on stage 3 of the seasonal quest line
      And stage 3 requires completing 5 tasks rated "Hard" or above
      And I have completed 4 qualifying tasks
      When I complete a 5th qualifying hard task
      Then stage 3 should be marked as complete
      And I should receive seasonal XP
      And stage 4 should become available
      And I should earn the stage 3 cosmetic reward

    Scenario: Complete the full seasonal quest line
      Given I have completed stages 1 through 7 of the seasonal quest line
      When I complete stage 8
      Then I should receive a seasonal completion bonus
      And I should earn the exclusive season-completion cosmetic
      And a seasonal completion badge should appear on my profile

  # ───────────────────────────────────────────
  # Seasonal Leaderboard
  # ───────────────────────────────────────────

  @premium
  Rule: Seasonal leaderboards reset each quarter and rank users by seasonal XP

    Scenario: View seasonal leaderboard
      Given the current season is 6 weeks in
      When I view the seasonal leaderboard
      Then I should see my rank among my cohort
      And I should see my seasonal XP total
      And I should see the top 10 users in my cohort
      And users should be compared within similar level ranges

    Scenario: Season ends and final ranks are recorded
      Given the current season is ending
      And my seasonal rank is 15th in my cohort
      When the season concludes
      Then my final rank should be permanently recorded
      And I should receive a rank-based seasonal reward
      And the leaderboard should become read-only for the past season

  # ───────────────────────────────────────────
  # Seasonal Cosmetics
  # ───────────────────────────────────────────

  Rule: Seasonal cosmetics are limited to the season and cannot be earned later

    Scenario: Earn a seasonal cosmetic
      Given the current season offers a "Crystal Compass" profile badge
      And the badge requires completing the seasonal quest line stage 5
      When I complete stage 5
      Then the "Crystal Compass" badge should be added to my collection
      And it should be marked as a seasonal exclusive

    Scenario: Seasonal cosmetic unavailable after season ends
      Given the "Season of the Architect" offered the "Blueprint Frame" avatar border
      And I did not earn it during that season
      When the "Season of the Architect" ends
      Then the "Blueprint Frame" should no longer be earnable
      And it should appear as a locked seasonal item in my collection history

    Scenario: Display seasonal cosmetic on profile
      Given I have earned the "Crystal Compass" badge
      When I select it as my active profile badge
      Then it should be displayed on my profile
      And other users should see it marked with its season of origin

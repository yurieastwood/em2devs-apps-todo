@progression @seasons
Feature: Seasons
  As a Waypoint user
  I want quarterly seasons with themed challenges and refreshed leaderboards
  So that long-term engagement stays fresh without invalidating my permanent progress

  Background:
    Given I am an authenticated user

  Rule: Seasons run quarterly and introduce themed content

    @done
    Scenario: New season begins
      Given the current season "Season of the Architect" is ending
      When the new season "Season of the Explorer" begins
      Then I should see an announcement for the new season
      And the seasonal leaderboard should reset to zero
      And my permanent level and XP should remain unchanged
      And the new season's themed challenges should be available
      And the new season's cosmetics should be previewed

    @done
    Scenario: View current season details
      When I navigate to the seasons view
      Then I should see the current season name and theme
      And I should see the number of days remaining in the season
      And I should see the seasonal quest line with progress
      And I should see the seasonal leaderboard
      And I should see the seasonal cosmetics I can earn

    @done
    Scenario: View past season history
      Given I have participated in 3 previous seasons
      When I navigate to past seasons
      Then I should see a summary of each past season
      And each summary should show my final rank
      And each summary should show the cosmetics I earned
      And each summary should show the seasonal XP I accumulated

  Rule: Each season has a themed quest line that provides guided challenges

    @done
    Scenario: Start the seasonal quest line
      Given a new season has begun with a quest line of 8 stages
      When I view the seasonal quest line
      Then I should see stage 1 as available
      And stages 2-8 should be locked
      And each stage should preview its challenge theme

    @done
    Scenario Outline: Complete a seasonal quest line stage
      Given I am on stage <stage> of the seasonal quest line
      And stage <stage> requires completing <required> tasks rated "<min_difficulty>" or above
      And I have completed <completed> qualifying tasks
      When I complete another qualifying task
      Then stage <stage> should be marked as complete
      And I should receive seasonal XP
      And stage <next_stage> should become available
      And I should earn the stage <stage> cosmetic reward

      Examples:
        | stage | required | min_difficulty | completed | next_stage |
        | 1     | 3        | Easy           | 2         | 2          |
        | 3     | 5        | Hard           | 4         | 4          |
        | 5     | 7        | Normal         | 6         | 6          |

    @done
    Scenario: Complete the full seasonal quest line
      Given I have completed stages 1 through 7 of the seasonal quest line
      When I complete stage 8
      Then I should receive a seasonal completion bonus
      And I should earn the exclusive season-completion cosmetic
      And a seasonal completion badge should appear on my profile

  @premium
  Rule: Seasonal leaderboards reset each quarter and rank users by seasonal XP

    @done
    Scenario: View seasonal leaderboard
      Given the current season is 6 weeks in
      When I view the seasonal leaderboard
      Then I should see my rank among my cohort
      And I should see my seasonal XP total
      And I should see the top 10 users in my cohort
      And my cohort should consist of users within 5 levels of my current level

    @done
    Scenario: Season ends and final ranks are recorded
      Given the current season is ending
      And my seasonal rank is 15th in my cohort
      When the season concludes
      Then my final rank should be permanently recorded
      And I should receive a rank-based seasonal reward
      And the leaderboard should become read-only for the past season

  Rule: Seasonal cosmetics are limited to the season and cannot be earned later

    @done
    Scenario: Earn a seasonal cosmetic
      Given the current season offers a "Crystal Compass" profile badge
      And the badge requires completing the seasonal quest line stage 5
      When I complete stage 5
      Then the "Crystal Compass" badge should be added to my collection
      And it should be marked as a seasonal exclusive

    @done
    Scenario: Seasonal cosmetic unavailable after season ends
      Given the "Season of the Architect" offered the "Blueprint Frame" avatar border
      And I did not earn it during that season
      When the "Season of the Architect" ends
      Then the "Blueprint Frame" should no longer be earnable
      And it should appear as a locked seasonal item in my collection history

    @done
    Scenario: Display seasonal cosmetic on profile
      Given I have earned the "Crystal Compass" badge
      When I select it as my active profile badge
      Then it should be displayed on my profile
      And other users should see it marked with its season of origin

  Rule: Users who join mid-season or are inactive can still participate meaningfully

    @done
    Scenario: User joins mid-season
      Given the current season is 6 weeks in with 7 weeks remaining
      And I have just created my account
      When I view the seasonal quest line
      Then I should see all stages available from stage 1
      And I should be able to progress through the quest line normally
      And the seasonal leaderboard should include me with 0 seasonal XP

    @done
    Scenario: User inactive for an entire season
      Given I did not log in during the "Season of the Architect"
      When the next season begins
      Then my permanent level and XP should be unchanged
      And I should have no record for the missed season in my season history
      And I should be able to participate fully in the new season

    @done
    Scenario: Seamless transition between seasons
      Given the current season ends today
      When the next season begins
      Then the new season should be immediately available with no downtime
      And any incomplete seasonal quest line stages should be locked
      And the previous season's final leaderboard should be viewable in past seasons

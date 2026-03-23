@progression @levels
Feature: Levelling System
  As a Waypoint user
  I want to level up as I accumulate XP
  So that I have a clear sense of long-term progression

  Background:
    Given I am an authenticated user

  Rule: Levels require logarithmically scaling XP to prevent inflation

    @done
    Scenario: New user starts at level 1
      Given I have just created my account
      Then my level should be 1
      And my XP should be 0
      And the XP required for level 2 should be displayed

    @done
    Scenario: Level up when XP threshold is reached
      Given I am level 3 with 280 XP
      And the XP threshold for level 4 is 300
      When I earn 25 XP from completing a task
      Then my level should change to 4
      And a level-up celebration should be displayed
      And a level-up event should appear on my journey timeline
      And the excess 5 XP should carry over toward level 5

    @done
    Scenario: XP requirements scale logarithmically
      Then the XP thresholds for levels should follow a logarithmic curve:
        | Level | Cumulative XP Required |
        | 2     | 50                     |
        | 5     | 300                    |
        | 10    | 1,000                  |
        | 20    | 4,000                  |
        | 50    | 25,000                 |
      And level 5 should be reachable by completing 10 Normal tasks per day for 5 days
      And level 50 should require at least 30 days of sustained high-difficulty completions

    @done
    Scenario: Level up unlocks new features progressively
      Given I am level 2
      When I reach level 3
      Then I should unlock the "Skill Trees" feature
      And I should receive a tutorial prompt for the new feature
      And the feature should be accessible from that point forward

    @wip
    Scenario Outline: Progressive feature unlocks by level
      When I reach level <level>
      Then I should unlock "<feature>"

      Examples:
        | level | feature                      |
        | 1     | Tasks, Quests, Basic XP      |
        | 3     | Skill Trees                  |
        | 5     | Titles, Daily Brief          |
        | 7     | Accountability Partners      |
        | 10    | Leaderboards, Challenge Mode |
        | 15    | Insight Cards                |
        | 20    | Advanced Analytics           |

  Rule: Level information is visible and motivating

    @done
    Scenario: View level progress on dashboard
      Given I am level 7 with 850 XP
      And the threshold for level 8 is 1,000 XP
      When I view my dashboard
      Then I should see "Level 7" prominently displayed
      And I should see a progress bar showing 85% toward level 8
      And I should see "150 XP to next level"

    @done
    Scenario: Level badge displayed on profile
      Given I am level 12
      When another user views my profile
      Then they should see my level badge showing "Level 12"
      And the badge style should reflect my level tier

    @wip
    Scenario: Level milestones are celebrated
      Given I am about to reach level 10
      When I earn enough XP to reach level 10
      Then I should see an enhanced celebration animation
      And I should receive a milestone achievement
      And the milestone should be shareable

  Rule: There is a maximum level that gracefully handles continued progression

    @done
    Scenario: User reaches maximum level
      Given I am at the maximum level
      When I earn additional XP
      Then my level should remain at the maximum
      And the XP should still be tracked as lifetime XP
      And I should see a "Max Level" badge on my profile
      And I should still earn seasonal XP and rewards

    @wip
    Scenario: Existing users retain levels when XP thresholds are rebalanced
      Given I am level 15 with 3,500 XP
      And the XP thresholds have been rebalanced
      When I view my profile
      Then my level should reflect the new thresholds applied to my existing XP
      And I should never lose levels due to a rebalance

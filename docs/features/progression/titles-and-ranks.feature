@progression @titles
Feature: Titles and Ranks
  As a Waypoint user
  I want to earn titles through sustained behaviour patterns
  So that my productivity identity is recognised and visible to others

  Background:
    Given I am an authenticated user
    And I have reached at least level 5

  Rule: Titles are earned through sustained behaviour, not one-off achievements

    @done
    Scenario Outline: Earn a title through sustained behaviour
      Given I have met the sustained requirement for the title "<title>"
      When I check my title progress
      Then I should be awarded the title "<title>"
      And a title-earned event should appear on my journey timeline
      And the title should be visible on my profile

      Examples:
        | title              | requirement_summary                                           |
        | Early Bird         | Completed 50+ tasks before 9 AM over at least 4 weeks        |
        | Morning Architect  | Completed complex tasks before noon consistently for 6 weeks  |
        | Night Owl          | Completed 50+ tasks after 9 PM over at least 4 weeks         |
        | Marathon Builder   | Daily progress on a single saga for 60+ consecutive days      |
        | Boss Slayer        | Completed 10+ Boss Tasks                                     |
        | Streak Master      | Maintained a 30-day task completion streak                    |
        | Quest Closer       | Completed 25+ quests                                         |
        | Consistent Planner | Completed 12+ weekly reviews                                 |
        | Team Anchor        | Contributed to guild quests every week for 8+ weeks           |

    @done
    Scenario: Title requires sustained behaviour, not bursts
      Given I completed 50 tasks before 9 AM
      But they were all completed within a single week
      When I check my title progress for "Early Bird"
      Then I should not be awarded the title "Early Bird"
      And the system should show progress toward the sustained requirement

    @done
    Scenario: Title progress is visible before earning
      Given I am working toward the "Streak Master" title
      And I need a 30-day streak and I am currently at 18 days
      When I view my title progress
      Then I should see "Streak Master" with a progress indicator of 60%
      And I should see "12 more days of consistent completions needed"

  Rule: Users choose which title to display and titles are publicly visible

    @done
    Scenario: Select an active title
      Given I have earned the titles "Early Bird" and "Boss Slayer"
      When I select "Boss Slayer" as my active title
      Then "Boss Slayer" should appear next to my name on my profile
      And "Boss Slayer" should appear on leaderboards and guild views

    @done
    Scenario: View all earned titles
      Given I have earned 5 titles
      When I navigate to my titles collection
      Then I should see all 5 earned titles with their earn dates
      And I should see locked titles with their requirements
      And I should be able to select any earned title as active

    @done
    Scenario: Title visible on profile to other users
      Given I have "Morning Architect" as my active title
      When another user views my profile
      Then they should see "Morning Architect" displayed under my name

    @done
    Scenario: Title visible in guild member list
      Given I have "Morning Architect" as my active title
      When another user views a guild member list that includes me
      Then they should see my title next to my name

  Rule: Titles are permanently earned and never revoked

    @done
    Scenario: Title retained after behaviour change
      Given I earned the title "Early Bird" through consistent morning completions
      And I have not completed a task before 9 AM in the last 3 weeks
      When I view my titles
      Then "Early Bird" should still be in my earned titles
      And it should remain selectable as my active title

    @done
    Scenario: Active title displayed when user holds many titles
      Given I have earned 8 titles
      And I have selected "Boss Slayer" as my active title
      When another user views my profile
      Then they should see "Boss Slayer" as my displayed title
      And they should see a count indicating I have earned 8 titles total

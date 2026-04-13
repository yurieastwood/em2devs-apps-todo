@reflection @wrapped @premium
Feature: Annual Wrapped
  As a Waypoint user
  I want an annual summary of my productivity journey
  So that I can celebrate my year and share my accomplishments

  Background:
    Given I am an authenticated user
    And I have a premium subscription
    And I have used Waypoint for at least 3 months in the current year

  Rule: An annual summary is generated at year-end with personalised highlights

    @done
    Scenario: Annual wrapped is generated
      Given it is December 15th or later in the current year
      When my annual wrapped is generated
      Then I should see a multi-slide summary including:
        | Slide                    | Content                                      |
        | Total tasks completed    | Count for the year                            |
        | Total XP earned          | Sum of all XP this year                       |
        | Levels gained            | Start level to end level                      |
        | Longest streak           | Maximum consecutive day streak                |
        | Quests completed         | Total quest count                             |
        | Hardest Boss Task        | The highest-difficulty Boss Task completed     |
        | Most productive month    | Month with highest task completion             |
        | Skill tree growth        | Trees unlocked and tiers advanced              |
        | Titles earned            | New titles earned this year                   |
        | Top insight              | Most impactful insight card of the year       |
        | Seasons participated in  | Seasonal ranks and achievements               |

    @done
    Scenario: Wrapped not available with insufficient data
      Given I signed up in November and have only 6 weeks of data
      When the wrapped period arrives
      Then I should see a message that my wrapped will be available next year
      And I should see a teaser of what wrapped will include

    @done
    Scenario: Slides with zero data show encouraging messaging
      Given it is December 15th or later in the current year
      And I have not completed any quests this year
      When my annual wrapped is generated
      Then the "Quests completed" slide should not be hidden
      And it should display an encouraging message such as "No quests yet — your first quest awaits next year!"

    @done
    Scenario: Mid-year signup users receive a partial wrapped
      Given I signed up in June and have at least 3 months of data
      When the wrapped period arrives
      Then I should receive a "Year So Far" wrapped summary
      And it should cover only the months since my signup
      And it should clearly indicate the partial time period

  Rule: The wrapped experience is engaging and shareable

    @done
    Scenario: View wrapped as an interactive slideshow
      When I open my annual wrapped
      Then I should see a slide-by-slide interactive presentation
      And each slide should display the data point prominently with a celebratory visual treatment
      And I should be able to navigate forward and backward through slides

    @done
    Scenario: Share wrapped highlights
      Given I am viewing my annual wrapped
      When I choose to share a slide
      Then I should be able to generate a shareable image of that slide
      And the image should include Waypoint branding
      And I should be able to share it as an image to any platform via the system share sheet

    @done
    Scenario: View past year's wrapped
      Given I have a wrapped summary from last year
      When I navigate to my wrapped history
      Then I should see last year's wrapped available for replay
      And I should be able to compare year-over-year statistics

    @done
    Scenario: User can exclude specific data from shareable wrapped
      Given I am viewing my annual wrapped
      When I choose to share a slide
      Then I should see privacy options to exclude specific data points from the shareable image
      And the generated image should omit any data I chose to exclude
      And the excluded data should still be visible in my private wrapped view

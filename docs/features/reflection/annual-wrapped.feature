@reflection @wrapped @premium
Feature: Annual Wrapped
  As a Waypoint user
  I want an annual summary of my productivity journey
  So that I can celebrate my year and share my accomplishments

  Background:
    Given I am an authenticated user
    And I have a premium subscription
    And I have used Waypoint for at least 3 months in the current year

  # ───────────────────────────────────────────
  # Wrapped Generation
  # ───────────────────────────────────────────

  Rule: An annual summary is generated at year-end with personalised highlights

    Scenario: Annual wrapped is generated
      Given it is December and the year is ending
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

    Scenario: Wrapped not available with insufficient data
      Given I signed up in November and have only 6 weeks of data
      When the wrapped period arrives
      Then I should see a message that my wrapped will be available next year
      And I should see a teaser of what wrapped will include

  # ───────────────────────────────────────────
  # Wrapped Interaction
  # ───────────────────────────────────────────

  Rule: The wrapped experience is engaging and shareable

    Scenario: View wrapped as an interactive slideshow
      When I open my annual wrapped
      Then I should see a slide-by-slide interactive presentation
      And each slide should have an engaging visual and animation
      And I should be able to navigate forward and backward through slides

    Scenario: Share wrapped highlights
      Given I am viewing my annual wrapped
      When I choose to share a slide
      Then I should be able to generate a shareable image of that slide
      And the image should include Waypoint branding
      And I should be able to share it to external platforms

    Scenario: View past year's wrapped
      Given I have a wrapped summary from last year
      When I navigate to my wrapped history
      Then I should see last year's wrapped available for replay
      And I should be able to compare year-over-year statistics

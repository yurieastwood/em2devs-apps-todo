@reflection @insights @premium
Feature: Insight Cards
  As a Waypoint user
  I want to receive personalised productivity observations
  So that I discover patterns about myself I would not have noticed on my own

  Background:
    Given I am an authenticated user
    And I have a premium subscription
    And I have reached at least level 15
    And I have at least 30 days of task completion history

  # ───────────────────────────────────────────
  # Insight Generation
  # ───────────────────────────────────────────

  Rule: Insights are generated from behavioural data and delivered as discoverable cards

    Scenario Outline: System generates an insight card
      Given the system has detected the pattern "<pattern>"
      When an insight card is generated
      Then I should see a card with the message "<message>"
      And the card should include supporting data or a mini-chart

      Examples:
        | pattern                                          | message                                                                          |
        | High creative task completion on Tuesdays        | You are 3x more likely to complete creative tasks on Tuesday mornings.            |
        | Quest completion time improving                  | Your average quest completion time has improved by 22% this season.               |
        | Consistent weekly reviews                        | You have completed every weekly review for 8 weeks. That puts you in the top 5%.  |
        | Morning productivity peak                        | Your most productive hours are 9 AM to 11 AM. You complete 40% of daily tasks then.|
        | Estimation accuracy improving                    | Your time estimates are now within 15% of actual. That is up from 40% last month.  |
        | Side project consistency                         | You have worked on your side project 5 out of 7 days for 3 weeks straight.        |

    Scenario: Insight cards are delivered periodically
      Given I meet the criteria for multiple insights
      When insights are generated
      Then I should receive no more than 2-3 insight cards per week
      And the cards should be spaced out across different days
      And the most impactful insights should be prioritised

    Scenario: No insight card when insufficient data
      Given I have only 7 days of task history
      When the system evaluates potential insights
      Then no insight cards should be generated
      And I should not see the insights section until enough data is available

  # ───────────────────────────────────────────
  # Insight Interaction
  # ───────────────────────────────────────────

  Rule: Users can view, dismiss, and save insight cards

    Scenario: View an insight card
      Given I have an unread insight card
      When I open the insights section
      Then I should see the insight card with its message and data
      And I should be able to mark it as read

    Scenario: Save an insight card
      Given I have an insight card about my morning productivity
      When I save the card to my collection
      Then it should appear in my saved insights
      And I should be able to reference it later

    Scenario: Dismiss an insight card
      Given I have an insight card I find irrelevant
      When I dismiss the card
      Then the card should be removed from my active insights
      And the system should factor my dismissal into future insight relevance

    Scenario: Insight cards appear in weekly review
      Given I have received 2 insight cards this week
      When I complete my weekly review
      Then the review should include a section highlighting this week's insights

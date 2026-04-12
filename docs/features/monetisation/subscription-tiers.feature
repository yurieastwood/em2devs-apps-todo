@monetisation @tiers
Feature: Subscription Tiers
  As a Waypoint user
  I want clear free and premium tiers
  So that I can use the app effectively for free and upgrade when I need more

  Background:
    Given I am an authenticated user

  Rule: Free-tier users have full access to core task management and gamification

    @todo
    Scenario: Free-tier user has access to core features
      Given I have a free-tier account
      Then I should have access to the following features:
        | Feature                          |
        | Unlimited tasks                  |
        | Unlimited quests                 |
        | Unlimited epics                  |
        | Full XP and levelling engine     |
        | Skill trees                      |
        | Titles and ranks                 |
        | Basic daily brief                |
        | Energy-aware scheduling          |
        | One accountability partner       |
        | Basic weekly review              |
        | Journey timeline                 |
        | Local data storage               |
        | Manual data export               |

    @todo
    Scenario: Free-tier user encounters a premium feature
      Given I have a free-tier account
      When I attempt to access a premium feature such as "Sagas"
      Then I should see a tasteful upgrade prompt explaining the feature
      And I should be able to dismiss the prompt
      And the prompt should not interfere with my current workflow
      And the same feature prompt should not appear more than once per week

  Rule: Premium unlocks advanced intelligence, social, and customisation features

    @todo
    Scenario: Subscribe to premium
      Given I have a free-tier account
      When I navigate to the subscription page
      And I choose the "Waypoint Pro" plan
      And I complete the payment process
      Then my account should be upgraded to premium
      And all premium features should become immediately available

    @todo
    Scenario: Premium user has access to all premium features
      Given I have a premium subscription
      Then I should have access to the following additional features:
        | Feature                           |
        | Sagas and long-arc goal tracking  |
        | Capacity modelling                |
        | Time estimation learning          |
        | Insight cards                     |
        | Guilds (create and join up to 5)  |
        | Challenge mode                    |
        | Seasonal leaderboards             |
        | Cross-device sync                 |
        | Priority themes and cosmetics     |
        | Advanced weekly review            |
        | Annual Wrapped                    |
        | Calendar integration              |

    @todo
    Scenario: Premium subscription expires
      Given I have a premium subscription that has expired
      Then my account should revert to free-tier access
      And I should retain all data created during the premium period
      And premium-only data should be read-only but not deleted
      And sagas should be viewable but not editable
      And guild memberships should be preserved but limited to view-only
      And I should be prompted to renew with a clear explanation of what was lost

    @todo
    Scenario: In-progress guild activities on premium expiry
      Given I have a premium subscription
      And I am participating in a guild challenge
      And I have in-progress shared quests
      When my premium subscription expires
      Then my challenge participation should end gracefully
      And my contributions to shared quests should remain
      But I should not be able to create new guild activities
      And I should be able to view but not interact with guild boards

    @todo
    Scenario: Cosmetics retained after downgrade
      Given I have a premium subscription
      And I have purchased the "Midnight Theme" colour palette
      When my premium subscription expires
      Then I should retain the "Midnight Theme" in my collection
      And I should still be able to use purchased cosmetics
      And no purchased cosmetic should be removed or locked

  Rule: Team tier provides everything in Pro plus team management features

    @todo
    Scenario: Subscribe to team tier
      Given I am the administrator of a team workspace
      When I subscribe to the "Waypoint Guild" plan for up to 25 members
      Then I should be able to invite team members to the team workspace
      And all team members should receive premium features
      And team-specific features should be available

    @todo
    Scenario: Team tier includes team-specific features
      Given my team has the "Waypoint Guild" subscription
      Then the team should have access to:
        | Feature                              |
        | Everything in Pro                    |
        | Shared quest boards with roles       |
        | Team analytics and velocity tracking |
        | Admin controls and onboarding flows  |
        | Dedicated team leaderboards          |

    @todo
    Scenario: Team lead cancels the subscription
      Given my team has the "Waypoint Guild" subscription with 10 members
      When the team subscription is cancelled
      Then all team members should revert to free-tier access
      And team members should retain all data created during the team subscription
      And team-specific features should become read-only
      And each member should be notified of the change

    @todo
    Scenario: Team member is removed from the team
      Given my team has the "Waypoint Guild" subscription
      And "Jordan" is a team member
      When I remove "Jordan" from the team
      Then "Jordan" should revert to free-tier access
      And "Jordan" should retain a copy of their personal data
      And "Jordan" should lose access to shared team quest boards

    @todo
    Scenario: Downgrade from Team to Pro
      Given my team has the "Waypoint Guild" subscription
      When I downgrade to the "Waypoint Pro" plan
      Then my personal account should become a Pro account
      And all team members should revert to free-tier access
      And team-specific features should become read-only
      And all members should be notified of the downgrade

  Rule: Cosmetics are purchasable and provide no productivity advantage

    @todo
    Scenario: Purchase a cosmetic item
      Given I am viewing the cosmetics shop
      When I purchase the "Midnight Theme" colour palette
      Then the theme should be added to my collection
      And I should be able to apply it in my settings
      And the purchase should provide no XP or gameplay advantage

    @todo
    Scenario: Cosmetics do not affect gameplay
      Given two users with identical task completion patterns
      And one user has purchased premium cosmetics
      When XP is calculated for both users
      Then both users should receive identical XP amounts
      And no cosmetic purchase should modify XP rates or difficulty weights

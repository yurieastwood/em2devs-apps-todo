@monetisation @tiers
Feature: Subscription Tiers
  As a Waypoint user
  I want clear free and premium tiers
  So that I can use the app effectively for free and upgrade when I need more

  Background:
    Given I am an authenticated user

  # ───────────────────────────────────────────
  # Free Tier (Waypoint Core)
  # ───────────────────────────────────────────

  Rule: Free-tier users have full access to core task management and gamification

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

    Scenario: Free-tier user encounters a premium feature
      Given I have a free-tier account
      When I attempt to access a premium feature such as "Sagas"
      Then I should see a tasteful upgrade prompt explaining the feature
      And I should be able to dismiss the prompt
      And the prompt should not interfere with my current workflow
      And the same feature prompt should not appear more than once per week

  # ───────────────────────────────────────────
  # Premium Tier (Waypoint Pro)
  # ───────────────────────────────────────────

  Rule: Premium unlocks advanced intelligence, social, and customisation features

    Scenario: Subscribe to premium
      Given I have a free-tier account
      When I navigate to the subscription page
      And I choose the "Waypoint Pro" plan
      And I complete the payment process
      Then my account should be upgraded to premium
      And all premium features should become immediately available

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

    Scenario: Premium subscription expires
      Given I have a premium subscription that has expired
      Then my account should revert to free-tier access
      And I should retain all data created during the premium period
      And premium-only data should be read-only but not deleted
      And sagas should be viewable but not editable
      And guild memberships should be preserved but limited to view-only
      And I should be prompted to renew with a clear explanation of what was lost

  # ───────────────────────────────────────────
  # Team Tier (Waypoint Guild)
  # ───────────────────────────────────────────

  Rule: Team tier provides everything in Pro plus team management features

    Scenario: Subscribe to team tier
      Given I am a team lead looking for a shared productivity tool
      When I subscribe to the "Waypoint Guild" plan for up to 25 members
      Then I should be able to invite team members to the team workspace
      And all team members should receive premium features
      And team-specific features should be available

    Scenario: Team tier includes team-specific features
      Given my team has the "Waypoint Guild" subscription
      Then the team should have access to:
        | Feature                              |
        | Everything in Pro                    |
        | Shared quest boards with roles       |
        | Team analytics and velocity tracking |
        | Admin controls and onboarding flows  |
        | Dedicated team leaderboards          |

  # ───────────────────────────────────────────
  # Cosmetic Purchases
  # ───────────────────────────────────────────

  Rule: Cosmetics are purchasable and provide no productivity advantage

    Scenario: Purchase a cosmetic item
      Given I am viewing the cosmetics shop
      When I purchase the "Midnight Theme" colour palette
      Then the theme should be added to my collection
      And I should be able to apply it in my settings
      And the purchase should provide no XP or gameplay advantage

    Scenario: Cosmetics do not affect gameplay
      Given two users with identical task completion patterns
      And one user has purchased premium cosmetics
      When XP is calculated for both users
      Then both users should receive identical XP amounts
      And no cosmetic purchase should modify XP rates or difficulty weights

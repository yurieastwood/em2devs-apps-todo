@progression @skill-trees
Feature: Skill Trees
  As a Waypoint user
  I want to unlock and progress through skill trees based on my behaviour patterns
  So that my productivity identity is reflected and rewarded

  Background:
    Given I am an authenticated user
    And I have reached at least level 3

  Rule: Skill trees are discovered and unlocked through natural behaviour patterns

    @done
    Scenario Outline: Skill tree unlocked by behaviour pattern
      Given I have consistently completed tasks tagged or categorised as "<category>"
      And I have completed at least <threshold> such tasks
      When the system evaluates my behaviour patterns
      Then the "<tree_name>" skill tree should be unlocked
      And I should receive a notification about the discovery
      And the skill tree should appear in my progression view

      Examples:
        | category         | threshold | tree_name       |
        | creative         | 15        | Creator         |
        | health           | 15        | Guardian        |
        | fitness          | 15        | Guardian        |
        | learning         | 15        | Scholar         |
        | study            | 15        | Scholar         |
        | work             | 20        | Architect       |
        | career           | 20        | Architect       |
        | social           | 15        | Connector       |
        | people           | 15        | Connector       |
        | home             | 15        | Steward         |
        | organising       | 15        | Steward         |
        | side-project     | 10        | Builder         |

    @wip
    Scenario: View available and locked skill trees
      When I navigate to the skill tree view
      Then I should see my unlocked skill trees with progress
      And I should see locked skill trees as silhouettes
      And each locked tree should show a hint about how to unlock it

  Rule: Skill trees are hidden until the user reaches the required level

    @done
    Scenario: User does not see skill trees before level 3
      Given I am an authenticated user
      And I am at level 2
      When I navigate to the progression view
      Then I should not see the skill trees section
      And I should see a teaser message about unlocking skill trees at level 3

  Rule: Each skill tree has multiple tiers that unlock through sustained behaviour

    @done
    Scenario: Progress within a skill tree
      Given I have unlocked the "Builder" skill tree at tier 1
      And the "Builder" tree requires 30 side-project tasks for tier 2
      And I have completed 25 side-project tasks since unlocking
      When I complete 5 more side-project tasks
      Then my "Builder" skill tree should advance to tier 2
      And I should receive a tier-up bonus XP award
      And I should unlock the tier 2 perks

    @done
    Scenario: View skill tree details
      Given I have the "Scholar" skill tree at tier 2
      When I view the "Scholar" skill tree details
      Then I should see my current tier and progress toward tier 3
      And I should see the perks unlocked at each tier
      And I should see personalised study tips based on my patterns
      And I should see a history of qualifying task completions

    @wip
    Scenario: Multiple skill trees can be active simultaneously
      Given I have unlocked the "Creator" and "Builder" skill trees
      When I complete a task tagged "creative" and "side-project"
      Then progress should be applied to both the "Creator" and "Builder" trees
      And the XP earned should only count once for my total

  Rule: Skill tree tiers unlock personalised tips, workflows, and cosmetics

    @wip
    Scenario: Tier 1 perk unlocks personalised tips
      Given I have just unlocked the "Guardian" skill tree at tier 1
      Then I should receive a set of health and fitness productivity tips
      And the tips should be accessible from my skill tree view

    @wip
    Scenario: Tier 2 perk unlocks suggested workflows
      Given I have reached tier 2 of the "Architect" skill tree
      Then I should receive suggested quest templates for work projects
      And the templates should be usable when creating new quests

    @wip
    Scenario: Tier 3 perk unlocks cosmetic rewards
      Given I have reached tier 3 of the "Creator" skill tree
      Then I should unlock a unique profile badge for the "Creator" tree
      And I should unlock a themed colour palette for my interface
      And these cosmetics should be selectable in my profile settings

  Rule: Skill tree progress is permanent and does not decay on inactivity

    @done
    Scenario: Skill tree progress retained after inactivity
      Given I have the "Builder" skill tree at tier 2
      And I have not completed any side-project tasks in 60 days
      When I view the "Builder" skill tree details
      Then my tier should still be 2
      And my progress toward tier 3 should be unchanged
      And I should see a gentle prompt encouraging me to pick up side-project tasks again

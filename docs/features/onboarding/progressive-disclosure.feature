@onboarding @progressive-disclosure
Feature: Progressive Disclosure and Onboarding
  As a new Waypoint user
  I want a clean, simple experience that reveals depth as I engage
  So that I am not overwhelmed on day one but discover richness over time

  Rule: Account creation is minimal and gets users to their first task immediately

    @todo
    Scenario: Create an account with minimal friction
      Given I am on the Waypoint signup page
      When I sign up using a social login provider
      Then my account should be created
      And I should be taken directly to a "Create your first task" prompt
      And I should not see any gamification elements yet
      And the interface should look like a clean, simple TODO app

    @todo
    Scenario: Create first task during onboarding
      Given I have just created my account
      When I am prompted to create my first task
      And I enter "Buy groceries"
      Then the task should be created
      And I should see a brief welcome message
      And I should be taken to my task inbox

    @todo
    Scenario: Skip the first-task prompt
      Given I have just created my account
      When I dismiss the first-task prompt
      Then I should be taken to an empty task inbox
      And I should see a helpful empty state with guidance

  Rule: Features are revealed gradually as users demonstrate engagement

    Background:
      Given I am an authenticated user

    @todo
    Scenario: Day 1 experience is a clean TODO app
      Given I signed up today
      When I use Waypoint on day 1
      Then I should see only: task inbox, today view, and upcoming view
      And I should not see: XP, levels, skill trees, quests, guilds, or leaderboards
      And the interface should feel familiar and unintimidating

    @todo
    Scenario: Quest creation unlocked after creating multiple tasks
      Given I have created 5 or more tasks
      When the system evaluates my engagement
      Then I should see a gentle prompt introducing quests
      And the quest creation feature should become available
      And a brief tooltip should explain how quests group related tasks

    @todo
    Scenario: XP becomes visible after completing several tasks
      Given I have completed 10 or more tasks
      When the system evaluates my engagement
      Then XP indicators should begin appearing on task completions
      And a brief explanation should introduce the XP concept
      And my retroactive XP from previous completions should be displayed

    @todo
    Scenario Outline: Features unlock at specific engagement thresholds
      Given I have reached <threshold>
      When the system evaluates my engagement
      Then the "<feature>" feature should be revealed to me
      And a brief contextual explanation should introduce the feature

      Examples:
        | threshold               | feature                |
        | 5 tasks created         | Quests                 |
        | 10 tasks completed      | XP and Levels          |
        | Level 3                 | Skill Trees            |
        | Level 5                 | Titles and Ranks       |
        | 3 quests completed      | Epics                  |
        | Level 7                 | Accountability Partner |

    @todo
    Scenario: Features are never removed once revealed
      Given the quest feature has been revealed to me
      Then the quest feature should remain permanently available
      And I should never lose access to a feature that was previously unlocked

    @todo
    Scenario: User can manually explore features early
      Given I am a new user on day 2
      When I navigate to a "Discover features" section
      Then I should see a preview of upcoming features
      And I should see what engagement level unlocks each feature
      And I should not be able to force-unlock features prematurely

    @todo
    Scenario: Premium users see all features immediately
      Given I have a premium subscription
      When I complete onboarding
      Then all features should be immediately available
      And I should still see contextual tutorials as I use each feature for the first time

  Rule: When the gamification layer activates, users see progress they already earned

    Background:
      Given I am an authenticated user who has been using Waypoint during Phase 1

    @todo
    Scenario: Retroactive XP reveal
      Given I have completed 45 tasks over 3 weeks
      And the system has been silently tracking XP
      When the gamification layer is activated for my account
      Then I should see a special "Your Journey So Far" reveal screen
      And I should see all retroactive XP calculated from my 45 completions
      And I should see the level I have already reached
      And this should feel like discovering hidden progress, not starting from scratch

    @todo
    Scenario: Retroactive skill tree unlocks
      Given I have completed 20 tasks tagged "creative" during the pre-gamification phase
      When the gamification layer is activated
      Then the "Creator" skill tree should be immediately unlocked
      And progress toward tier 2 should reflect my historical completions

    @todo
    Scenario: Retroactive title eligibility
      Given I completed tasks before 9 AM consistently during the pre-gamification phase
      When the gamification layer is activated
      Then I should receive any titles I qualify for based on historical data
      And the titles should appear in the retroactive reveal experience

  Rule: Tutorials appear at the moment of relevance, not as a front-loaded tour

    Background:
      Given I am an authenticated user
      And features have been progressively unlocked for me

    @todo
    Scenario: Quest tutorial on first quest creation
      Given quests have been revealed to me
      When I create my first quest
      Then I should see a brief contextual tooltip explaining quest mechanics
      And the tooltip should be dismissible
      And it should not appear again for quest creation

    @todo
    Scenario: Boss Task tutorial on first Boss Task encounter
      Given I have my first task flagged as a Boss Task
      When I view the Boss Task for the first time
      Then I should see a brief explanation of Boss Tasks
      And the explanation should describe the intervention options and bonus XP

    @todo
    Scenario: No tutorial bombardment
      Given multiple features are newly unlocked
      When I log in
      Then I should see at most 1 tutorial prompt per session
      And additional tutorials should be queued for future sessions

    @todo
    Scenario: Session is defined by a login or app launch
      When I launch the app or log in
      Then this should count as a new session for tutorial purposes
      And returning from background should not count as a new session

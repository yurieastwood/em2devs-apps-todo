@social @guilds @premium
Feature: Guilds
  As a Waypoint user
  I want to form or join small groups with shared quest boards
  So that my team and I can maintain shared accountability and momentum

  Background:
    Given I am an authenticated user
    And I have a premium subscription

  Rule: Users can create and manage guilds of 2-12 members

    @todo
    Scenario: Create a guild
      When I create a guild with the following details:
        | Field       | Value                        |
        | Name        | Side Project Squad           |
        | Description | Accountability for builders  |
        | Type        | Private                      |
      Then the guild "Side Project Squad" should be created
      And I should be the guild leader
      And the guild should have 1 member (me)

    @todo
    Scenario: Generate an invite link for a guild
      Given I am the leader of "Side Project Squad"
      When I generate an invite link for the guild
      Then a shareable invite link should be created
      And the link should expire after 7 days by default

    @todo
    Scenario: Accept a guild invite via link
      Given a valid invite link exists for "Side Project Squad"
      When another user clicks the invite link and accepts
      Then they should be added to "Side Project Squad"
      And the guild should have 2 members

    @todo
    Scenario: Guild reaches maximum capacity
      Given my guild "Side Project Squad" has 12 members
      When a 13th user attempts to join via invite link
      Then they should see a message that the guild is at capacity
      And they should not be added to the guild

    @todo
    Scenario: Remove a member from a guild
      Given I am the leader of "Side Project Squad" with 5 members
      When I remove the member "Alex" from the guild
      Then "Alex" should no longer be a guild member
      And "Alex" should receive a notification about the removal
      And their contributions to guild quests should remain in history

    @todo
    Scenario: Leave a guild
      Given I am a member of "Study Group Alpha"
      And I am not the guild leader
      When I choose to leave the guild
      Then I should no longer be a member
      And my past contributions should remain visible in guild history

    @todo
    Scenario: Leader leaves the guild
      Given I am the leader of "Side Project Squad" with 3 members
      When I choose to leave the guild
      Then I should be prompted to transfer leadership
      When I transfer leadership to "Jordan"
      Then "Jordan" should become the new guild leader
      And I should be removed from the guild

    @todo
    Scenario: Disband a guild
      Given I am the leader of "Side Project Squad"
      When I choose to disband the guild
      And I confirm the disbandment
      Then all members should be notified
      And the guild should be archived
      And individual contributions should remain in each member's history

    @todo
    Scenario: User can only lead a limited number of guilds
      Given I am the leader of 3 guilds
      When I attempt to create a new guild
      Then I should see a message that I have reached the maximum number of guilds I can lead
      And I should be offered the option to disband or transfer leadership of an existing guild

    @todo
    Scenario: Edit guild details
      Given I am the leader of "Side Project Squad"
      When I update the guild details:
        | Field       | Value                        |
        | Name        | Side Project Champions       |
        | Description | Shipping greatness together  |
      Then the guild details should be updated
      And all members should be notified of the changes

    @todo
    Scenario: Remove a member with in-progress guild quest tasks
      Given I am the leader of "Side Project Squad"
      And "Alex" has 3 in-progress tasks on the guild quest "Ship landing page"
      When I remove "Alex" from the guild
      Then "Alex" should no longer be a guild member
      And their in-progress tasks should become unassigned on the guild quest board
      And the remaining members should be notified of the unassigned tasks

    @todo
    Scenario: Leader transfer is declined
      Given I am the leader of "Side Project Squad" with 3 members
      When I choose to leave the guild
      And I attempt to transfer leadership to "Jordan"
      And "Jordan" declines the transfer
      Then I should be prompted to select another member for leadership
      And I should remain the guild leader until the transfer is accepted

    @todo
    Scenario: Last non-leader member leaves the guild
      Given I am the leader of "Side Project Squad" with 2 members
      And the only other member is "Jordan"
      When "Jordan" leaves the guild
      Then I should be the sole remaining member
      And the guild should remain active
      And I should be prompted to invite new members or disband

  Rule: Guilds have shared quest boards where members collaborate

    @todo
    Scenario: Create a guild quest
      Given I am a member of "Side Project Squad"
      When I create a guild quest with the following details:
        | Field       | Value                           |
        | Title       | Ship landing page               |
        | Description | Get the marketing site live     |
        | Due Date    | 2026-05-01                      |
      And I add tasks and assign them to guild members:
        | Task               | Assignee |
        | Write copy         | Me       |
        | Design mockups     | Jordan   |
        | Implement HTML/CSS | Alex     |
        | Deploy to hosting  | Me       |
      Then the guild quest should appear on the shared quest board
      And each member should see their assigned tasks

    @todo
    Scenario: View guild quest board
      Given my guild has 3 active quests
      When I view the guild quest board
      Then I should see all 3 quests with their progress
      And I should see which tasks are assigned to which members
      And I should see the overall guild activity feed

    @todo
    Scenario: Complete an assigned guild task
      Given I have a guild task "Write copy" assigned to me
      When I complete the task
      Then the task should be marked as complete on the guild quest board
      And I should receive XP (both personal and guild contribution)
      And guild members should see the completion in the guild feed

    @todo
    Scenario: Guild quest completion
      Given all tasks in the guild quest "Ship landing page" are complete
      When the final task is completed
      Then the guild quest should be marked as complete
      And all contributing members should receive a guild quest bonus
      And the completion should appear in the guild feed with a celebration

  Rule: Guilds have collective XP and shared milestones

    @todo
    Scenario: View guild XP and level
      When I view my guild's profile
      Then I should see the guild's collective XP total
      And I should see the guild level
      And I should see each member's contribution to guild XP

    @todo
    Scenario: Guild levels up
      Given my guild has accumulated enough collective XP
      When the guild XP threshold for the next level is reached
      Then all guild members should receive a guild level-up notification
      And the guild should unlock the next tier of guild perks

    @todo
    Scenario: View guild activity feed
      When I view the guild feed
      Then I should see recent task completions by guild members
      And I should see quest completions and milestones
      And I should see members' level-ups and title achievements
      And I should be able to react to feed items with encouragement

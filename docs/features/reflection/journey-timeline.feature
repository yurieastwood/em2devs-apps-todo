@reflection @timeline
Feature: Journey Timeline
  As a Waypoint user
  I want a visual timeline of my accomplishments and milestones
  So that I can look back on my progress and feel motivated by how far I have come

  Background:
    Given I am an authenticated user

  # ───────────────────────────────────────────
  # Timeline Events
  # ───────────────────────────────────────────

  Rule: Significant events are automatically added to the timeline

    Scenario Outline: Event types appear on the timeline
      Given I have triggered a "<event_type>" event
      Then a "<event_type>" entry should appear on my journey timeline
      And it should include the date and relevant details

      Examples:
        | event_type                    |
        | Level up                      |
        | Quest completed               |
        | Epic completed                |
        | Saga completed                |
        | Boss Task defeated            |
        | Title earned                  |
        | Skill tree unlocked           |
        | Skill tree tier advanced      |
        | Streak milestone (7, 30, 100) |
        | Seasonal quest line completed |
        | Guild joined                  |
        | Guild quest completed         |
        | Challenge won                 |
        | Weekly review streak milestone|

    Scenario: Timeline displays events chronologically
      Given I have 20 events on my journey timeline
      When I view the timeline
      Then events should be displayed in reverse chronological order
      And each event should show its date and type
      And I should be able to scroll through my full history

    Scenario: Timeline groups events by month
      Given I have events spanning 6 months
      When I view the timeline
      Then events should be grouped by month
      And each month should show a summary count of events

  # ───────────────────────────────────────────
  # Timeline Interaction
  # ───────────────────────────────────────────

  Rule: Users can browse, filter, and annotate their timeline

    Scenario: Filter timeline by event type
      Given I have a mix of level-up, quest, and title events
      When I filter the timeline by "Quest completed"
      Then I should only see quest completion events

    Scenario: Add a personal note to a timeline event
      Given I have a "Quest completed" event for "Prepare conference talk"
      When I add a note "First ever conference talk - terrifying but worth it!"
      Then the note should be saved with the timeline event
      And the note should be visible when I view the event

    Scenario: View timeline event details
      Given I have a "Level up" event for reaching level 10
      When I tap on the event
      Then I should see the date and time of the level up
      And I should see the XP that triggered it
      And I should see what features were unlocked at that level

  # ───────────────────────────────────────────
  # Timeline as Retention Mechanic
  # ───────────────────────────────────────────

  Rule: The timeline reinforces the user's sense of accumulated progress

    Scenario: New user has an empty timeline with encouragement
      Given I am a new user with no timeline events
      When I view the timeline
      Then I should see an encouraging message about building my journey
      And I should see what kinds of events will appear

    Scenario: Long-term user scrolling back through months of progress
      Given I have been using Waypoint for 8 months
      And I have 50+ timeline events
      When I scroll through my timeline
      Then I should be able to see the full arc of my productivity journey
      And visual density should increase as I became more active

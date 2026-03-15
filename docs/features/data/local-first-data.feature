@data @local-first
Feature: Local-First Data and Export
  As a Waypoint user
  I want my data stored locally by default with full export capabilities
  So that I own my data and am never locked into the platform

  Background:
    Given I am an authenticated user

  # ───────────────────────────────────────────
  # Local Storage
  # ───────────────────────────────────────────

  Rule: All data is stored locally by default and the app works offline

    Scenario: App works without internet connection
      Given I have no internet connection
      When I open Waypoint
      Then I should be able to view all my tasks, quests, and progression
      And I should be able to create new tasks
      And I should be able to complete tasks
      And I should be able to earn XP
      And all changes should be saved locally

    Scenario: Data persists across app restarts
      Given I have created 10 tasks and completed 5
      When I close and reopen the app
      Then all 10 tasks should be present
      And 5 should show as completed
      And my XP and level should be correct

    Scenario: No data sent to servers without explicit opt-in
      Given I have a free-tier account without sync enabled
      When I use the app for a full week
      Then no task data should be transmitted to external servers
      And all analytics should be computed on-device
      And the only network calls should be for authentication and subscription validation

  # ───────────────────────────────────────────
  # Cross-Device Sync
  # ───────────────────────────────────────────

  @premium
  Rule: Premium users can opt into cross-device sync

    Scenario: Enable cross-device sync
      Given I have a premium subscription
      When I enable cross-device sync in settings
      Then I should see a clear explanation of what data will be synced
      And I should confirm my consent
      When I confirm
      Then my data should begin syncing to the cloud
      And changes should propagate to my other devices

    Scenario: Sync conflict resolution
      Given I have sync enabled on two devices
      And I complete a task on device A while offline
      And I edit the same task on device B while offline
      When both devices come online
      Then the system should detect the conflict
      And the most recent change should take priority
      And both versions should be available in a conflict log

    Scenario: Disable sync and delete cloud data
      Given I have sync enabled
      When I disable sync
      Then I should be offered the option to delete all cloud-stored data
      When I confirm cloud data deletion
      Then all my data should be removed from the server
      And my local data should remain intact
      And the app should continue working offline

  # ───────────────────────────────────────────
  # Data Export
  # ───────────────────────────────────────────

  Rule: Users can export all their data at any time in open formats

    Scenario: Export all data as JSON
      When I navigate to data export settings
      And I choose to export all data as JSON
      Then a complete JSON file should be generated containing:
        | Data Type          |
        | All tasks          |
        | All quests         |
        | All epics          |
        | All sagas          |
        | XP history         |
        | Level history      |
        | Skill tree progress|
        | Titles earned      |
        | Weekly reviews     |
        | Timeline events    |
        | Insight cards      |
        | Settings           |
      And the file should be downloadable to my device

    Scenario: Export tasks as CSV
      When I choose to export tasks as CSV
      Then a CSV file should be generated with all task data
      And the CSV should include all fields: title, description, status, dates, tags, XP, difficulty, quest assignment
      And the file should be compatible with spreadsheet applications

    Scenario: Export is always available regardless of subscription
      Given I have a free-tier account
      When I navigate to data export
      Then the full JSON and CSV export options should be available
      And no export functionality should be restricted by tier

    Scenario: Scheduled automatic export
      Given I have a premium subscription
      When I configure a weekly automatic export
      Then a JSON backup should be generated every week
      And it should be stored in my designated local directory
      And the 4 most recent backups should be retained

  # ───────────────────────────────────────────
  # Data Deletion
  # ───────────────────────────────────────────

  Rule: Users can delete their data and account permanently

    Scenario: Delete all data
      When I choose to delete all my Waypoint data
      Then I should see a warning about permanent data loss
      And I should be required to type a confirmation phrase
      When I confirm
      Then all local data should be permanently deleted
      And all cloud data (if sync was enabled) should be permanently deleted
      And my account should remain active but empty

    Scenario: Delete account entirely
      When I choose to delete my account
      Then I should see a warning about permanent account and data loss
      And I should be offered a final export before deletion
      When I confirm account deletion
      Then all data should be permanently deleted
      And my account should be deactivated
      And my username should be released after a 30-day holding period

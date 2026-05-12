@data @local-first
Feature: Local-First Data and Export
  As a Waypoint user
  I want my data stored locally by default with full export capabilities
  So that I own my data and am never locked into the platform

  Background:
    Given I am an authenticated user

  Rule: All data is stored locally by default and the app works offline

    @todo
    Scenario: App works without internet connection
      Given I have no internet connection
      When I open Waypoint
      Then I should be able to view all my tasks, quests, and progression
      And I should be able to create new tasks
      And I should be able to complete tasks
      And I should be able to earn XP
      And all changes should be saved locally

    @todo
    Scenario: Data persists across app restarts
      Given I have created 10 tasks and completed 5
      When I close and reopen the app
      Then all 10 tasks should be present
      And 5 should show as completed
      And my XP and level should be correct

    @todo
    Scenario: No data sent to servers without explicit opt-in
      Given I have a free-tier account without sync enabled
      When I use the app for a full week
      Then no task data should be transmitted to external servers
      And all analytics should be computed on-device
      And the only network calls should be for authentication and subscription validation

  @premium
  Rule: Premium users can opt into cross-device sync

    @todo
    Scenario: Enable cross-device sync
      Given I have a premium subscription
      When I enable cross-device sync in settings
      Then I should see a clear explanation of what data will be synced
      And I should confirm my consent
      When I confirm
      Then my data should begin syncing to the cloud
      And changes should propagate to my other devices

    @todo
    Scenario: Sync conflict resolution
      Given I have sync enabled on two devices
      And I complete a task on device A while offline
      And I edit the same task on device B while offline
      When both devices come online
      Then the system should detect the conflict
      And the change with the most recent server-side timestamp should take priority
      And both versions should be available in a conflict log for manual review

    @todo
    Scenario: Disable sync and delete cloud data
      Given I have sync enabled
      When I disable sync
      Then I should be offered the option to delete all cloud-stored data
      When I confirm cloud data deletion
      Then all my data should be removed from the server
      And my local data should remain intact
      And the app should continue working offline

    @todo
    Scenario: Social features require server-side state
      Given I am a member of a guild with shared quests
      And I have no internet connection
      When I view guild and shared quest data
      Then I should see the last-synced state of guild and shared quest data
      And I should see a notice that social data may be outdated
      And I should not be able to modify guild or shared quest data while offline

  Rule: Users can export all their data at any time in open formats

    @done
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

    @done
    Scenario: Export tasks as CSV
      When I choose to export tasks as CSV
      Then a CSV file should be generated with all task data
      And the CSV should include all fields: title, description, status, dates, tags, XP, difficulty, quest assignment
      And the file should be compatible with spreadsheet applications

    @done
    Scenario: Export is always available regardless of subscription
      Given I have a free-tier account
      When I navigate to data export
      Then the full JSON and CSV export options should be available
      And no export functionality should be restricted by tier

    @done
    Scenario: Import data from a previous export
      When I navigate to data import settings
      And I select a previously exported JSON file
      Then I should see a preview of the data to be imported
      And I should be warned that importing will overwrite existing data
      When I confirm the import
      Then all data from the export file should be restored
      And my XP, level, and progression should reflect the imported state

    @todo
    Scenario: Scheduled automatic export
      Given I have a premium subscription
      When I configure a weekly automatic export
      Then a JSON backup should be generated every week
      And it should be stored in my designated local directory
      And the 4 most recent backups should be retained

    @todo
    Scenario: Scheduled export when local directory is unavailable
      Given I have configured a weekly automatic export
      And the designated local directory is unavailable
      When the scheduled export runs
      Then I should receive a notification that the export failed
      And the reason for the failure should be explained
      And the system should retry at the next scheduled interval

  Rule: Users can delete their data and account permanently

    @done
    Scenario: Delete all data
      When I choose to delete all my Waypoint data
      Then I should see a warning about permanent data loss
      And I should be required to type a confirmation phrase
      When I confirm
      Then all local data should be permanently deleted
      And all cloud data (if sync was enabled) should be permanently deleted
      And my account should remain active but empty

    @done
    Scenario: Delete account entirely
      When I choose to delete my account
      Then I should see a warning about permanent account and data loss
      And I should be offered a final export before deletion
      When I confirm account deletion
      Then all data should be permanently deleted
      And my account should be deactivated
      And my username should be released after a 30-day holding period

    @done
    Scenario: Recover account during the 30-day holding period
      Given I have deleted my account within the last 30 days
      When I sign in using my original social login provider
      Then I should be prompted to recover my account
      When I confirm recovery
      Then my account should be reactivated
      And all data should be restored to its pre-deletion state
      And the 30-day holding period should be cancelled

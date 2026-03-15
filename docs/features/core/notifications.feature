@core @notifications
Feature: Notifications and Reminders
  As a Waypoint user
  I want intelligent notifications that help me stay on track
  So that I am reminded at the right time without being overwhelmed

  Background:
    Given I am an authenticated user

  # ───────────────────────────────────────────
  # Task Reminders
  # ───────────────────────────────────────────

  Rule: Users receive timely reminders for due tasks

    Scenario: Reminder for task due today
      Given I have a task "Submit report" due today
      And I have not completed it
      When it reaches my configured reminder time
      Then I should receive a notification reminding me about "Submit report"

    Scenario: Reminder for upcoming deadline
      Given I have a task "Prepare presentation" due in 2 days
      And I have notifications enabled for upcoming deadlines
      When the 2-day-before reminder triggers
      Then I should receive a notification about the approaching deadline

    Scenario: No reminder for completed tasks
      Given I have a task "Buy milk" due today
      And I have already completed it
      When the reminder time arrives
      Then I should not receive a notification for "Buy milk"

  # ───────────────────────────────────────────
  # Achievement Notifications
  # ───────────────────────────────────────────

  Rule: Achievements and milestones generate celebratory notifications

    Scenario Outline: Notification for achievement
      Given I have triggered the achievement "<achievement>"
      Then I should receive a notification celebrating "<achievement>"
      And the notification should feel rewarding, not intrusive

      Examples:
        | achievement                 |
        | Level up                    |
        | Title earned                |
        | Streak milestone reached    |
        | Skill tree unlocked         |
        | Quest completed             |
        | Boss Task defeated          |
        | Season rank achieved        |

  # ───────────────────────────────────────────
  # Notification Preferences
  # ───────────────────────────────────────────

  Rule: Users have granular control over notification settings

    Scenario: Configure notification categories
      When I navigate to notification settings
      Then I should be able to toggle notifications for each category:
        | Category              | Default |
        | Task reminders        | On      |
        | Achievement alerts    | On      |
        | Daily brief ready     | On      |
        | Weekly review prompt  | On      |
        | Guild activity        | On      |
        | Partner messages      | On      |
        | Insight cards         | On      |
        | Capacity warnings     | On      |
        | Upgrade prompts       | Off     |

    Scenario: Set quiet hours
      When I set quiet hours from 10 PM to 7 AM
      Then no notifications should be delivered during that window
      And queued notifications should be delivered after 7 AM

    Scenario: Disable all notifications
      When I disable all notifications
      Then I should receive no push notifications
      And in-app indicators should still show for unread items

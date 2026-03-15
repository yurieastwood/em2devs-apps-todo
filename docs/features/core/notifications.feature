@core @notifications
Feature: Notifications and Reminders
  As a Waypoint user
  I want intelligent notifications that help me stay on track
  So that I am reminded at the right time without being overwhelmed

  Background:
    Given I am an authenticated user

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

    Scenario: Repeated reminders for overdue tasks
      Given I have a task "Submit report" that is 2 days overdue
      And I have not completed or skipped it
      Then I should receive a daily reminder until the task is completed, skipped, or deleted

  Rule: Achievements and milestones generate celebratory notifications

    Scenario Outline: Notification for achievement
      Given I have triggered the achievement "<achievement>"
      Then I should receive a notification celebrating "<achievement>"
      And the notification should include a positive message and achievement icon
      And the notification should auto-dismiss after 5 seconds

      Examples:
        | achievement                 |
        | Level up                    |
        | Title earned                |
        | Streak milestone reached    |
        | Skill tree unlocked         |
        | Quest completed             |
        | Boss Task defeated          |
        | Season rank achieved        |

  Rule: Notifications are delivered through multiple channels

    Scenario: Receive an in-app notification
      Given I have a task reminder triggered
      When the notification is delivered
      Then I should see an in-app notification badge
      And I should see the notification in my notification centre

    Scenario: Receive a push notification
      Given I have push notifications enabled
      And a task reminder is triggered while I am not in the app
      When the notification is delivered
      Then I should receive a push notification on my device

    Scenario: Tap a notification to navigate to the relevant item
      Given I have received a notification about the task "Submit report"
      When I tap the notification
      Then I should be navigated to the task detail view for "Submit report"

    Scenario: Batch notifications when many arrive simultaneously
      Given 5 achievement notifications are triggered within 10 seconds
      Then the notifications should be grouped into a single summary notification
      And the summary should indicate the number of achievements earned
      And I should be able to expand the summary to see individual achievements

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

    Scenario: Quiet hours respect user timezone
      Given I have set quiet hours from 10 PM to 7 AM
      And my timezone is set to "Europe/London"
      When a notification is triggered at 11 PM London time
      Then the notification should be queued until 7 AM London time

    Scenario: Disable all notifications
      When I disable all notifications
      Then I should receive no push notifications
      And in-app indicators should still show for unread items

@intelligence @estimation @premium
Feature: Time Estimation Learning
  As a Waypoint user
  I want the system to learn my estimation patterns and correct my biases
  So that I can plan my time more accurately over time

  Background:
    Given I am an authenticated user
    And I have a premium subscription

  # ───────────────────────────────────────────
  # Estimation Tracking
  # ───────────────────────────────────────────

  Rule: The system tracks estimated vs actual time for every task

    Scenario: Record estimation variance on task completion
      Given I have a task "Write blog post" with estimated time of 1 hour
      When I complete the task and record actual time as 1 hour 40 minutes
      Then the system should record a variance of +66.7%
      And this data point should feed into my estimation model

    Scenario: Prompt for actual time only when estimate was provided
      Given I have a task "Buy milk" with no time estimate
      When I complete the task
      Then I should not be prompted for actual time spent
      And no estimation data should be recorded

    Scenario: Optional time tracking during task execution
      Given I have a task "Code review" with estimated time of 30 minutes
      When I start a timer for the task
      And I stop the timer after 45 minutes
      Then the actual time should be auto-populated as 45 minutes
      And I should be able to adjust the time before confirming

  # ───────────────────────────────────────────
  # Estimation Bias Detection
  # ───────────────────────────────────────────

  Rule: The system identifies systematic estimation biases by task type

    Scenario: Detect consistent underestimation for a task category
      Given I have completed 10 writing tasks over the last month
      And my average estimation for writing tasks was 1 hour
      And my average actual time for writing tasks was 1 hour 25 minutes
      When the system analyses my estimation patterns
      Then it should detect a +42% underestimation bias for writing tasks
      And this bias should be stored in my estimation model

    Scenario: Detect consistent overestimation for a task category
      Given I have completed 8 code review tasks over the last month
      And my average estimation was 1 hour
      And my average actual time was 35 minutes
      When the system analyses my estimation patterns
      Then it should detect a -42% overestimation bias for code review tasks

    Scenario: No bias detected when estimates are accurate
      Given I have completed 12 meeting prep tasks
      And my average estimation variance is within ±15%
      When the system analyses my estimation patterns
      Then no bias should be flagged for meeting prep tasks

  # ───────────────────────────────────────────
  # Corrected Suggestions
  # ───────────────────────────────────────────

  Rule: The system offers corrected time estimates based on learned biases

    Scenario: Suggest corrected estimate for new task
      Given the system has detected I underestimate writing tasks by 40%
      When I create a new task tagged "writing" with estimated time of 2 hours
      Then the system should suggest a corrected estimate of approximately 2 hours 48 minutes
      And the suggestion should explain "Based on your history, writing tasks typically take 40% longer than estimated"
      And I should be able to accept or dismiss the suggestion

    Scenario: User accepts corrected estimate
      Given the system suggests a corrected estimate of 2 hours 48 minutes
      When I accept the corrected estimate
      Then the task estimated time should be updated to 2 hours 48 minutes

    Scenario: User dismisses corrected estimate
      Given the system suggests a corrected estimate of 2 hours 48 minutes
      When I dismiss the suggestion
      Then the task estimated time should remain at 2 hours
      And the system should respect my choice without repeating the suggestion for this task

  # ───────────────────────────────────────────
  # Estimation Insights
  # ───────────────────────────────────────────

  Rule: Users can view their estimation accuracy trends

    Scenario: View estimation accuracy dashboard
      When I navigate to my estimation insights
      Then I should see my overall estimation accuracy percentage
      And I should see estimation bias broken down by task category
      And I should see a trend line showing accuracy improvement over time

    Scenario: Estimation accuracy improves over time
      Given I have been using corrected estimates for 8 weeks
      When I view my estimation accuracy trend
      Then my recent estimation variance should be lower than my initial variance
      And an insight card should celebrate the improvement

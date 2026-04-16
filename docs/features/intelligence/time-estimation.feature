@intelligence @estimation @premium
Feature: Time Estimation Learning
  As a Waypoint user
  I want the system to learn my estimation patterns and correct my biases
  So that I can plan my time more accurately over time

  Background:
    Given I am an authenticated user
    And I have a premium subscription

  Rule: The system tracks estimated vs actual time for every task

    @done
    Scenario: Record estimation variance on task completion
      Given I have a task "Write blog post" with estimated time of 1 hour
      When I complete the task and record actual time as 1 hour 40 minutes
      Then the system should record a variance of +66.7%
      And this data point should feed into my estimation model

    @done
    Scenario: Prompt for actual time only when estimate was provided
      Given I have a task "Buy milk" with no time estimate
      When I complete the task
      Then I should not be prompted for actual time spent
      And no estimation data should be recorded

    @done
    Scenario: Optional time tracking during task execution
      Given I have a task "Code review" with estimated time of 30 minutes
      When I start a timer for the task
      And I stop the timer after 45 minutes
      Then the actual time should be auto-populated as 45 minutes
      And I should be able to adjust the time before confirming

  Rule: The system identifies systematic estimation biases by task type

    @done
    Scenario: Detect consistent underestimation for a task category
      Given I have completed at least 10 tasks in the "writing" category over the last month
      And my average estimation for writing tasks was 1 hour
      And my average actual time for writing tasks was 1 hour 25 minutes
      When the system analyses my estimation patterns
      Then it should detect a +42% underestimation bias for writing tasks
      And this bias should be stored in my estimation model

    @done
    Scenario: Detect consistent overestimation for a task category
      Given I have completed at least 10 tasks in the "code review" category over the last month
      And my average estimation was 1 hour
      And my average actual time was 35 minutes
      When the system analyses my estimation patterns
      Then it should detect a -42% overestimation bias for code review tasks

    @done
    Scenario: Detect dramatic overestimation
      Given I have a task "Organise inbox" with estimated time of 2 hours
      When I complete the task and record actual time as 30 minutes
      Then the system should record a variance of -75%
      And this data point should feed into my estimation model
      And if this pattern recurs across 10 or more tasks in the same category the system should flag a significant overestimation bias

    @done
    Scenario: No bias detected when estimates are accurate
      Given I have completed 12 meeting prep tasks
      And my average estimation variance is within the configurable accuracy threshold of ±15%
      When the system analyses my estimation patterns
      Then no bias should be flagged for meeting prep tasks

  Rule: The system offers corrected time estimates based on learned biases

    @done
    Scenario: Suggest corrected estimate for new task
      Given the system has detected I underestimate writing tasks by 40%
      When I create a new task tagged "writing" with estimated time of 2 hours
      Then the system should suggest a corrected estimate of approximately 2 hours 48 minutes
      And the suggestion should explain "Based on your history, writing tasks typically take 40% longer than estimated"
      And I should be able to accept or dismiss the suggestion

    @done
    Scenario: User accepts corrected estimate
      Given the system suggests a corrected estimate of 2 hours 48 minutes
      When I accept the corrected estimate
      Then the task estimated time should be updated to 2 hours 48 minutes

    @done
    Scenario: User accepts corrected estimate but completes in original time
      Given the system has detected I underestimate writing tasks by 40%
      And I accepted a corrected estimate of 2 hours 48 minutes for a writing task
      When I complete the task and record actual time as 2 hours
      Then the system should record this as a data point where the original estimate was more accurate
      And the estimation model should reduce the bias correction factor for this category
      And the model should not over-correct based on a single instance

    @done
    Scenario: User dismisses corrected estimate
      Given the system suggests a corrected estimate of 2 hours 48 minutes
      When I dismiss the suggestion
      Then the task estimated time should remain at 2 hours
      And the system should respect my choice without repeating the suggestion for this specific task instance

  Rule: Users can view their estimation accuracy trends

    @done
    Scenario: View estimation accuracy dashboard
      When I navigate to my estimation insights
      Then I should see my overall estimation accuracy percentage
      And I should see estimation bias broken down by task category
      And I should see a trend line showing accuracy improvement over time

    @todo
    Scenario: Estimation accuracy improves over time
      Given I have been using corrected estimates for 8 weeks
      When I view my estimation accuracy trend
      Then my recent estimation variance should be lower than my initial variance
      And an insight card should celebrate the improvement

jsonPWrapper ({
  "Features": [
    {
      "RelativeFolder": "core/boss-tasks.feature",
      "Feature": {
        "Name": "Boss Tasks",
        "Description": "As a Waypoint user\nI want difficult or procrastinated tasks to be surfaced as Boss Tasks\nSo that I am supported in tackling my hardest work with special tools and rewards",
        "FeatureElements": [
          {
            "Examples": [],
            "Name": "Task promoted after repeated rescheduling",
            "Slug": "task-promoted-after-repeated-rescheduling",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task \"Write architecture decision record\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have rescheduled it 3 or more times",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the system evaluates my task list",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task should be flagged as a Boss Task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive a notification about the promotion",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the task should display a distinct Boss Task visual indicator",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Task promoted based on age and priority",
            "Slug": "task-promoted-based-on-age-and-priority",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task \"Refactor authentication module\" with priority \"High\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the task has been open for more than 14 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the task has no completed subtasks or time logged",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the system evaluates my task list",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task should be flagged as a Boss Task",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Task promoted based on high difficulty and avoidance",
            "Slug": "task-promoted-based-on-high-difficulty-and-avoidance",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task \"Prepare annual tax filing\" with difficulty \"Hard\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have viewed the task 5 or more times without completing any part of it",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the system evaluates my task list",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task should be flagged as a Boss Task",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "User manually promotes a task to Boss Task",
            "Slug": "user-manually-promotes-a-task-to-boss-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task \"Have difficult conversation with manager\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I manually flag the task as a Boss Task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task should display the Boss Task indicator",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be offered the Boss Task intervention flow",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Low-priority task is not promoted despite age",
            "Slug": "low-priority-task-is-not-promoted-despite-age",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task \"Reorganise bookshelf\" with priority \"Low\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the task has been open for 30 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the system evaluates my task list",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task should not be flagged as a Boss Task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it should be suggested for deletion or archival instead",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Boss Task is demoted when conditions no longer apply",
            "Slug": "boss-task-is-demoted-when-conditions-no-longer-apply",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a Boss Task \"Refactor authentication module\" promoted due to age and priority",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I change the priority to \"Low\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the system re-evaluates my task list",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task should be demoted from Boss Task status",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the Boss Task visual indicator should be removed",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Offer task breakdown",
            "Slug": "offer-task-breakdown",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a Boss Task \"Write architecture decision record\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I open the Boss Task intervention flow",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should be offered the option to break it into smaller subtasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the system should suggest a breakdown based on similar tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Accept suggested breakdown",
            "Slug": "accept-suggested-breakdown",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a Boss Task \"Prepare annual tax filing\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the system suggests breaking it into 4 subtasks:",
                "TableArgument": {
                  "HeaderRow": [
                    "Subtask"
                  ],
                  "DataRows": [
                    [
                      "Gather all income documents"
                    ],
                    [
                      "Collect deduction receipts"
                    ],
                    [
                      "Fill in tax form sections"
                    ],
                    [
                      "Review and submit"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I accept the suggested breakdown",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "4 subtasks should be created under the Boss Task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each subtask should have its own difficulty rating",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the Boss Task becomes a parent task tracking subtask completion",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Offer re-evaluation of task necessity",
            "Slug": "offer-re-evaluation-of-task-necessity",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a Boss Task \"Redesign landing page\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I open the Boss Task intervention flow",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should be offered the option to re-evaluate whether the task still matters",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I choose to re-evaluate",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see prompts asking about the task's current relevance",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to archive the task without penalty if it is no longer needed",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Offer delegation suggestion",
            "Slug": "offer-delegation-suggestion",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a Boss Task \"Create onboarding documentation\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I am a member of a guild",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I open the Boss Task intervention flow",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should be offered the option to convert it to a shared quest",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to assign it to a guild member",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Trigger focus mode for a Boss Task",
            "Slug": "trigger-focus-mode-for-a-boss-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a Boss Task \"Write Q3 strategy document\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I choose to enter Focus Mode for the Boss Task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "all notifications should be suppressed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my task view should show only this task and its subtasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "a timer should begin tracking my focused time",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should earn a Focus Mode XP bonus upon completion",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Complete a Boss Task",
            "Slug": "complete-a-boss-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a Boss Task \"Write architecture decision record\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I mark the Boss Task as complete",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive Boss Task bonus XP on top of standard task XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "a Boss Task victory event should appear on my journey timeline",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "a celebration animation should be displayed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my \"Boss Slayer\" achievement counter should increment",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Complete a Boss Task within Focus Mode",
            "Slug": "complete-a-boss-task-within-focus-mode",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am in Focus Mode working on the Boss Task \"Write Q3 strategy document\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have been in Focus Mode for 45 minutes",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete the Boss Task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive standard task XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive Boss Task bonus XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive Focus Mode bonus XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the total XP should be displayed in a combined breakdown",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Boss Task completion contributes to title progression",
            "Slug": "boss-task-completion-contributes-to-title-progression",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have completed 9 Boss Tasks total",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete my 10th Boss Task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should earn the title \"Boss Slayer\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the title should be visible on my profile",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Delete a Boss Task",
            "Slug": "delete-a-boss-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a Boss Task \"Obsolete research\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I delete the Boss Task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I confirm the deletion",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task should be removed from my task list",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "no XP should be awarded or deducted",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my \"Boss Slayer\" achievement counter should not change",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Boss Task that is also a recurring task instance",
            "Slug": "boss-task-that-is-also-a-recurring-task-instance",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a recurring task \"Weekly report\" flagged as a Boss Task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete the Boss Task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive both recurring completion XP and Boss Task bonus XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the next recurring instance should be generated as a normal task",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@core",
          "@boss-tasks"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "core/notifications.feature",
      "Feature": {
        "Name": "Notifications and Reminders",
        "Description": "As a Waypoint user\nI want intelligent notifications that help me stay on track\nSo that I am reminded at the right time without being overwhelmed",
        "FeatureElements": [
          {
            "Examples": [],
            "Name": "Reminder for task due today",
            "Slug": "reminder-for-task-due-today",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task \"Submit report\" due today",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have not completed it",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "it reaches my configured reminder time",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive a notification reminding me about \"Submit report\"",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Reminder for upcoming deadline",
            "Slug": "reminder-for-upcoming-deadline",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task \"Prepare presentation\" due in 2 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have notifications enabled for upcoming deadlines",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the 2-day-before reminder triggers",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive a notification about the approaching deadline",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "No reminder for completed tasks",
            "Slug": "no-reminder-for-completed-tasks",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task \"Buy milk\" due today",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have already completed it",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the reminder time arrives",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should not receive a notification for \"Buy milk\"",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Repeated reminders for overdue tasks",
            "Slug": "repeated-reminders-for-overdue-tasks",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task \"Submit report\" that is 2 days overdue",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have not completed or skipped it",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive a daily reminder until the task is completed, skipped, or deleted",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [
              {
                "Name": "",
                "TableArgument": {
                  "HeaderRow": [
                    "achievement"
                  ],
                  "DataRows": [
                    [
                      "Level up",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Title earned",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Streak milestone reached",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Skill tree unlocked",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Quest completed",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Boss Task defeated",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Season rank achieved",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ]
                  ]
                },
                "Tags": [],
                "NativeKeyword": "Examples"
              }
            ],
            "Name": "Notification for achievement",
            "Slug": "notification-for-achievement",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have triggered the achievement \"<achievement>\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive a notification celebrating \"<achievement>\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the notification should include a positive message and achievement icon",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the notification should auto-dismiss after 5 seconds",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Receive an in-app notification",
            "Slug": "receive-an-in-app-notification",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task reminder triggered",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the notification is delivered",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see an in-app notification badge",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the notification in my notification centre",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Receive a push notification",
            "Slug": "receive-a-push-notification",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have push notifications enabled",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "a task reminder is triggered while I am not in the app",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the notification is delivered",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive a push notification on my device",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Tap a notification to navigate to the relevant item",
            "Slug": "tap-a-notification-to-navigate-to-the-relevant-item",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have received a notification about the task \"Submit report\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I tap the notification",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should be navigated to the task detail view for \"Submit report\"",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Batch notifications when many arrive simultaneously",
            "Slug": "batch-notifications-when-many-arrive-simultaneously",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "5 achievement notifications are triggered within 10 seconds",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the notifications should be grouped into a single summary notification",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the summary should indicate the number of achievements earned",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to expand the summary to see individual achievements",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Configure notification categories",
            "Slug": "configure-notification-categories",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to notification settings",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should be able to toggle notifications for each category:",
                "TableArgument": {
                  "HeaderRow": [
                    "Category",
                    "Default"
                  ],
                  "DataRows": [
                    [
                      "Task reminders",
                      "On"
                    ],
                    [
                      "Achievement alerts",
                      "On"
                    ],
                    [
                      "Daily brief ready",
                      "On"
                    ],
                    [
                      "Weekly review prompt",
                      "On"
                    ],
                    [
                      "Guild activity",
                      "On"
                    ],
                    [
                      "Partner messages",
                      "On"
                    ],
                    [
                      "Insight cards",
                      "On"
                    ],
                    [
                      "Capacity warnings",
                      "On"
                    ],
                    [
                      "Upgrade prompts",
                      "Off"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Set quiet hours",
            "Slug": "set-quiet-hours",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I set quiet hours from 10 PM to 7 AM",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "no notifications should be delivered during that window",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "queued notifications should be delivered after 7 AM",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Quiet hours respect user timezone",
            "Slug": "quiet-hours-respect-user-timezone",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have set quiet hours from 10 PM to 7 AM",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my timezone is set to \"Europe/London\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "a notification is triggered at 11 PM London time",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the notification should be queued until 7 AM London time",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Disable all notifications",
            "Slug": "disable-all-notifications",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I disable all notifications",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive no push notifications",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "in-app indicators should still show for unread items",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@core",
          "@notifications"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "core/quest-hierarchy.feature",
      "Feature": {
        "Name": "Quest Hierarchy",
        "Description": "As a Waypoint user\nI want to organise tasks into quests, epics, and sagas\nSo that individual tasks connect to meaningful larger goals",
        "FeatureElements": [
          {
            "Examples": [],
            "Name": "Create a quest",
            "Slug": "create-a-quest",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I create a quest with the following details:",
                "TableArgument": {
                  "HeaderRow": [
                    "Field",
                    "Value"
                  ],
                  "DataRows": [
                    [
                      "Title",
                      "Prepare conference talk"
                    ],
                    [
                      "Description",
                      "Write and rehearse DDD talk for NDC"
                    ],
                    [
                      "Due Date",
                      "2026-06-01"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the quest \"Prepare conference talk\" should be created",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it should appear in my quest list",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it should have a progress of 0%",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Add tasks to a quest",
            "Slug": "add-tasks-to-a-quest",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a quest \"Prepare conference talk\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I add the following tasks to the quest:",
                "TableArgument": {
                  "HeaderRow": [
                    "Title"
                  ],
                  "DataRows": [
                    [
                      "Write abstract"
                    ],
                    [
                      "Create slide deck"
                    ],
                    [
                      "Build demo project"
                    ],
                    [
                      "First rehearsal"
                    ],
                    [
                      "Final rehearsal"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the quest should contain 5 tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the quest progress should be 0%",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Quest progress updates as tasks complete",
            "Slug": "quest-progress-updates-as-tasks-complete",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a quest \"Prepare conference talk\" with 5 tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "0 tasks are completed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete the task \"Write abstract\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the quest progress should be 20%",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Complete a quest",
            "Slug": "complete-a-quest",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a quest \"Prepare conference talk\" with 5 tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "4 tasks are completed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete the remaining task \"Final rehearsal\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the quest progress should be 100%",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the quest status should change to \"Completed\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive quest completion bonus XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "a celebration animation should be displayed",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View quest details",
            "Slug": "view-quest-details",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a quest \"Prepare conference talk\" with 5 tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "3 tasks are completed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the quest details",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see the quest title and description",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see a progress bar showing 60%",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see all 5 tasks with their statuses",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the total XP earned so far",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the estimated remaining effort",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Move a task between quests",
            "Slug": "move-a-task-between-quests",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a quest \"Work tasks\" containing the task \"Update docs\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have a quest \"Side project\" with 2 tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I move the task \"Update docs\" to the quest \"Side project\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "\"Work tasks\" should no longer contain \"Update docs\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "\"Side project\" should contain 3 tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Remove a task from a quest without deleting it",
            "Slug": "remove-a-task-from-a-quest-without-deleting-it",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a quest \"Sprint work\" containing the task \"Fix CSS bug\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I unassign the task \"Fix CSS bug\" from the quest",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task should appear in my inbox",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the quest progress should be recalculated",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Delete a quest",
            "Slug": "delete-a-quest",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a quest \"Abandoned project\" containing 3 tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I delete the quest \"Abandoned project\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I confirm the deletion",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the quest should be removed from my quest list",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the 3 tasks should be moved to my inbox",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the quest XP bonus should not be affected for completed tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "A quest cannot belong to more than one epic",
            "Slug": "a-quest-cannot-belong-to-more-than-one-epic",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a quest \"Build authentication\" assigned to the epic \"Launch MVP\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I attempt to assign the quest to the epic \"Side Project\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a message indicating the quest already belongs to an epic",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be offered the option to move it instead",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Create an epic",
            "Slug": "create-an-epic",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I create an epic with the following details:",
                "TableArgument": {
                  "HeaderRow": [
                    "Field",
                    "Value"
                  ],
                  "DataRows": [
                    [
                      "Title",
                      "Launch MVP"
                    ],
                    [
                      "Description",
                      "Ship the first public version of the app"
                    ],
                    [
                      "Target Date",
                      "2026-09-01"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the epic \"Launch MVP\" should be created",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it should appear in my epic list",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Assign quests to an epic",
            "Slug": "assign-quests-to-an-epic",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an epic \"Launch MVP\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have the following quests:",
                "TableArgument": {
                  "HeaderRow": [
                    "Quest Title"
                  ],
                  "DataRows": [
                    [
                      "Build authentication"
                    ],
                    [
                      "Implement task engine"
                    ],
                    [
                      "Design UI"
                    ],
                    [
                      "Beta testing"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I assign all four quests to the epic \"Launch MVP\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the epic should contain 4 quests",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the epic progress should reflect aggregate quest progress",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Epic progress reflects quest completion with equal weighting",
            "Slug": "epic-progress-reflects-quest-completion-with-equal-weighting",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an epic \"Launch MVP\" with 4 quests",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each quest contributes equally to epic progress regardless of task count",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the quest \"Build authentication\" is 100% complete",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the quest \"Implement task engine\" is 50% complete",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the other quests are 0% complete",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the epic progress",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the epic progress should be 37.5%",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Complete an epic",
            "Slug": "complete-an-epic",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an epic \"Launch MVP\" with 4 quests",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "3 quests are completed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the final quest is completed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the epic status should change to \"Completed\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive epic completion bonus XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "a milestone event should appear on my journey timeline",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Delete an epic",
            "Slug": "delete-an-epic",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an epic \"Abandoned initiative\" containing 3 quests",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I delete the epic \"Abandoned initiative\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I confirm the deletion",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the epic should be removed from my epic list",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the 3 quests should remain intact but no longer belong to any epic",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Remove a quest from an epic",
            "Slug": "remove-a-quest-from-an-epic",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an epic \"Launch MVP\" with 4 quests",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I remove the quest \"Beta testing\" from the epic",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the epic should contain 3 quests",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the epic progress should be recalculated",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Create a saga",
            "Slug": "create-a-saga",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a premium subscription",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I create a saga with the following details:",
                "TableArgument": {
                  "HeaderRow": [
                    "Field",
                    "Value"
                  ],
                  "DataRows": [
                    [
                      "Title",
                      "Launch my SaaS business"
                    ],
                    [
                      "Description",
                      "Go from idea to paying customers"
                    ],
                    [
                      "Vision",
                      "Build a sustainable product that solves a real problem"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the saga \"Launch my SaaS business\" should be created",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it should appear in my saga view",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it should have no target date by default",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip",
              "@premium"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Assign epics to a saga",
            "Slug": "assign-epics-to-a-saga",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a saga \"Launch my SaaS business\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have epics \"Launch MVP\" and \"Acquire first 100 users\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I assign both epics to the saga",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the saga should contain 2 epics",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the saga progress should reflect aggregate epic progress",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip",
              "@premium"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View saga timeline",
            "Slug": "view-saga-timeline",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a saga \"Launch my SaaS business\" with 3 epics",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "work has been ongoing for 4 months",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the saga timeline",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a visual representation of progress over time",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see completed and in-progress epics",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see a projected completion trajectory",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip",
              "@premium"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Free-tier user attempts to create a saga",
            "Slug": "free-tier-user-attempts-to-create-a-saga",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a free-tier account",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I attempt to create a saga",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a message explaining sagas are a premium feature",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be offered the option to upgrade",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should still be able to create tasks, quests, and epics",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "An epic cannot belong to more than one saga",
            "Slug": "an-epic-cannot-belong-to-more-than-one-saga",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an epic \"Launch MVP\" assigned to the saga \"Launch my SaaS business\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I attempt to assign the epic to the saga \"Career growth\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a message indicating the epic already belongs to a saga",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be offered the option to move it instead",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip",
              "@premium"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View task context within hierarchy",
            "Slug": "view-task-context-within-hierarchy",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task \"Write unit tests\" in the quest \"Build authentication\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the quest belongs to the epic \"Launch MVP\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the epic belongs to the saga \"Launch my SaaS business\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the task \"Write unit tests\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see the full breadcrumb: Saga > Epic > Quest > Task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each level should be clickable for navigation",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View all unassigned tasks",
            "Slug": "view-all-unassigned-tasks",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have 10 tasks total",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "6 tasks are assigned to quests",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "4 tasks are not assigned to any quest",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view unassigned tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see exactly 4 tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be offered suggestions to group them into quests",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@core",
          "@quests"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "core/recurring-tasks.feature",
      "Feature": {
        "Name": "Recurring Tasks and Quest Chains",
        "Description": "As a Waypoint user\nI want to set up recurring tasks and quest chains\nSo that repetitive workflows are automated and tracked consistently",
        "FeatureElements": [
          {
            "Examples": [],
            "Name": "Create a daily recurring task",
            "Slug": "create-a-daily-recurring-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I create a recurring task with the following details:",
                "TableArgument": {
                  "HeaderRow": [
                    "Field",
                    "Value"
                  ],
                  "DataRows": [
                    [
                      "Title",
                      "Morning standup prep"
                    ],
                    [
                      "Recurrence",
                      "Daily"
                    ],
                    [
                      "Time",
                      "08:30"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task should appear in my Today view each day at 08:30",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each instance should be a separate completable task",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Create a weekly recurring task",
            "Slug": "create-a-weekly-recurring-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I create a recurring task with the following details:",
                "TableArgument": {
                  "HeaderRow": [
                    "Field",
                    "Value"
                  ],
                  "DataRows": [
                    [
                      "Title",
                      "Weekly meal prep"
                    ],
                    [
                      "Recurrence",
                      "Weekly on Sunday"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task should appear every Sunday",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "completing one instance should not affect future instances",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Create a monthly recurring task",
            "Slug": "create-a-monthly-recurring-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I create a recurring task with the following details:",
                "TableArgument": {
                  "HeaderRow": [
                    "Field",
                    "Value"
                  ],
                  "DataRows": [
                    [
                      "Title",
                      "Submit expense report"
                    ],
                    [
                      "Recurrence",
                      "Monthly on the last Friday"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task should appear on the last Friday of each month",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Create a recurring task with an end date",
            "Slug": "create-a-recurring-task-with-an-end-date",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I create a recurring task with the following details:",
                "TableArgument": {
                  "HeaderRow": [
                    "Field",
                    "Value"
                  ],
                  "DataRows": [
                    [
                      "Title",
                      "Sprint retrospective"
                    ],
                    [
                      "Recurrence",
                      "Weekly on Friday"
                    ],
                    [
                      "End Date",
                      "2026-06-30"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task should appear every Friday until 2026-06-30",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "no instances should be generated after the end date",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Complete a recurring task instance",
            "Slug": "complete-a-recurring-task-instance",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a daily recurring task \"Morning standup prep\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "today's instance is open",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete today's instance",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "today's instance should be marked as completed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive XP for the completion",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "tomorrow's instance should be generated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the recurring task streak should increment by 1",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Complete a recurring task instance late",
            "Slug": "complete-a-recurring-task-instance-late",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a daily recurring task \"Morning standup prep\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "yesterday's instance is still open",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete yesterday's instance",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the instance should be marked as completed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive XP with an overdue penalty applied",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my streak should be broken",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Skip a recurring task instance",
            "Slug": "skip-a-recurring-task-instance",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a daily recurring task \"Morning standup prep\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "today's instance is open",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I skip today's instance",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "today's instance should be marked as \"Skipped\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "no XP should be awarded or deducted",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the streak counter should freeze at its current value",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the skip should appear in my recurring task history",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Pause a recurring task",
            "Slug": "pause-a-recurring-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a weekly recurring task \"Team retrospective\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I pause the recurring task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "no new instances should be generated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "existing uncompleted instances should remain",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the task should show a \"Paused\" status",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Resume a paused recurring task",
            "Slug": "resume-a-paused-recurring-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a paused recurring task \"Team retrospective\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I resume the recurring task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "new instances should begin generating again",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the streak counter should resume from where it was paused",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Edit all future instances of a recurring task",
            "Slug": "edit-all-future-instances-of-a-recurring-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a daily recurring task \"Check email\" at 09:00",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I edit the recurring task time to 08:00",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I choose to apply changes to all future instances",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "all future instances should be scheduled at 08:00",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "past instances should remain unchanged",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Delete a recurring task",
            "Slug": "delete-a-recurring-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a weekly recurring task \"Water plants\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I delete the recurring task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I confirm the deletion",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "no future instances should be generated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "completed past instances should remain in my history",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the XP earned from past instances should be retained",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Handle overlapping recurring task instances",
            "Slug": "handle-overlapping-recurring-task-instances",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a daily recurring task \"Morning standup prep\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "yesterday's instance is still open",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "today's instance is generated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "both yesterday's and today's instances should be visible",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "yesterday's instance should be marked as overdue",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "User receives a suggestion for a recurring quest pattern",
            "Slug": "user-receives-a-suggestion-for-a-recurring-quest-pattern",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have completed the following quests in the last 3 weeks:",
                "TableArgument": {
                  "HeaderRow": [
                    "Quest Title",
                    "Completed On"
                  ],
                  "DataRows": [
                    [
                      "Weekly meal prep",
                      "2026-03-01"
                    ],
                    [
                      "Weekly meal prep",
                      "2026-03-08"
                    ],
                    [
                      "Weekly meal prep",
                      "2026-03-15"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view my quest insights",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a suggestion to create a quest chain for \"Weekly meal prep\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the suggestion should include the detected cadence of \"Weekly\"",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Create a quest chain from a template",
            "Slug": "create-a-quest-chain-from-a-template",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I create a quest chain with the following details:",
                "TableArgument": {
                  "HeaderRow": [
                    "Field",
                    "Value"
                  ],
                  "DataRows": [
                    [
                      "Title",
                      "Weekly Meal Prep"
                    ],
                    [
                      "Cadence",
                      "Weekly on Saturday"
                    ],
                    [
                      "Tasks",
                      "Plan meals, Write shopping list, Buy ingredients, Prep ingredients"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "a new quest should be auto-generated every Saturday",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each quest should contain the 4 specified tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each quest should have a 24-hour default deadline",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Quest chain adapts task list over time",
            "Slug": "quest-chain-adapts-task-list-over-time",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a quest chain \"Weekly Meal Prep\" running for 4 weeks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have consistently added an extra task \"Clean kitchen\" to each instance",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the next instance is generated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the system should suggest adding \"Clean kitchen\" to the chain template",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I accept the suggestion",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "all future instances should include \"Clean kitchen\"",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View quest chain history and stats",
            "Slug": "view-quest-chain-history-and-stats",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a quest chain \"Weekly Meal Prep\" running for 8 weeks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the quest chain details",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see the completion rate across all instances",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the average time to complete each instance",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the streak of consecutive completions",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the total XP earned from the chain",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Quest chain generates bonus XP for consistency",
            "Slug": "quest-chain-generates-bonus-xp-for-consistency",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a quest chain \"Weekly Meal Prep\" with a 4-week streak",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the 5th consecutive instance is completed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive the standard quest completion XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive a chain consistency bonus multiplier",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the bonus should increase with longer streaks",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@core",
          "@recurring"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "core/task-management.feature",
      "Feature": {
        "Name": "Task Management",
        "Description": "As a Waypoint user\nI want to create, organise, and complete tasks\nSo that I can track and accomplish my work effectively",
        "FeatureElements": [
          {
            "Examples": [],
            "Name": "Create a task with only a title",
            "Slug": "create-a-task-with-only-a-title",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I create a task with the title \"Buy groceries\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task \"Buy groceries\" should appear in my inbox",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the task should have no due date",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the task should have no quest assignment",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the task should have a default difficulty of \"Normal\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the task should have a status of \"Open\"",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Create a task with full details",
            "Slug": "create-a-task-with-full-details",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I create a task with the following details:",
                "TableArgument": {
                  "HeaderRow": [
                    "Field",
                    "Value"
                  ],
                  "DataRows": [
                    [
                      "Title",
                      "Write Q2 report"
                    ],
                    [
                      "Description",
                      "Quarterly financial summary for stakeholders"
                    ],
                    [
                      "Due Date",
                      "2026-04-15"
                    ],
                    [
                      "Estimated Time",
                      "2 hours"
                    ],
                    [
                      "Priority",
                      "High"
                    ],
                    [
                      "Tags",
                      "work, reporting"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task \"Write Q2 report\" should appear in my inbox",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the task should have all specified details saved",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Create a task with natural language date parsing",
            "Slug": "create-a-task-with-natural-language-date-parsing",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I create a task with the title \"Call dentist\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I set the due date to \"next Tuesday\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the due date should resolve to the next occurring Tuesday",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the task should appear in my upcoming view on that date",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Create a task via quick-add from any screen",
            "Slug": "create-a-task-via-quick-add-from-any-screen",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am on any screen in the application",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I activate the quick-add shortcut",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I type \"Submit tax return #personal !high ^April 15\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "a task \"Submit tax return\" should be created",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it should be tagged \"personal\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it should have priority \"High\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it should have a due date of April 15",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Reject a task with an empty title",
            "Slug": "reject-a-task-with-an-empty-title",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I attempt to create a task with an empty title",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task should not be created",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see a validation error indicating a title is required",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Complete a simple task",
            "Slug": "complete-a-simple-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an open task \"Buy groceries\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I mark the task \"Buy groceries\" as complete",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task status should change to \"Completed\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the completion timestamp should be recorded",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive XP for the task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the task should appear in my completed tasks history",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Complete a task with an estimated time",
            "Slug": "complete-a-task-with-an-estimated-time",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an open task \"Write Q2 report\" with an estimated time of 2 hours",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I mark the task as complete",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task status should change to \"Completed\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be prompted to record actual time spent",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Record actual time spent after completing a task",
            "Slug": "record-actual-time-spent-after-completing-a-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have just completed the task \"Write Q2 report\" with an estimated time of 2 hours",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I record the actual time spent as \"2 hours 45 minutes\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the time estimation variance should be recorded as +37.5%",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the variance should be visible in my estimation history",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Complete a task that is overdue",
            "Slug": "complete-a-task-that-is-overdue",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task \"Submit proposal\" that was due 3 days ago",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I mark the task as complete",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task should be marked as completed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the XP awarded should reflect the overdue penalty",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the XP awarded should still be greater than zero",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Complete the final task in a quest",
            "Slug": "complete-the-final-task-in-a-quest",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a quest \"Prepare presentation\" with 5 tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "4 of the 5 tasks are completed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete the remaining task \"Do final rehearsal\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the quest \"Prepare presentation\" should be marked as complete",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive quest completion bonus XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "a quest completion event should appear on my journey timeline",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Complete an already-completed task",
            "Slug": "complete-an-already-completed-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a completed task \"Buy groceries\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I attempt to mark the task as complete again",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task status should remain \"Completed\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "no additional XP should be awarded",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Re-open a completed task",
            "Slug": "re-open-a-completed-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a completed task \"Submit report\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I re-open the task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task status should change to \"Open\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the XP previously earned for completing it should be deducted",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the task should reappear in my active task list",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Edit a task title",
            "Slug": "edit-a-task-title",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an open task \"Buy grocries\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I edit the task title to \"Buy groceries\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task title should be updated to \"Buy groceries\"",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Change task priority",
            "Slug": "change-task-priority",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an open task \"Update website\" with priority \"Low\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I change the priority to \"High\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task priority should be \"High\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the task difficulty rating should be updated accordingly",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Reschedule a task",
            "Slug": "reschedule-a-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an open task \"Team lunch\" due on \"2026-04-10\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I change the due date to \"2026-04-17\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the due date should be updated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "a reschedule event should be recorded against the task",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Add a description to an existing task",
            "Slug": "add-a-description-to-an-existing-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an open task \"Research competitors\" with no description",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I add the description \"Focus on gamified productivity apps in the market\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task description should be saved",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Edit a completed task",
            "Slug": "edit-a-completed-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a completed task \"Submit report\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I edit the task description to \"Updated summary\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task description should be saved",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the task status should remain \"Completed\"",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Delete a task",
            "Slug": "delete-a-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an open task \"Cancelled meeting prep\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I delete the task \"Cancelled meeting prep\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I confirm the deletion",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task should be removed from my task list",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "no XP should be awarded or deducted",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Cancel a task deletion",
            "Slug": "cancel-a-task-deletion",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an open task \"Important work\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I delete the task \"Important work\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I cancel the deletion",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task should remain in my task list",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Delete a task that belongs to a quest",
            "Slug": "delete-a-task-that-belongs-to-a-quest",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a quest \"Launch campaign\" containing 4 tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "one task is \"Design flyer\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I delete the task \"Design flyer\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I confirm the deletion",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the quest \"Launch campaign\" should show 3 remaining tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the quest progress should be recalculated",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Delete a completed task",
            "Slug": "delete-a-completed-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a completed task \"Old report\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I delete the task \"Old report\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I confirm the deletion",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task should be removed from my completed tasks history",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the XP earned from the task should be retained",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Filter tasks by tag",
            "Slug": "filter-tasks-by-tag",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have the following tasks:",
                "TableArgument": {
                  "HeaderRow": [
                    "Title",
                    "Tags"
                  ],
                  "DataRows": [
                    [
                      "Fix login bug",
                      "work, dev"
                    ],
                    [
                      "Buy birthday gift",
                      "personal"
                    ],
                    [
                      "Update API docs",
                      "work, dev"
                    ],
                    [
                      "Book flights",
                      "personal, travel"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I filter by the tag \"work\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see 2 tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see \"Fix login bug\" and \"Update API docs\"",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Sort tasks by due date",
            "Slug": "sort-tasks-by-due-date",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have multiple tasks with different due dates",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I sort tasks by \"Due Date\" ascending",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "tasks should be ordered from earliest due date to latest",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "tasks with no due date should appear at the end",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Sort tasks by priority",
            "Slug": "sort-tasks-by-priority",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have tasks with priorities \"Low\", \"High\", \"Medium\", and \"Critical\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I sort tasks by \"Priority\" descending",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "tasks should be ordered: Critical, High, Medium, Low",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Search tasks by keyword",
            "Slug": "search-tasks-by-keyword",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have 20 tasks with various titles and descriptions",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I search for \"report\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see only tasks whose title or description contains \"report\"",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View tasks in Inbox",
            "Slug": "view-tasks-in-inbox",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to the Inbox view",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see all tasks not assigned to a quest",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "tasks should be sorted by creation date descending",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View tasks in Today view",
            "Slug": "view-tasks-in-today-view",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to the Today view",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see all tasks due today",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see all overdue tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see tasks from my Smart Daily Brief if generated",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View tasks in Upcoming view",
            "Slug": "view-tasks-in-upcoming-view",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to the Upcoming view",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see tasks grouped by due date",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the next 14 days by default",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "days with no tasks should still be visible",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View completed tasks history",
            "Slug": "view-completed-tasks-history",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to the Completed view",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see all completed tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "tasks should be grouped by completion date",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each task should show the XP that was earned",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@core",
          "@tasks"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "data/local-first-data.feature",
      "Feature": {
        "Name": "Local-First Data and Export",
        "Description": "As a Waypoint user\nI want my data stored locally by default with full export capabilities\nSo that I own my data and am never locked into the platform",
        "FeatureElements": [
          {
            "Examples": [],
            "Name": "App works without internet connection",
            "Slug": "app-works-without-internet-connection",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have no internet connection",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I open Waypoint",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should be able to view all my tasks, quests, and progression",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to create new tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to complete tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to earn XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "all changes should be saved locally",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Data persists across app restarts",
            "Slug": "data-persists-across-app-restarts",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have created 10 tasks and completed 5",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I close and reopen the app",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "all 10 tasks should be present",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "5 should show as completed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my XP and level should be correct",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "No data sent to servers without explicit opt-in",
            "Slug": "no-data-sent-to-servers-without-explicit-opt-in",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a free-tier account without sync enabled",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I use the app for a full week",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "no task data should be transmitted to external servers",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "all analytics should be computed on-device",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the only network calls should be for authentication and subscription validation",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Enable cross-device sync",
            "Slug": "enable-cross-device-sync",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a premium subscription",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I enable cross-device sync in settings",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a clear explanation of what data will be synced",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should confirm my consent",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I confirm",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my data should begin syncing to the cloud",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "changes should propagate to my other devices",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@premium",
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Sync conflict resolution",
            "Slug": "sync-conflict-resolution",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have sync enabled on two devices",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I complete a task on device A while offline",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I edit the same task on device B while offline",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "both devices come online",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the system should detect the conflict",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the change with the most recent server-side timestamp should take priority",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "both versions should be available in a conflict log for manual review",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Disable sync and delete cloud data",
            "Slug": "disable-sync-and-delete-cloud-data",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have sync enabled",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I disable sync",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should be offered the option to delete all cloud-stored data",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I confirm cloud data deletion",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "all my data should be removed from the server",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my local data should remain intact",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the app should continue working offline",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Social features require server-side state",
            "Slug": "social-features-require-server-side-state",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am a member of a guild with shared quests",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have no internet connection",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view guild and shared quest data",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see the last-synced state of guild and shared quest data",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see a notice that social data may be outdated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should not be able to modify guild or shared quest data while offline",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Export all data as JSON",
            "Slug": "export-all-data-as-json",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to data export settings",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I choose to export all data as JSON",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "a complete JSON file should be generated containing:",
                "TableArgument": {
                  "HeaderRow": [
                    "Data Type"
                  ],
                  "DataRows": [
                    [
                      "All tasks"
                    ],
                    [
                      "All quests"
                    ],
                    [
                      "All epics"
                    ],
                    [
                      "All sagas"
                    ],
                    [
                      "XP history"
                    ],
                    [
                      "Level history"
                    ],
                    [
                      "Skill tree progress"
                    ],
                    [
                      "Titles earned"
                    ],
                    [
                      "Weekly reviews"
                    ],
                    [
                      "Timeline events"
                    ],
                    [
                      "Insight cards"
                    ],
                    [
                      "Settings"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the file should be downloadable to my device",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Export tasks as CSV",
            "Slug": "export-tasks-as-csv",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I choose to export tasks as CSV",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "a CSV file should be generated with all task data",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the CSV should include all fields: title, description, status, dates, tags, XP, difficulty, quest assignment",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the file should be compatible with spreadsheet applications",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Export is always available regardless of subscription",
            "Slug": "export-is-always-available-regardless-of-subscription",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a free-tier account",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to data export",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the full JSON and CSV export options should be available",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "no export functionality should be restricted by tier",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Import data from a previous export",
            "Slug": "import-data-from-a-previous-export",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to data import settings",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I select a previously exported JSON file",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a preview of the data to be imported",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be warned that importing will overwrite existing data",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I confirm the import",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "all data from the export file should be restored",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my XP, level, and progression should reflect the imported state",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Scheduled automatic export",
            "Slug": "scheduled-automatic-export",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a premium subscription",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I configure a weekly automatic export",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "a JSON backup should be generated every week",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it should be stored in my designated local directory",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the 4 most recent backups should be retained",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Scheduled export when local directory is unavailable",
            "Slug": "scheduled-export-when-local-directory-is-unavailable",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have configured a weekly automatic export",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the designated local directory is unavailable",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the scheduled export runs",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive a notification that the export failed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the reason for the failure should be explained",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the system should retry at the next scheduled interval",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Delete all data",
            "Slug": "delete-all-data",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I choose to delete all my Waypoint data",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a warning about permanent data loss",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be required to type a confirmation phrase",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I confirm",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "all local data should be permanently deleted",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "all cloud data (if sync was enabled) should be permanently deleted",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my account should remain active but empty",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Delete account entirely",
            "Slug": "delete-account-entirely",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I choose to delete my account",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a warning about permanent account and data loss",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be offered a final export before deletion",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I confirm account deletion",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "all data should be permanently deleted",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my account should be deactivated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my username should be released after a 30-day holding period",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Recover account during the 30-day holding period",
            "Slug": "recover-account-during-the-30-day-holding-period",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have deleted my account within the last 30 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I sign in using my original social login provider",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should be prompted to recover my account",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I confirm recovery",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my account should be reactivated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "all data should be restored to its pre-deletion state",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the 30-day holding period should be cancelled",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@data",
          "@local-first"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "intelligence/energy-scheduling.feature",
      "Feature": {
        "Name": "Energy-Aware Scheduling",
        "Description": "As a Waypoint user\nI want tasks surfaced based on my current energy level\nSo that I tackle hard work when I am sharp and routine work when I am depleted",
        "FeatureElements": [
          {
            "Examples": [],
            "Name": "Manually set energy level at start of session",
            "Slug": "manually-set-energy-level-at-start-of-session",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I open Waypoint for my first session of the day",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see an optional energy check-in prompt",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I set my energy level to \"High\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my current energy should be recorded as \"High\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my task suggestions should prioritise difficult tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Skip energy check-in with sufficient history",
            "Slug": "skip-energy-check-in-with-sufficient-history",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have at least 14 days of task completion data",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I open Waypoint for my first session of the day",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I dismiss the energy check-in prompt",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the system should infer my energy from historical patterns",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the prompt should not appear again until the next session",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Skip energy check-in on day 1 with no historical data",
            "Slug": "skip-energy-check-in-on-day-1-with-no-historical-data",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have no task completion history",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I open Waypoint for my first session of the day",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I dismiss the energy check-in prompt",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the system should default my energy level to \"Medium\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "tasks should be shown in standard priority order",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the prompt should not appear again until the next session",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [
              {
                "Name": "",
                "TableArgument": {
                  "HeaderRow": [
                    "energy",
                    "priority_type"
                  ],
                  "DataRows": [
                    [
                      "High",
                      "Hard and complex tasks",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Medium",
                      "Normal difficulty tasks",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Low",
                      "Easy, routine, and administrative tasks",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ]
                  ]
                },
                "Tags": [],
                "NativeKeyword": "Examples"
              }
            ],
            "Name": "Energy level affects task surfacing",
            "Slug": "energy-level-affects-task-surfacing",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my current energy level is \"<energy>\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view my Today tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task ordering should prioritise \"<priority_type>\" tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "New user with insufficient data for pattern inference",
            "Slug": "new-user-with-insufficient-data-for-pattern-inference",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have fewer than 14 days of task completion data",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I dismiss the energy check-in prompt",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the system should display a message \"We're still learning your energy patterns — check in daily for personalised suggestions after 14 days\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the system should default my energy level to \"Medium\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "tasks should be shown in standard priority order",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "System infers energy from time-of-day patterns",
            "Slug": "system-infers-energy-from-time-of-day-patterns",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have at least 14 days of task completion data",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the system has detected that I complete hard tasks most often between 9 AM and 12 PM",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it is currently 10 AM",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I did not provide an energy check-in",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view my Today tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the system should infer \"High\" energy",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "difficult tasks should be surfaced first",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Energy pattern detected across weeks",
            "Slug": "energy-pattern-detected-across-weeks",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have consistently reported \"High\" energy on weekday mornings",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have consistently reported \"Low\" energy on Friday afternoons",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the system analyses my energy patterns",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "it should build a weekly energy profile for me",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the profile should be visible in my productivity insights",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Energy inference improves with data",
            "Slug": "energy-inference-improves-with-data",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have provided energy check-ins for 14 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I skip a check-in on a typical Wednesday morning",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the system should infer my energy based on the pattern",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the confidence of the inference should be moderate",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have provided energy check-ins for 60 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I skip a check-in on a typical Wednesday morning",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the confidence of the inference should be high",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Mid-day energy shift recommendation",
            "Slug": "mid-day-energy-shift-recommendation",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my energy was \"High\" this morning",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it is now 2 PM",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the system detects I typically experience an energy dip at this time",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I return to my task list",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the system should suggest switching to easier tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "a gentle prompt should say something like \"Energy usually dips around now — lighter tasks might be a good fit\"",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Mid-day re-check-in after rapid energy fluctuation",
            "Slug": "mid-day-re-check-in-after-rapid-energy-fluctuation",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my energy was \"High\" this morning at 9 AM",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it is now 11 AM",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I manually re-check-in and set my energy level to \"Low\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my current energy should be updated to \"Low\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my task suggestions should immediately reprioritise to show easier tasks first",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the system should record the rapid fluctuation for future pattern analysis",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Energy-aware reordering does not hide tasks",
            "Slug": "energy-aware-reordering-does-not-hide-tasks",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my energy level is \"Low\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view my Today tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "difficult tasks should still be visible and accessible",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "But",
                "NativeKeyword": "But ",
                "Name": "they should be ordered below easier tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "a label should indicate the ordering is energy-aware",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@intelligence",
          "@energy"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "intelligence/daily-brief.feature",
      "Feature": {
        "Name": "Smart Daily Brief",
        "Description": "As a Waypoint user\nI want a personalised daily plan each morning\nSo that I start my day with clarity and focus on the right tasks",
        "FeatureElements": [
          {
            "Examples": [],
            "Name": "Daily brief generated on first session",
            "Slug": "daily-brief-generated-on-first-session",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "it is a new day and I have not opened Waypoint yet",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have 8 tasks due today or overdue",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have energy pattern data available",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I open Waypoint",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "a Smart Daily Brief should be generated and displayed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it should recommend a prioritised task sequence for the day",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the sequence should account for my energy patterns",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the sequence should account for task deadlines and priorities",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Daily brief factors in calendar blocks",
            "Slug": "daily-brief-factors-in-calendar-blocks",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a premium subscription with calendar integration",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have a 2-hour meeting block from 10 AM to 12 PM",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have 6 tasks to schedule today",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the daily brief is generated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "no tasks should be suggested during the 10 AM to 12 PM block",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "harder tasks should be suggested for my peak energy windows outside the meeting",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip",
              "@premium"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Daily brief without calendar integration",
            "Slug": "daily-brief-without-calendar-integration",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I do not have calendar integration enabled",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have 6 tasks to schedule today",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have energy pattern data available",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the daily brief is generated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the brief should be generated from tasks only",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it should recommend a prioritised task sequence based on energy patterns and deadlines",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "no calendar-related scheduling adjustments should be applied",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Daily brief highlights overdue tasks",
            "Slug": "daily-brief-highlights-overdue-tasks",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have 3 overdue tasks and 5 tasks due today",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the daily brief is generated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "overdue tasks should appear at the top of the brief with a clear indicator",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the brief should suggest addressing at least 1 overdue task first",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Daily brief respects capacity model",
            "Slug": "daily-brief-respects-capacity-model",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my capacity model indicates I typically complete 6 tasks on this day of the week",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have 10 tasks due today",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the daily brief is generated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the brief should recommend 6 priority tasks as the core plan",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the remaining 4 should be listed as \"if time allows\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see a note about today exceeding typical capacity",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Accept the daily brief as-is",
            "Slug": "accept-the-daily-brief-as-is",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the daily brief recommends 6 tasks in a specific order",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I accept the daily brief",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my Today view should reorder to match the brief",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "a \"Following daily brief\" indicator should be visible",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Modify the daily brief",
            "Slug": "modify-the-daily-brief",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the daily brief recommends 6 tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I reorder the tasks in the brief",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I remove 1 task and add a different one",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I confirm the modified brief",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my Today view should reflect my modifications",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the system should learn from my modifications for future briefs",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "User modifies brief to exceed capacity limit",
            "Slug": "user-modifies-brief-to-exceed-capacity-limit",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the daily brief recommends 6 tasks matching my capacity model",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I add additional tasks to the brief beyond my capacity of 6",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the system should show a gentle warning \"This plan exceeds your typical daily capacity of 6 tasks — you may want to mark some as 'if time allows'\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should still be able to confirm the modified brief",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Dismiss the daily brief",
            "Slug": "dismiss-the-daily-brief",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the daily brief is displayed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I dismiss the daily brief",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my Today view should show the default task ordering",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the brief should not reappear until the next day",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Brief not generated when insufficient tasks",
            "Slug": "brief-not-generated-when-insufficient-tasks",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have fewer than 2 tasks due today",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I open Waypoint",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "no daily brief should be generated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see my standard Today view",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Brief accuracy improves with feedback",
            "Slug": "brief-accuracy-improves-with-feedback",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have used the daily brief for 14 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I consistently move creative tasks earlier and defer admin tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the next daily brief is generated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "creative tasks should be scheduled earlier in the day",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "administrative tasks should be suggested later",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            },
            {
              "Keyword": "And",
              "NativeKeyword": "And ",
              "Name": "I have reached at least level 5",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@intelligence",
          "@daily-brief"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "intelligence/time-estimation.feature",
      "Feature": {
        "Name": "Time Estimation Learning",
        "Description": "As a Waypoint user\nI want the system to learn my estimation patterns and correct my biases\nSo that I can plan my time more accurately over time",
        "FeatureElements": [
          {
            "Examples": [],
            "Name": "Record estimation variance on task completion",
            "Slug": "record-estimation-variance-on-task-completion",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task \"Write blog post\" with estimated time of 1 hour",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete the task and record actual time as 1 hour 40 minutes",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the system should record a variance of +66.7%",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "this data point should feed into my estimation model",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Prompt for actual time only when estimate was provided",
            "Slug": "prompt-for-actual-time-only-when-estimate-was-provided",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task \"Buy milk\" with no time estimate",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete the task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should not be prompted for actual time spent",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "no estimation data should be recorded",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Optional time tracking during task execution",
            "Slug": "optional-time-tracking-during-task-execution",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task \"Code review\" with estimated time of 30 minutes",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I start a timer for the task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I stop the timer after 45 minutes",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the actual time should be auto-populated as 45 minutes",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to adjust the time before confirming",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Detect consistent underestimation for a task category",
            "Slug": "detect-consistent-underestimation-for-a-task-category",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have completed at least 10 tasks in the \"writing\" category over the last month",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my average estimation for writing tasks was 1 hour",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my average actual time for writing tasks was 1 hour 25 minutes",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the system analyses my estimation patterns",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "it should detect a +42% underestimation bias for writing tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "this bias should be stored in my estimation model",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Detect consistent overestimation for a task category",
            "Slug": "detect-consistent-overestimation-for-a-task-category",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have completed at least 10 tasks in the \"code review\" category over the last month",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my average estimation was 1 hour",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my average actual time was 35 minutes",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the system analyses my estimation patterns",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "it should detect a -42% overestimation bias for code review tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Detect dramatic overestimation",
            "Slug": "detect-dramatic-overestimation",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task \"Organise inbox\" with estimated time of 2 hours",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete the task and record actual time as 30 minutes",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the system should record a variance of -75%",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "this data point should feed into my estimation model",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "if this pattern recurs across 10 or more tasks in the same category the system should flag a significant overestimation bias",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "No bias detected when estimates are accurate",
            "Slug": "no-bias-detected-when-estimates-are-accurate",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have completed 12 meeting prep tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my average estimation variance is within the configurable accuracy threshold of ±15%",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the system analyses my estimation patterns",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "no bias should be flagged for meeting prep tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Suggest corrected estimate for new task",
            "Slug": "suggest-corrected-estimate-for-new-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the system has detected I underestimate writing tasks by 40%",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I create a new task tagged \"writing\" with estimated time of 2 hours",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the system should suggest a corrected estimate of approximately 2 hours 48 minutes",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the suggestion should explain \"Based on your history, writing tasks typically take 40% longer than estimated\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to accept or dismiss the suggestion",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "User accepts corrected estimate",
            "Slug": "user-accepts-corrected-estimate",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the system suggests a corrected estimate of 2 hours 48 minutes",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I accept the corrected estimate",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task estimated time should be updated to 2 hours 48 minutes",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "User accepts corrected estimate but completes in original time",
            "Slug": "user-accepts-corrected-estimate-but-completes-in-original-time",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the system has detected I underestimate writing tasks by 40%",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I accepted a corrected estimate of 2 hours 48 minutes for a writing task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete the task and record actual time as 2 hours",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the system should record this as a data point where the original estimate was more accurate",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the estimation model should reduce the bias correction factor for this category",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the model should not over-correct based on a single instance",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "User dismisses corrected estimate",
            "Slug": "user-dismisses-corrected-estimate",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the system suggests a corrected estimate of 2 hours 48 minutes",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I dismiss the suggestion",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task estimated time should remain at 2 hours",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the system should respect my choice without repeating the suggestion for this specific task instance",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View estimation accuracy dashboard",
            "Slug": "view-estimation-accuracy-dashboard",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to my estimation insights",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see my overall estimation accuracy percentage",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see estimation bias broken down by task category",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see a trend line showing accuracy improvement over time",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Estimation accuracy improves over time",
            "Slug": "estimation-accuracy-improves-over-time",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have been using corrected estimates for 8 weeks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view my estimation accuracy trend",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my recent estimation variance should be lower than my initial variance",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "an insight card should celebrate the improvement",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            },
            {
              "Keyword": "And",
              "NativeKeyword": "And ",
              "Name": "I have a premium subscription",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@intelligence",
          "@estimation",
          "@premium"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "monetisation/subscription-tiers.feature",
      "Feature": {
        "Name": "Subscription Tiers",
        "Description": "As a Waypoint user\nI want clear free and premium tiers\nSo that I can use the app effectively for free and upgrade when I need more",
        "FeatureElements": [
          {
            "Examples": [],
            "Name": "Free-tier user has access to core features",
            "Slug": "free-tier-user-has-access-to-core-features",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a free-tier account",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should have access to the following features:",
                "TableArgument": {
                  "HeaderRow": [
                    "Feature"
                  ],
                  "DataRows": [
                    [
                      "Unlimited tasks"
                    ],
                    [
                      "Unlimited quests"
                    ],
                    [
                      "Unlimited epics"
                    ],
                    [
                      "Full XP and levelling engine"
                    ],
                    [
                      "Skill trees"
                    ],
                    [
                      "Titles and ranks"
                    ],
                    [
                      "Basic daily brief"
                    ],
                    [
                      "Energy-aware scheduling"
                    ],
                    [
                      "One accountability partner"
                    ],
                    [
                      "Basic weekly review"
                    ],
                    [
                      "Journey timeline"
                    ],
                    [
                      "Local data storage"
                    ],
                    [
                      "Manual data export"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Free-tier user encounters a premium feature",
            "Slug": "free-tier-user-encounters-a-premium-feature",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a free-tier account",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I attempt to access a premium feature such as \"Sagas\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a tasteful upgrade prompt explaining the feature",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to dismiss the prompt",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the prompt should not interfere with my current workflow",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the same feature prompt should not appear more than once per week",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Subscribe to premium",
            "Slug": "subscribe-to-premium",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a free-tier account",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to the subscription page",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I choose the \"Waypoint Pro\" plan",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I complete the payment process",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my account should be upgraded to premium",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "all premium features should become immediately available",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Premium user has access to all premium features",
            "Slug": "premium-user-has-access-to-all-premium-features",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a premium subscription",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should have access to the following additional features:",
                "TableArgument": {
                  "HeaderRow": [
                    "Feature"
                  ],
                  "DataRows": [
                    [
                      "Sagas and long-arc goal tracking"
                    ],
                    [
                      "Capacity modelling"
                    ],
                    [
                      "Time estimation learning"
                    ],
                    [
                      "Insight cards"
                    ],
                    [
                      "Guilds (create and join up to 5)"
                    ],
                    [
                      "Challenge mode"
                    ],
                    [
                      "Seasonal leaderboards"
                    ],
                    [
                      "Cross-device sync"
                    ],
                    [
                      "Priority themes and cosmetics"
                    ],
                    [
                      "Advanced weekly review"
                    ],
                    [
                      "Annual Wrapped"
                    ],
                    [
                      "Calendar integration"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Premium subscription expires",
            "Slug": "premium-subscription-expires",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a premium subscription that has expired",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my account should revert to free-tier access",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should retain all data created during the premium period",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "premium-only data should be read-only but not deleted",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "sagas should be viewable but not editable",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "guild memberships should be preserved but limited to view-only",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be prompted to renew with a clear explanation of what was lost",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "In-progress guild activities on premium expiry",
            "Slug": "in-progress-guild-activities-on-premium-expiry",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a premium subscription",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I am participating in a guild challenge",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have in-progress shared quests",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "my premium subscription expires",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my challenge participation should end gracefully",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my contributions to shared quests should remain",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "But",
                "NativeKeyword": "But ",
                "Name": "I should not be able to create new guild activities",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to view but not interact with guild boards",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Cosmetics retained after downgrade",
            "Slug": "cosmetics-retained-after-downgrade",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a premium subscription",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have purchased the \"Midnight Theme\" colour palette",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "my premium subscription expires",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should retain the \"Midnight Theme\" in my collection",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should still be able to use purchased cosmetics",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "no purchased cosmetic should be removed or locked",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Subscribe to team tier",
            "Slug": "subscribe-to-team-tier",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am the administrator of a team workspace",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I subscribe to the \"Waypoint Guild\" plan for up to 25 members",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should be able to invite team members to the team workspace",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "all team members should receive premium features",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "team-specific features should be available",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Team tier includes team-specific features",
            "Slug": "team-tier-includes-team-specific-features",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my team has the \"Waypoint Guild\" subscription",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the team should have access to:",
                "TableArgument": {
                  "HeaderRow": [
                    "Feature"
                  ],
                  "DataRows": [
                    [
                      "Everything in Pro"
                    ],
                    [
                      "Shared quest boards with roles"
                    ],
                    [
                      "Team analytics and velocity tracking"
                    ],
                    [
                      "Admin controls and onboarding flows"
                    ],
                    [
                      "Dedicated team leaderboards"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Team lead cancels the subscription",
            "Slug": "team-lead-cancels-the-subscription",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my team has the \"Waypoint Guild\" subscription with 10 members",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the team subscription is cancelled",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "all team members should revert to free-tier access",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "team members should retain all data created during the team subscription",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "team-specific features should become read-only",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each member should be notified of the change",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Team member is removed from the team",
            "Slug": "team-member-is-removed-from-the-team",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my team has the \"Waypoint Guild\" subscription",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "\"Jordan\" is a team member",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I remove \"Jordan\" from the team",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "\"Jordan\" should revert to free-tier access",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "\"Jordan\" should retain a copy of their personal data",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "\"Jordan\" should lose access to shared team quest boards",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Downgrade from Team to Pro",
            "Slug": "downgrade-from-team-to-pro",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my team has the \"Waypoint Guild\" subscription",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I downgrade to the \"Waypoint Pro\" plan",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my personal account should become a Pro account",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "all team members should revert to free-tier access",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "team-specific features should become read-only",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "all members should be notified of the downgrade",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Purchase a cosmetic item",
            "Slug": "purchase-a-cosmetic-item",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am viewing the cosmetics shop",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I purchase the \"Midnight Theme\" colour palette",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the theme should be added to my collection",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to apply it in my settings",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the purchase should provide no XP or gameplay advantage",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Cosmetics do not affect gameplay",
            "Slug": "cosmetics-do-not-affect-gameplay",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "two users with identical task completion patterns",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "one user has purchased premium cosmetics",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "XP is calculated for both users",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "both users should receive identical XP amounts",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "no cosmetic purchase should modify XP rates or difficulty weights",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@monetisation",
          "@tiers"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "progression/experience-points.feature",
      "Feature": {
        "Name": "Experience Points",
        "Description": "As a Waypoint user\nI want to earn XP for completing tasks that reflects the genuine effort involved\nSo that my progression feels earned and honest",
        "FeatureElements": [
          {
            "Examples": [
              {
                "Name": "",
                "TableArgument": {
                  "HeaderRow": [
                    "difficulty",
                    "min_xp",
                    "max_xp"
                  ],
                  "DataRows": [
                    [
                      "Trivial",
                      "5",
                      "10",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Easy",
                      "10",
                      "20",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Normal",
                      "20",
                      "40",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Hard",
                      "40",
                      "80",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Epic",
                      "80",
                      "150",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ]
                  ]
                },
                "Tags": [],
                "NativeKeyword": "Examples"
              }
            ],
            "Name": "XP awarded based on task difficulty",
            "Slug": "xp-awarded-based-on-task-difficulty",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task with difficulty \"<difficulty>\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete the task on time",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive between <min_xp> and <max_xp> XP",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "XP bonus for completing a task before the deadline",
            "Slug": "xp-bonus-for-completing-a-task-before-the-deadline",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task due in 3 days with difficulty \"Normal\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete the task 2 days before the deadline",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive the base XP for the difficulty",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive an early completion bonus",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Reduced XP for completing a task after the deadline",
            "Slug": "reduced-xp-for-completing-a-task-after-the-deadline",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task that was due yesterday with difficulty \"Normal\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete the task 1 day late",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive reduced XP compared to on-time completion",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the XP should still be greater than zero",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "XP is never negative",
            "Slug": "xp-is-never-negative",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task that is 30 days overdue",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete the task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive a minimum positive XP amount",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "no XP should be deducted from my total",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Consistency multiplier for daily streaks",
            "Slug": "consistency-multiplier-for-daily-streaks",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have completed at least one task each day for 7 consecutive days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete a task today",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive the base XP for the task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive a streak consistency multiplier bonus",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the multiplier should be displayed in the XP breakdown",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "XP correctly attributed to parent quest",
            "Slug": "xp-correctly-attributed-to-parent-quest",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a quest \"Sprint work\" containing the task \"Fix login bug\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete the task \"Fix login bug\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the XP should be counted toward my total",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the XP should also be reflected in the quest's XP tally",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View XP breakdown after task completion",
            "Slug": "view-xp-breakdown-after-task-completion",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have just completed a task \"Write unit tests\" with difficulty \"Hard\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the XP award details",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a breakdown showing:",
                "TableArgument": {
                  "HeaderRow": [
                    "Component"
                  ],
                  "DataRows": [
                    [
                      "Base XP"
                    ],
                    [
                      "Early completion"
                    ],
                    [
                      "Streak bonus"
                    ],
                    [
                      "Total"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each component should display its calculated value",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the total should equal the sum of all components",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View cumulative XP on profile",
            "Slug": "view-cumulative-xp-on-profile",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view my profile",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see my total lifetime XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see my current level",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the XP required for the next level",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see a progress bar toward the next level",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View XP history over time",
            "Slug": "view-xp-history-over-time",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to my XP history",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a chart showing XP earned per day over the last 30 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the total XP earned this week",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the total XP earned this season",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Detect burst of trivial task creation",
            "Slug": "detect-burst-of-trivial-task-creation",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I create 20 tasks in 5 minutes with no descriptions or due dates",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I immediately complete all 20 tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the XP awarded should be at the reduced trivial-task rate",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive a gentle notification explaining the adjustment",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Repeated trivial tasks earn diminishing returns",
            "Slug": "repeated-trivial-tasks-earn-diminishing-returns",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have completed 10 tasks with difficulty \"Trivial\" today",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete an 11th trivial task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the XP awarded should be less than the first trivial task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the diminishing rate should be visible in the XP breakdown",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Difficulty rating auto-adjusts for repeated identical tasks",
            "Slug": "difficulty-rating-auto-adjusts-for-repeated-identical-tasks",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a recurring task \"Check email\" rated as \"Normal\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have consistently completed it in under 2 minutes",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the task details",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a suggestion to adjust the difficulty to \"Easy\" or \"Trivial\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see an explanation of why the adjustment is recommended",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "XP awarded with default difficulty when none is set",
            "Slug": "xp-awarded-with-default-difficulty-when-none-is-set",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a task with no difficulty set",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete the task on time",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task should be treated as \"Normal\" difficulty for XP purposes",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive the corresponding XP for \"Normal\" difficulty",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see a prompt suggesting I set a difficulty for more accurate XP",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "XP for recurring task completions",
            "Slug": "xp-for-recurring-task-completions",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a recurring task \"Morning standup\" with difficulty \"Easy\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have completed this recurring task 5 times this week",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete it again",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive the base XP for \"Easy\" difficulty",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the XP should reflect any applicable diminishing returns for repeated tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@progression",
          "@xp"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "progression/levelling.feature",
      "Feature": {
        "Name": "Levelling System",
        "Description": "As a Waypoint user\nI want to level up as I accumulate XP\nSo that I have a clear sense of long-term progression",
        "FeatureElements": [
          {
            "Examples": [],
            "Name": "New user starts at level 1",
            "Slug": "new-user-starts-at-level-1",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have just created my account",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my level should be 1",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my XP should be 0",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the XP required for level 2 should be displayed",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Level up when XP threshold is reached",
            "Slug": "level-up-when-xp-threshold-is-reached",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am level 3 with 280 XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the XP threshold for level 4 is 300",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I earn 25 XP from completing a task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my level should change to 4",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "a level-up celebration should be displayed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "a level-up event should appear on my journey timeline",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the excess 5 XP should carry over toward level 5",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "XP requirements scale logarithmically",
            "Slug": "xp-requirements-scale-logarithmically",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the XP thresholds for levels should follow a logarithmic curve:",
                "TableArgument": {
                  "HeaderRow": [
                    "Level",
                    "Cumulative XP Required"
                  ],
                  "DataRows": [
                    [
                      "2",
                      "50"
                    ],
                    [
                      "5",
                      "300"
                    ],
                    [
                      "10",
                      "1,000"
                    ],
                    [
                      "20",
                      "4,000"
                    ],
                    [
                      "50",
                      "25,000"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "level 5 should be reachable by completing 10 Normal tasks per day for 5 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "level 50 should require at least 30 days of sustained high-difficulty completions",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Level up unlocks new features progressively",
            "Slug": "level-up-unlocks-new-features-progressively",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am level 2",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I reach level 3",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should unlock the \"Skill Trees\" feature",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive a tutorial prompt for the new feature",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the feature should be accessible from that point forward",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [
              {
                "Name": "",
                "TableArgument": {
                  "HeaderRow": [
                    "level",
                    "feature"
                  ],
                  "DataRows": [
                    [
                      "1",
                      "Tasks, Quests, Basic XP",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "3",
                      "Skill Trees",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "5",
                      "Titles, Daily Brief",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "7",
                      "Accountability Partners",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "10",
                      "Leaderboards, Challenge Mode",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "15",
                      "Insight Cards",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "20",
                      "Advanced Analytics",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ]
                  ]
                },
                "Tags": [],
                "NativeKeyword": "Examples"
              }
            ],
            "Name": "Progressive feature unlocks by level",
            "Slug": "progressive-feature-unlocks-by-level",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I reach level <level>",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should unlock \"<feature>\"",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View level progress on dashboard",
            "Slug": "view-level-progress-on-dashboard",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am level 7 with 850 XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the threshold for level 8 is 1,000 XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view my dashboard",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see \"Level 7\" prominently displayed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see a progress bar showing 85% toward level 8",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see \"150 XP to next level\"",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Level badge displayed on profile",
            "Slug": "level-badge-displayed-on-profile",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am level 12",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "another user views my profile",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "they should see my level badge showing \"Level 12\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the badge style should reflect my level tier",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Level milestones are celebrated",
            "Slug": "level-milestones-are-celebrated",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am about to reach level 10",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I earn enough XP to reach level 10",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see an enhanced celebration animation",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive a milestone achievement",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the milestone should be shareable",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "User reaches maximum level",
            "Slug": "user-reaches-maximum-level",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am at the maximum level",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I earn additional XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my level should remain at the maximum",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the XP should still be tracked as lifetime XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see a \"Max Level\" badge on my profile",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should still earn seasonal XP and rewards",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Existing users retain levels when XP thresholds are rebalanced",
            "Slug": "existing-users-retain-levels-when-xp-thresholds-are-rebalanced",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am level 15 with 3,500 XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the XP thresholds have been rebalanced",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view my profile",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my level should reflect the new thresholds applied to my existing XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should never lose levels due to a rebalance",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@progression",
          "@levels"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "progression/seasons.feature",
      "Feature": {
        "Name": "Seasons",
        "Description": "As a Waypoint user\nI want quarterly seasons with themed challenges and refreshed leaderboards\nSo that long-term engagement stays fresh without invalidating my permanent progress",
        "FeatureElements": [
          {
            "Examples": [],
            "Name": "New season begins",
            "Slug": "new-season-begins",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the current season \"Season of the Architect\" is ending",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the new season \"Season of the Explorer\" begins",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see an announcement for the new season",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the seasonal leaderboard should reset to zero",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my permanent level and XP should remain unchanged",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the new season's themed challenges should be available",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the new season's cosmetics should be previewed",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View current season details",
            "Slug": "view-current-season-details",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to the seasons view",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see the current season name and theme",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the number of days remaining in the season",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the seasonal quest line with progress",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the seasonal leaderboard",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the seasonal cosmetics I can earn",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View past season history",
            "Slug": "view-past-season-history",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have participated in 3 previous seasons",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to past seasons",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a summary of each past season",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each summary should show my final rank",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each summary should show the cosmetics I earned",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each summary should show the seasonal XP I accumulated",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Start the seasonal quest line",
            "Slug": "start-the-seasonal-quest-line",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "a new season has begun with a quest line of 8 stages",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the seasonal quest line",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see stage 1 as available",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "stages 2-8 should be locked",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each stage should preview its challenge theme",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [
              {
                "Name": "",
                "TableArgument": {
                  "HeaderRow": [
                    "stage",
                    "required",
                    "min_difficulty",
                    "completed",
                    "next_stage"
                  ],
                  "DataRows": [
                    [
                      "1",
                      "3",
                      "Easy",
                      "2",
                      "2",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "3",
                      "5",
                      "Hard",
                      "4",
                      "4",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "5",
                      "7",
                      "Normal",
                      "6",
                      "6",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ]
                  ]
                },
                "Tags": [],
                "NativeKeyword": "Examples"
              }
            ],
            "Name": "Complete a seasonal quest line stage",
            "Slug": "complete-a-seasonal-quest-line-stage",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am on stage <stage> of the seasonal quest line",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "stage <stage> requires completing <required> tasks rated \"<min_difficulty>\" or above",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have completed <completed> qualifying tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete another qualifying task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "stage <stage> should be marked as complete",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive seasonal XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "stage <next_stage> should become available",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should earn the stage <stage> cosmetic reward",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Complete the full seasonal quest line",
            "Slug": "complete-the-full-seasonal-quest-line",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have completed stages 1 through 7 of the seasonal quest line",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete stage 8",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive a seasonal completion bonus",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should earn the exclusive season-completion cosmetic",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "a seasonal completion badge should appear on my profile",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View seasonal leaderboard",
            "Slug": "view-seasonal-leaderboard",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the current season is 6 weeks in",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the seasonal leaderboard",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see my rank among my cohort",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see my seasonal XP total",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the top 10 users in my cohort",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my cohort should consist of users within 5 levels of my current level",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@premium",
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Season ends and final ranks are recorded",
            "Slug": "season-ends-and-final-ranks-are-recorded",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the current season is ending",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my seasonal rank is 15th in my cohort",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the season concludes",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my final rank should be permanently recorded",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive a rank-based seasonal reward",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the leaderboard should become read-only for the past season",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Earn a seasonal cosmetic",
            "Slug": "earn-a-seasonal-cosmetic",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the current season offers a \"Crystal Compass\" profile badge",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the badge requires completing the seasonal quest line stage 5",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete stage 5",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the \"Crystal Compass\" badge should be added to my collection",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it should be marked as a seasonal exclusive",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Seasonal cosmetic unavailable after season ends",
            "Slug": "seasonal-cosmetic-unavailable-after-season-ends",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the \"Season of the Architect\" offered the \"Blueprint Frame\" avatar border",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I did not earn it during that season",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the \"Season of the Architect\" ends",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the \"Blueprint Frame\" should no longer be earnable",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it should appear as a locked seasonal item in my collection history",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Display seasonal cosmetic on profile",
            "Slug": "display-seasonal-cosmetic-on-profile",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have earned the \"Crystal Compass\" badge",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I select it as my active profile badge",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "it should be displayed on my profile",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "other users should see it marked with its season of origin",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "User joins mid-season",
            "Slug": "user-joins-mid-season",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the current season is 6 weeks in with 7 weeks remaining",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have just created my account",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the seasonal quest line",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see all stages available from stage 1",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to progress through the quest line normally",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the seasonal leaderboard should include me with 0 seasonal XP",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "User inactive for an entire season",
            "Slug": "user-inactive-for-an-entire-season",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I did not log in during the \"Season of the Architect\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the next season begins",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my permanent level and XP should be unchanged",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should have no record for the missed season in my season history",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to participate fully in the new season",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Seamless transition between seasons",
            "Slug": "seamless-transition-between-seasons",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the current season ends today",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the next season begins",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the new season should be immediately available with no downtime",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "any incomplete seasonal quest line stages should be locked",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the previous season's final leaderboard should be viewable in past seasons",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@progression",
          "@seasons"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "progression/skill-trees.feature",
      "Feature": {
        "Name": "Skill Trees",
        "Description": "As a Waypoint user\nI want to unlock and progress through skill trees based on my behaviour patterns\nSo that my productivity identity is reflected and rewarded",
        "FeatureElements": [
          {
            "Examples": [
              {
                "Name": "",
                "TableArgument": {
                  "HeaderRow": [
                    "category",
                    "threshold",
                    "tree_name"
                  ],
                  "DataRows": [
                    [
                      "creative",
                      "15",
                      "Creator",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "health",
                      "15",
                      "Guardian",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "fitness",
                      "15",
                      "Guardian",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "learning",
                      "15",
                      "Scholar",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "study",
                      "15",
                      "Scholar",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "work",
                      "20",
                      "Architect",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "career",
                      "20",
                      "Architect",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "social",
                      "15",
                      "Connector",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "people",
                      "15",
                      "Connector",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "home",
                      "15",
                      "Steward",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "organising",
                      "15",
                      "Steward",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "side-project",
                      "10",
                      "Builder",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ]
                  ]
                },
                "Tags": [],
                "NativeKeyword": "Examples"
              }
            ],
            "Name": "Skill tree unlocked by behaviour pattern",
            "Slug": "skill-tree-unlocked-by-behaviour-pattern",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have consistently completed tasks tagged or categorised as \"<category>\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have completed at least <threshold> such tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the system evaluates my behaviour patterns",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the \"<tree_name>\" skill tree should be unlocked",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive a notification about the discovery",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the skill tree should appear in my progression view",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View available and locked skill trees",
            "Slug": "view-available-and-locked-skill-trees",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to the skill tree view",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see my unlocked skill trees with progress",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see locked skill trees as silhouettes",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each locked tree should show a hint about how to unlock it",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "User does not see skill trees before level 3",
            "Slug": "user-does-not-see-skill-trees-before-level-3",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am an authenticated user",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I am at level 2",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to the progression view",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should not see the skill trees section",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see a teaser message about unlocking skill trees at level 3",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Progress within a skill tree",
            "Slug": "progress-within-a-skill-tree",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have unlocked the \"Builder\" skill tree at tier 1",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the \"Builder\" tree requires 30 side-project tasks for tier 2",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have completed 25 side-project tasks since unlocking",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete 5 more side-project tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my \"Builder\" skill tree should advance to tier 2",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive a tier-up bonus XP award",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should unlock the tier 2 perks",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View skill tree details",
            "Slug": "view-skill-tree-details",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have the \"Scholar\" skill tree at tier 2",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the \"Scholar\" skill tree details",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see my current tier and progress toward tier 3",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the perks unlocked at each tier",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see personalised study tips based on my patterns",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see a history of qualifying task completions",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Multiple skill trees can be active simultaneously",
            "Slug": "multiple-skill-trees-can-be-active-simultaneously",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have unlocked the \"Creator\" and \"Builder\" skill trees",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete a task tagged \"creative\" and \"side-project\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "progress should be applied to both the \"Creator\" and \"Builder\" trees",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the XP earned should only count once for my total",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Tier 1 perk unlocks personalised tips",
            "Slug": "tier-1-perk-unlocks-personalised-tips",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have just unlocked the \"Guardian\" skill tree at tier 1",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive a set of health and fitness productivity tips",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the tips should be accessible from my skill tree view",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Tier 2 perk unlocks suggested workflows",
            "Slug": "tier-2-perk-unlocks-suggested-workflows",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have reached tier 2 of the \"Architect\" skill tree",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive suggested quest templates for work projects",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the templates should be usable when creating new quests",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Tier 3 perk unlocks cosmetic rewards",
            "Slug": "tier-3-perk-unlocks-cosmetic-rewards",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have reached tier 3 of the \"Creator\" skill tree",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should unlock a unique profile badge for the \"Creator\" tree",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should unlock a themed colour palette for my interface",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "these cosmetics should be selectable in my profile settings",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Skill tree progress retained after inactivity",
            "Slug": "skill-tree-progress-retained-after-inactivity",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have the \"Builder\" skill tree at tier 2",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have not completed any side-project tasks in 60 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the \"Builder\" skill tree details",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my tier should still be 2",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my progress toward tier 3 should be unchanged",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see a gentle prompt encouraging me to pick up side-project tasks again",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            },
            {
              "Keyword": "And",
              "NativeKeyword": "And ",
              "Name": "I have reached at least level 3",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@progression",
          "@skill-trees"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "progression/streaks.feature",
      "Feature": {
        "Name": "Streaks and Grace Days",
        "Description": "As a Waypoint user\nI want my streaks to be celebrated without punishing inevitable off-days\nSo that I stay motivated by consistency without developing streak anxiety",
        "FeatureElements": [
          {
            "Examples": [],
            "Name": "Streak increments on daily completion",
            "Slug": "streak-increments-on-daily-completion",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my current streak is 5 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have not completed any tasks today",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete a task today",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my streak should increment to 6 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "only the first completion should trigger the increment",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Streak persists through multiple completions",
            "Slug": "streak-persists-through-multiple-completions",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my current streak is 10 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have already completed 3 tasks today",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete a 4th task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my streak should remain at 10 days (already counted today)",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Streak milestone celebration",
            "Slug": "streak-milestone-celebration",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my current streak is 6 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete a task and my streak reaches 7 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a streak milestone celebration",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "\"7-day streak\" should appear on my journey timeline",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [
              {
                "Name": "",
                "TableArgument": {
                  "HeaderRow": [
                    "previous_days",
                    "streak_days",
                    "label"
                  ],
                  "DataRows": [
                    [
                      "6",
                      "7",
                      "One Week",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "13",
                      "14",
                      "Two Weeks",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "29",
                      "30",
                      "One Month",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "59",
                      "60",
                      "Two Months",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "99",
                      "100",
                      "The Century",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "364",
                      "365",
                      "The Full Year",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ]
                  ]
                },
                "Tags": [],
                "NativeKeyword": "Examples"
              }
            ],
            "Name": "Streak milestones are celebrated at key thresholds",
            "Slug": "streak-milestones-are-celebrated-at-key-thresholds",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my current streak is <previous_days> days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete a task and my streak reaches <streak_days> days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a milestone celebration for \"<label>\"",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Grace day preserves streak on a missed day",
            "Slug": "grace-day-preserves-streak-on-a-missed-day",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my current streak is 15 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have 1 grace day available",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I complete no tasks today",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the day ends",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my streak should remain at 15 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "1 grace day should be consumed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be notified that a grace day was used",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Grace day not consumed on an active day",
            "Slug": "grace-day-not-consumed-on-an-active-day",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my current streak is 15 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have 2 grace days available",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I complete 3 tasks today",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the day ends",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my streak should be 16 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should still have 2 grace days available",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Grace days accumulate over time",
            "Slug": "grace-days-accumulate-over-time",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have 0 grace days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I complete my weekly review this week",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should earn 1 grace day",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I can hold a maximum of 3 grace days at once",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Streak broken when no grace days available",
            "Slug": "streak-broken-when-no-grace-days-available",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my current streak is 20 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have 0 grace days available",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I complete no tasks today",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the day ends",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my streak should reset to 0",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see an encouraging restart message mentioning my previous 20-day streak",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see my previous streak of 20 days recorded in my history",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "No negative consequences for broken streak",
            "Slug": "no-negative-consequences-for-broken-streak",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my streak just reset from 20 to 0",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "no XP should be deducted",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "no titles should be revoked",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "no skill tree progress should be lost",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my past streak should remain on my journey timeline as an achievement",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Activate a streak freeze",
            "Slug": "activate-a-streak-freeze",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my current streak is 30 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I am going on holiday for 5 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I activate a streak freeze for 5 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my streak should be frozen at 30 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the 5 frozen days should not count against my streak",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should not receive task reminders during the freeze",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Streak freeze has a maximum duration",
            "Slug": "streak-freeze-has-a-maximum-duration",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I attempt to freeze my streak for 15 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a message that the maximum freeze duration is 7 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be offered to set a 7-day freeze instead",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Streak resumes after freeze ends",
            "Slug": "streak-resumes-after-freeze-ends",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my streak is frozen at 30 days for 5 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the freeze period ends",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I complete a task the next day",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my streak should continue from 31 days",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Streak day boundary respects user timezone",
            "Slug": "streak-day-boundary-respects-user-timezone",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my timezone is set to \"Australia/Sydney\" (UTC+11)",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my current streak is 5 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it is 11:30 PM in my timezone",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete a task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "it should count toward today's streak in my timezone",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my streak should remain at 5 days if I already completed a task today",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Completing tasks during a streak freeze does not end the freeze early",
            "Slug": "completing-tasks-during-a-streak-freeze-does-not-end-the-freeze-early",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my streak is frozen at 30 days for 5 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I am on day 2 of the freeze",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete a task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the freeze should remain active for the remaining 3 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the task completion should be recorded normally",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the streak should remain frozen at 30 days",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@progression",
          "@streaks"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "progression/titles-and-ranks.feature",
      "Feature": {
        "Name": "Titles and Ranks",
        "Description": "As a Waypoint user\nI want to earn titles through sustained behaviour patterns\nSo that my productivity identity is recognised and visible to others",
        "FeatureElements": [
          {
            "Examples": [
              {
                "Name": "",
                "TableArgument": {
                  "HeaderRow": [
                    "title",
                    "requirement_summary"
                  ],
                  "DataRows": [
                    [
                      "Early Bird",
                      "Completed 50+ tasks before 9 AM over at least 4 weeks",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Morning Architect",
                      "Completed complex tasks before noon consistently for 6 weeks",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Night Owl",
                      "Completed 50+ tasks after 9 PM over at least 4 weeks",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Marathon Builder",
                      "Daily progress on a single saga for 60+ consecutive days",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Boss Slayer",
                      "Completed 10+ Boss Tasks",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Streak Master",
                      "Maintained a 30-day task completion streak",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Quest Closer",
                      "Completed 25+ quests",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Consistent Planner",
                      "Completed 12+ weekly reviews",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Team Anchor",
                      "Contributed to guild quests every week for 8+ weeks",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ]
                  ]
                },
                "Tags": [],
                "NativeKeyword": "Examples"
              }
            ],
            "Name": "Earn a title through sustained behaviour",
            "Slug": "earn-a-title-through-sustained-behaviour",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have met the sustained requirement for the title \"<title>\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I check my title progress",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should be awarded the title \"<title>\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "a title-earned event should appear on my journey timeline",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the title should be visible on my profile",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Title requires sustained behaviour, not bursts",
            "Slug": "title-requires-sustained-behaviour-not-bursts",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I completed 50 tasks before 9 AM",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "But",
                "NativeKeyword": "But ",
                "Name": "they were all completed within a single week",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I check my title progress for \"Early Bird\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should not be awarded the title \"Early Bird\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the system should show progress toward the sustained requirement",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Title progress is visible before earning",
            "Slug": "title-progress-is-visible-before-earning",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am working toward the \"Streak Master\" title",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I need a 30-day streak and I am currently at 18 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view my title progress",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see \"Streak Master\" with a progress indicator of 60%",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see \"12 more days of consistent completions needed\"",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Select an active title",
            "Slug": "select-an-active-title",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have earned the titles \"Early Bird\" and \"Boss Slayer\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I select \"Boss Slayer\" as my active title",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "\"Boss Slayer\" should appear next to my name on my profile",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "\"Boss Slayer\" should appear on leaderboards and guild views",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View all earned titles",
            "Slug": "view-all-earned-titles",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have earned 5 titles",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to my titles collection",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see all 5 earned titles with their earn dates",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see locked titles with their requirements",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to select any earned title as active",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Title visible on profile to other users",
            "Slug": "title-visible-on-profile-to-other-users",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have \"Morning Architect\" as my active title",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "another user views my profile",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "they should see \"Morning Architect\" displayed under my name",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Title visible in guild member list",
            "Slug": "title-visible-in-guild-member-list",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have \"Morning Architect\" as my active title",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "another user views a guild member list that includes me",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "they should see my title next to my name",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Title retained after behaviour change",
            "Slug": "title-retained-after-behaviour-change",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I earned the title \"Early Bird\" through consistent morning completions",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have not completed a task before 9 AM in the last 3 weeks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view my titles",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "\"Early Bird\" should still be in my earned titles",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it should remain selectable as my active title",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Active title displayed when user holds many titles",
            "Slug": "active-title-displayed-when-user-holds-many-titles",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have earned 8 titles",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have selected \"Boss Slayer\" as my active title",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "another user views my profile",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "they should see \"Boss Slayer\" as my displayed title",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "they should see a count indicating I have earned 8 titles total",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            },
            {
              "Keyword": "And",
              "NativeKeyword": "And ",
              "Name": "I have reached at least level 5",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@progression",
          "@titles"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "reflection/annual-wrapped.feature",
      "Feature": {
        "Name": "Annual Wrapped",
        "Description": "As a Waypoint user\nI want an annual summary of my productivity journey\nSo that I can celebrate my year and share my accomplishments",
        "FeatureElements": [
          {
            "Examples": [],
            "Name": "Annual wrapped is generated",
            "Slug": "annual-wrapped-is-generated",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "it is December 15th or later in the current year",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "my annual wrapped is generated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a multi-slide summary including:",
                "TableArgument": {
                  "HeaderRow": [
                    "Slide",
                    "Content"
                  ],
                  "DataRows": [
                    [
                      "Total tasks completed",
                      "Count for the year"
                    ],
                    [
                      "Total XP earned",
                      "Sum of all XP this year"
                    ],
                    [
                      "Levels gained",
                      "Start level to end level"
                    ],
                    [
                      "Longest streak",
                      "Maximum consecutive day streak"
                    ],
                    [
                      "Quests completed",
                      "Total quest count"
                    ],
                    [
                      "Hardest Boss Task",
                      "The highest-difficulty Boss Task completed"
                    ],
                    [
                      "Most productive month",
                      "Month with highest task completion"
                    ],
                    [
                      "Skill tree growth",
                      "Trees unlocked and tiers advanced"
                    ],
                    [
                      "Titles earned",
                      "New titles earned this year"
                    ],
                    [
                      "Top insight",
                      "Most impactful insight card of the year"
                    ],
                    [
                      "Seasons participated in",
                      "Seasonal ranks and achievements"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Wrapped not available with insufficient data",
            "Slug": "wrapped-not-available-with-insufficient-data",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I signed up in November and have only 6 weeks of data",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the wrapped period arrives",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a message that my wrapped will be available next year",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see a teaser of what wrapped will include",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Slides with zero data show encouraging messaging",
            "Slug": "slides-with-zero-data-show-encouraging-messaging",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "it is December 15th or later in the current year",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have not completed any quests this year",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "my annual wrapped is generated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the \"Quests completed\" slide should not be hidden",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it should display an encouraging message such as \"No quests yet — your first quest awaits next year!\"",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Mid-year signup users receive a partial wrapped",
            "Slug": "mid-year-signup-users-receive-a-partial-wrapped",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I signed up in June and have at least 3 months of data",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the wrapped period arrives",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive a \"Year So Far\" wrapped summary",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it should cover only the months since my signup",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it should clearly indicate the partial time period",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View wrapped as an interactive slideshow",
            "Slug": "view-wrapped-as-an-interactive-slideshow",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I open my annual wrapped",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a slide-by-slide interactive presentation",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each slide should display the data point prominently with a celebratory visual treatment",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to navigate forward and backward through slides",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Share wrapped highlights",
            "Slug": "share-wrapped-highlights",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am viewing my annual wrapped",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I choose to share a slide",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should be able to generate a shareable image of that slide",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the image should include Waypoint branding",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to share it as an image to any platform via the system share sheet",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View past year's wrapped",
            "Slug": "view-past-years-wrapped",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a wrapped summary from last year",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to my wrapped history",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see last year's wrapped available for replay",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to compare year-over-year statistics",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "User can exclude specific data from shareable wrapped",
            "Slug": "user-can-exclude-specific-data-from-shareable-wrapped",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am viewing my annual wrapped",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I choose to share a slide",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see privacy options to exclude specific data points from the shareable image",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the generated image should omit any data I chose to exclude",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the excluded data should still be visible in my private wrapped view",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            },
            {
              "Keyword": "And",
              "NativeKeyword": "And ",
              "Name": "I have a premium subscription",
              "StepComments": [],
              "AfterLastStepComments": []
            },
            {
              "Keyword": "And",
              "NativeKeyword": "And ",
              "Name": "I have used Waypoint for at least 3 months in the current year",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@reflection",
          "@wrapped",
          "@premium"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "reflection/insight-cards.feature",
      "Feature": {
        "Name": "Insight Cards",
        "Description": "As a Waypoint user\nI want to receive personalised productivity observations\nSo that I discover patterns about myself I would not have noticed on my own",
        "FeatureElements": [
          {
            "Examples": [
              {
                "Name": "",
                "TableArgument": {
                  "HeaderRow": [
                    "pattern",
                    "message"
                  ],
                  "DataRows": [
                    [
                      "High creative task completion on Tuesdays",
                      "You are 3x more likely to complete creative tasks on Tuesday mornings.",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Quest completion time improving",
                      "Your average quest completion time has improved by 22% this season.",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Consistent weekly reviews",
                      "You have completed every weekly review for 8 weeks. That puts you in the top 5%.",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Morning productivity peak",
                      "Your most productive hours are 9 AM to 11 AM. You complete 40% of daily tasks then.",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Estimation accuracy improving",
                      "Your time estimates are now within 15% of actual. That is up from 40% last month.",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Side project consistency",
                      "You have worked on your side project 5 out of 7 days for 3 weeks straight.",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ]
                  ]
                },
                "Tags": [],
                "NativeKeyword": "Examples"
              }
            ],
            "Name": "System generates an insight card",
            "Slug": "system-generates-an-insight-card",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the system has detected the pattern \"<pattern>\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "an insight card is generated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a card with the message \"<message>\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the card should include supporting data or a visual trend (e.g., a visual trend of my Tuesday completion rates)",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Insight cards are delivered periodically",
            "Slug": "insight-cards-are-delivered-periodically",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I meet the criteria for multiple insights",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "insights are generated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive a maximum of 1 insight card per day and 2-3 per week",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the most impactful insights should be prioritised",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "No insight card when insufficient data",
            "Slug": "no-insight-card-when-insufficient-data",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have only 7 days of task history",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the system evaluates potential insights",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "no insight cards should be generated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should not see the insights section until enough data is available",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View an insight card",
            "Slug": "view-an-insight-card",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an unread insight card",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I open the insights section",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see the insight card with its message and data",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to mark it as read",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Save an insight card",
            "Slug": "save-an-insight-card",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an insight card about my morning productivity",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I save the card to my collection",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "it should appear in my saved insights",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to reference it later",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Dismiss an insight card",
            "Slug": "dismiss-an-insight-card",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an insight card I find irrelevant",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I dismiss the card",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the card should be removed from my active insights",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the system should learn from my dismissal to adjust future insight relevance",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Dismissed insight type reduces future frequency",
            "Slug": "dismissed-insight-type-reduces-future-frequency",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have dismissed 3 insight cards related to \"morning productivity\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the system evaluates future insights",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the frequency of morning-related insights should be reduced",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the system should prioritise other insight categories instead",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Insight must be validated against user data before delivery",
            "Slug": "insight-must-be-validated-against-user-data-before-delivery",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the system has detected the pattern \"Morning productivity peak\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "But",
                "NativeKeyword": "But ",
                "Name": "my task history shows I complete fewer than 10% of tasks before noon",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the system evaluates the insight for delivery",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the insight should not be generated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the system should only surface patterns consistent with my actual data",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Same insight type does not repeat within a quarter",
            "Slug": "same-insight-type-does-not-repeat-within-a-quarter",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I received an insight about \"quest completion time improving\" on January 15",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the system evaluates insights on February 20",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the system should not generate another \"quest completion time improving\" insight",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the same insight type should not appear more than once per quarter",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Insight cards appear in weekly review",
            "Slug": "insight-cards-appear-in-weekly-review",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have received 2 insight cards this week",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete my weekly review",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the review should include a section highlighting this week's insights",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            },
            {
              "Keyword": "And",
              "NativeKeyword": "And ",
              "Name": "I have a premium subscription",
              "StepComments": [],
              "AfterLastStepComments": []
            },
            {
              "Keyword": "And",
              "NativeKeyword": "And ",
              "Name": "I have reached at least level 15",
              "StepComments": [],
              "AfterLastStepComments": []
            },
            {
              "Keyword": "And",
              "NativeKeyword": "And ",
              "Name": "I have at least 30 days of task completion history",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@reflection",
          "@insights",
          "@premium"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "reflection/journey-timeline.feature",
      "Feature": {
        "Name": "Journey Timeline",
        "Description": "As a Waypoint user\nI want a visual timeline of my accomplishments and milestones\nSo that I can look back on my progress and feel motivated by how far I have come",
        "FeatureElements": [
          {
            "Examples": [
              {
                "Name": "",
                "TableArgument": {
                  "HeaderRow": [
                    "event_type"
                  ],
                  "DataRows": [
                    [
                      "Level up",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Quest completed",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Epic completed",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Saga completed",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Boss Task defeated",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Title earned",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Skill tree unlocked",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Skill tree tier advanced",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Streak milestone (7, 30, 100)",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Seasonal quest line completed",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Guild joined",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Guild quest completed",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Challenge won",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Weekly review streak milestone",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ]
                  ]
                },
                "Tags": [],
                "NativeKeyword": "Examples"
              }
            ],
            "Name": "Event types appear on the timeline",
            "Slug": "event-types-appear-on-the-timeline",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have triggered a \"<event_type>\" event",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "a \"<event_type>\" entry should appear on my journey timeline",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "it should include the date and relevant details",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Timeline displays events chronologically",
            "Slug": "timeline-displays-events-chronologically",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have 20 events on my journey timeline",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the timeline",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "events should be displayed in reverse chronological order",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each event should show its date and type",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to scroll through my full history",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Timeline groups events by month",
            "Slug": "timeline-groups-events-by-month",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have events spanning 6 months",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the timeline",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "events should be grouped by month",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each month should show a summary count of events",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Timeline displays year headers when events span multiple years",
            "Slug": "timeline-displays-year-headers-when-events-span-multiple-years",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have events spanning from November 2025 to March 2026",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the timeline",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "events should be grouped by month under a year header",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see a \"2026\" header above January 2026 events",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see a \"2025\" header above November 2025 events",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Filter timeline by event type",
            "Slug": "filter-timeline-by-event-type",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a mix of level-up, quest, and title events",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I filter the timeline by \"Quest completed\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should only see quest completion events",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Add a personal note to a timeline event",
            "Slug": "add-a-personal-note-to-a-timeline-event",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a \"Quest completed\" event for \"Prepare conference talk\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I add a note \"First ever conference talk - terrifying but worth it!\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the note should be saved with the timeline event",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the note should be visible when I view the event",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Personal notes persist when filtering by event type",
            "Slug": "personal-notes-persist-when-filtering-by-event-type",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a \"Quest completed\" event with the note \"My best quest yet!\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have a \"Level up\" event with no note",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I filter the timeline by \"Quest completed\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see the \"Quest completed\" event",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the note \"My best quest yet!\" should be visible on the event",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View timeline event details",
            "Slug": "view-timeline-event-details",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a \"Level up\" event for reaching level 10",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I tap on the event",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see the date and time of the level up",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the XP that triggered it",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see what features were unlocked at that level",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "New user has an empty timeline with encouragement",
            "Slug": "new-user-has-an-empty-timeline-with-encouragement",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am a new user with no timeline events",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the timeline",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see an encouraging message about building my journey",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see what kinds of events will appear",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Long-term user scrolling back through months of progress",
            "Slug": "long-term-user-scrolling-back-through-months-of-progress",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have been using Waypoint for 8 months",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have 50+ timeline events",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I scroll through my timeline",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should be able to see the full arc of my productivity journey",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "visual density should increase as I became more active",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Timeline loads incrementally for users with many events",
            "Slug": "timeline-loads-incrementally-for-users-with-many-events",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have more than 100 events on my journey timeline",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the timeline",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see the most recent 20 events loaded initially",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "as I scroll down, the next batch of events should load incrementally",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "a loading indicator should appear while fetching more events",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Timeline events display in the user's local timezone",
            "Slug": "timeline-events-display-in-the-users-local-timezone",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am in the \"America/New_York\" timezone",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have a \"Level up\" event that occurred at \"2026-01-15T03:00:00Z\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the timeline",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the event should display the date and time as \"January 14, 2026 at 10:00 PM\"",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@reflection",
          "@timeline"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "reflection/weekly-review.feature",
      "Feature": {
        "Name": "Weekly Review Ritual",
        "Description": "As a Waypoint user\nI want a guided weekly review that surfaces insights about my productivity\nSo that I build a habit of reflection and continuous improvement",
        "FeatureElements": [
          {
            "Examples": [],
            "Name": "Weekly review prompt at scheduled time",
            "Slug": "weekly-review-prompt-at-scheduled-time",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have configured my weekly review for Sunday at 7 PM",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "it is Sunday at 7 PM",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive a notification prompting me to start my weekly review",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the notification should indicate the estimated time of 5 minutes",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Configure weekly review schedule",
            "Slug": "configure-weekly-review-schedule",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to my review settings",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I set my weekly review to \"Saturday at 10 AM\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "future review prompts should arrive Saturday at 10 AM",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Start review manually at any time",
            "Slug": "start-review-manually-at-any-time",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to the weekly review section",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I choose to start a review now",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the review flow should begin regardless of scheduled time",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Default review schedule when no preference is set",
            "Slug": "default-review-schedule-when-no-preference-is-set",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have not configured a weekly review schedule",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my weekly review should default to Sunday at 6 PM in my local timezone",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive a notification at the default time",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Dismiss weekly review prompt",
            "Slug": "dismiss-weekly-review-prompt",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I receive the weekly review notification",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I dismiss the notification",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the review should remain available in the review section",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive one follow-up reminder 24 hours later in my local timezone",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "no further reminders should be sent for this week's review",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Complete a basic weekly review",
            "Slug": "complete-a-basic-weekly-review",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am a free-tier user",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I start the weekly review",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a summary of the week:",
                "TableArgument": {
                  "HeaderRow": [
                    "Metric",
                    "Example"
                  ],
                  "DataRows": [
                    [
                      "Tasks completed",
                      "24"
                    ],
                    [
                      "Tasks created",
                      "30"
                    ],
                    [
                      "Quests completed",
                      "2"
                    ],
                    [
                      "Current streak",
                      "11 days"
                    ],
                    [
                      "XP earned",
                      "420"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be prompted with \"What went well this week?\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I enter my reflection text for \"What went well this week?\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be prompted with \"What could go better next week?\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I enter my reflection text for \"What could go better next week?\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the review should be saved",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive weekly review XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the review should appear in my review history",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View past weekly reviews",
            "Slug": "view-past-weekly-reviews",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have completed 6 weekly reviews",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to my review history",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see all 6 reviews in reverse chronological order",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each review should show the week's summary metrics",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each review should show my reflection notes",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Complete an advanced weekly review",
            "Slug": "complete-an-advanced-weekly-review",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a premium subscription",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I start the weekly review",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see the basic summary metrics",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see a productivity chart comparing this week to the last 4 weeks showing completed tasks, XP earned, and streak status",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see my most productive day and time window",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see tasks I avoided or rescheduled repeatedly",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see estimation accuracy for the week",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see quest progress updates",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be prompted for reflection questions",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete all reflection prompts",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the review should be saved with all data and reflections",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@premium",
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Review surfaces patterns across weeks",
            "Slug": "review-surfaces-patterns-across-weeks",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have completed 8 weekly reviews",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I start this week's review",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see trend analysis such as:",
                "TableArgument": {
                  "HeaderRow": [
                    "Insight"
                  ],
                  "DataRows": [
                    [
                      "Your Tuesday productivity has increased 30% over the last month"
                    ],
                    [
                      "You complete more creative tasks in the morning"
                    ],
                    [
                      "Your estimation accuracy has improved from 55% to 72%"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Earn XP for completing weekly review",
            "Slug": "earn-xp-for-completing-weekly-review",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete my weekly review",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive weekly review XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my review streak should increment",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Review streak builds over weeks",
            "Slug": "review-streak-builds-over-weeks",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have completed weekly reviews for 11 consecutive weeks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete this week's review",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my review streak should be 12 weeks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be notified of my progress toward the \"Consistent Planner\" title",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Missed review does not break streak harshly",
            "Slug": "missed-review-does-not-break-streak-harshly",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a review streak of 8 weeks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I miss one week's review",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my streak should be paused",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should have a 1-week grace period to complete the missed review",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "if I complete next week's review, my streak should continue from 8",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Complete two missed weeks during the grace period",
            "Slug": "complete-two-missed-weeks-during-the-grace-period",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a review streak of 5 weeks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I missed the last two weeks' reviews",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I am within the 1-week grace period",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete both the missed week's review and the current week's review",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "both reviews should be saved and counted",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my review streak should continue from 7 weeks",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Progress is saved as draft when user logs out mid-review",
            "Slug": "progress-is-saved-as-draft-when-user-logs-out-mid-review",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have started my weekly review",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have entered reflection text for \"What went well this week?\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I log out before completing the review",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my in-progress review should be saved as a draft",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "when I log back in and navigate to the weekly review section",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see the option to resume my draft review",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my previously entered reflection text should be preserved",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@reflection",
          "@weekly-review"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "social/accountability-partners.feature",
      "Feature": {
        "Name": "Accountability Partners",
        "Description": "As a Waypoint user\nI want to pair with another person for mutual progress visibility\nSo that we keep each other motivated through shared accountability",
        "FeatureElements": [
          {
            "Examples": [],
            "Name": "Send an accountability partner request",
            "Slug": "send-an-accountability-partner-request",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I send an accountability partner request to user \"Jordan\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "\"Jordan\" should receive a partner request notification",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the request should be in a \"Pending\" state",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Accept a partner request",
            "Slug": "accept-a-partner-request",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a pending partner request from \"Casey\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I accept the request",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "\"Casey\" and I should be linked as accountability partners",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "we should both see each other's daily summary",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Decline a partner request",
            "Slug": "decline-a-partner-request",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a pending partner request from \"Casey\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I decline the request",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the request should be removed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "\"Casey\" should be notified that the request was declined",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Only one active partner at a time",
            "Slug": "only-one-active-partner-at-a-time",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I already have an accountability partner \"Jordan\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I attempt to send a partner request to \"Alex\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a message that I already have an active partner",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be offered the option to end my current partnership first",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "End an accountability partnership",
            "Slug": "end-an-accountability-partnership",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an accountability partner \"Jordan\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I choose to end the partnership",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the partnership should be dissolved",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "\"Jordan\" should be notified",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "both our past shared summaries should remain in our individual histories",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View partner's daily summary",
            "Slug": "view-partners-daily-summary",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an accountability partner \"Jordan\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view my partner's daily summary",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see Jordan's task completion count for today",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see Jordan's current streak status",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see Jordan's active quest count",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should not see individual task titles or descriptions",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Partner sees my summary",
            "Slug": "partner-sees-my-summary",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an accountability partner \"Jordan\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have completed 5 tasks today and my streak is at 12 days",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "\"Jordan\" views my daily summary",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "they should see \"5 tasks completed today\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "they should see \"12-day streak\"",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Send a check-in message to partner",
            "Slug": "send-a-check-in-message-to-partner",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an accountability partner \"Jordan\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I send a check-in message \"Great streak, keep it going!\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "\"Jordan\" should receive the message in their partner view",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the message should appear in our shared message history",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Partner check-in messages are limited scope",
            "Slug": "partner-check-in-messages-are-limited-scope",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an accountability partner \"Jordan\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the messaging interface",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should only be able to send encouragement messages up to 280 characters",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the messaging should not function as a full chat system",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Partner account is deactivated",
            "Slug": "partner-account-is-deactivated",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an accountability partner \"Jordan\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "\"Jordan\" deactivates their account",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the partnership should be automatically dissolved",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be notified that my partner is no longer available",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to send a new partner request to someone else",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Re-pair with a former partner",
            "Slug": "re-pair-with-a-former-partner",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I previously had a partnership with \"Jordan\" that was ended",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I send a new accountability partner request to \"Jordan\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the request should be sent successfully",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "our previous shared history should remain separate from the new partnership",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Existing partnership persists regardless of level changes",
            "Slug": "existing-partnership-persists-regardless-of-level-changes",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have an accountability partner \"Jordan\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I was level 7 when the partnership was formed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "my level calculation is adjusted and I am now below level 7",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the existing partnership should remain active",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should not be able to form new partnerships until I return to level 7",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            },
            {
              "Keyword": "And",
              "NativeKeyword": "And ",
              "Name": "I have reached at least level 7",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@social",
          "@accountability"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "social/challenge-mode.feature",
      "Feature": {
        "Name": "Challenge Mode",
        "Description": "As a Waypoint user\nI want to participate in time-limited competitions\nSo that I have extra motivation through friendly competition",
        "FeatureElements": [
          {
            "Examples": [],
            "Name": "View available challenges",
            "Slug": "view-available-challenges",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to the challenges section",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see currently active global challenges",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see guild-specific challenges if I belong to a guild",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each challenge should show its duration, rules, and reward",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Join a global challenge",
            "Slug": "join-a-global-challenge",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "there is an active challenge \"Weekend Warrior: Complete the most tasks this weekend\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the challenge runs from Saturday 00:00 to Sunday 23:59",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I opt into the challenge",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should be registered as a participant",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my task completions during the window should count toward the challenge",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see a challenge progress tracker",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Create a guild challenge",
            "Slug": "create-a-guild-challenge",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am a member of \"Side Project Squad\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I create a guild challenge with the following details:",
                "TableArgument": {
                  "HeaderRow": [
                    "Field",
                    "Value"
                  ],
                  "DataRows": [
                    [
                      "Title",
                      "Boss Rush: Clear all Boss Tasks before Friday"
                    ],
                    [
                      "Duration",
                      "Monday to Friday"
                    ],
                    [
                      "Objective",
                      "Complete the most Boss Tasks"
                    ],
                    [
                      "Reward",
                      "Seasonal cosmetic + bonus XP"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "all guild members should receive an invitation to participate",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the challenge should appear on the guild board",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Track challenge progress",
            "Slug": "track-challenge-progress",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am participating in \"Weekend Warrior\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have completed 8 tasks so far",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the challenge progress",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see my task count of 8",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see my current rank among participants",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the top 5 participants and their counts",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Challenge ends and results are announced",
            "Slug": "challenge-ends-and-results-are-announced",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the \"Weekend Warrior\" challenge period has ended",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I completed 12 tasks, ranking 3rd overall",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the challenge concludes",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should receive a notification with my final rank",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the top 3 participants should receive seasonal cosmetics",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "all participants should receive participation XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "results should be visible in the challenge history",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Challenge does not penalise non-participation",
            "Slug": "challenge-does-not-penalise-non-participation",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "there is an active challenge \"Weekend Warrior\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I choose not to participate",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should not see any penalty or negative indicator",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my regular task completions should not be affected",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to view challenge results as a spectator",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Trivial task spam during a challenge",
            "Slug": "trivial-task-spam-during-a-challenge",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am participating in a \"most tasks completed\" challenge",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I create and immediately complete 30 trivial tasks in 10 minutes",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the system evaluates my challenge activity",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "only tasks meeting a minimum difficulty threshold should count",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "a notification should explain the quality requirement",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Tasks completed during the challenge window count regardless of creation date",
            "Slug": "tasks-completed-during-the-challenge-window-count-regardless-of-creation-date",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the challenge \"Weekend Warrior\" starts on Saturday",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I created 5 tasks on Friday but complete them on Saturday",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the tasks are evaluated for the challenge",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "all 5 tasks should count because they were completed during the challenge window",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "But",
                "NativeKeyword": "But ",
                "Name": "tasks completed before Saturday should not count",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Minimum difficulty threshold for challenge tasks",
            "Slug": "minimum-difficulty-threshold-for-challenge-tasks",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am participating in a \"most tasks completed\" challenge",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the system evaluates a task for challenge eligibility",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task must have been open for at least 5 minutes before completion",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the task must have a title of at least 10 characters",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "trivial or duplicate tasks should be excluded from the challenge count",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Withdraw from a challenge after joining",
            "Slug": "withdraw-from-a-challenge-after-joining",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am participating in \"Weekend Warrior\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the challenge is still active",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I choose to withdraw from the challenge",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should no longer be a participant",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my progress should be removed from the leaderboard",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should not receive any challenge rewards",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Global challenges are system-generated",
            "Slug": "global-challenges-are-system-generated",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the system generates a new global challenge",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the challenge should appear in the challenges section for all eligible users",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "no individual user should be able to create global challenges",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Guild challenges can be created by any guild member",
            "Slug": "guild-challenges-can-be-created-by-any-guild-member",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am a member of \"Side Project Squad\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I am not the guild leader",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I create a guild challenge",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the challenge should be created successfully",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "all guild members should receive an invitation to participate",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Tie resolution in challenge rankings",
            "Slug": "tie-resolution-in-challenge-rankings",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the \"Weekend Warrior\" challenge has ended",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "two participants both completed 15 tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the final rankings are determined",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the participant who reached 15 tasks first should rank higher",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "both tied participants should receive the same tier of reward",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            },
            {
              "Keyword": "And",
              "NativeKeyword": "And ",
              "Name": "I have a premium subscription",
              "StepComments": [],
              "AfterLastStepComments": []
            },
            {
              "Keyword": "And",
              "NativeKeyword": "And ",
              "Name": "I have reached at least level 10",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@social",
          "@challenges",
          "@premium"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "social/guilds.feature",
      "Feature": {
        "Name": "Guilds",
        "Description": "As a Waypoint user\nI want to form or join small groups with shared quest boards\nSo that my team and I can maintain shared accountability and momentum",
        "FeatureElements": [
          {
            "Examples": [],
            "Name": "Create a guild",
            "Slug": "create-a-guild",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I create a guild with the following details:",
                "TableArgument": {
                  "HeaderRow": [
                    "Field",
                    "Value"
                  ],
                  "DataRows": [
                    [
                      "Name",
                      "Side Project Squad"
                    ],
                    [
                      "Description",
                      "Accountability for builders"
                    ],
                    [
                      "Type",
                      "Private"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the guild \"Side Project Squad\" should be created",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be the guild leader",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the guild should have 1 member (me)",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Generate an invite link for a guild",
            "Slug": "generate-an-invite-link-for-a-guild",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am the leader of \"Side Project Squad\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I generate an invite link for the guild",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "a shareable invite link should be created",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the link should expire after 7 days by default",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Accept a guild invite via link",
            "Slug": "accept-a-guild-invite-via-link",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "a valid invite link exists for \"Side Project Squad\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "another user clicks the invite link and accepts",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "they should be added to \"Side Project Squad\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the guild should have 2 members",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Guild reaches maximum capacity",
            "Slug": "guild-reaches-maximum-capacity",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my guild \"Side Project Squad\" has 12 members",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "a 13th user attempts to join via invite link",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "they should see a message that the guild is at capacity",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "they should not be added to the guild",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Remove a member from a guild",
            "Slug": "remove-a-member-from-a-guild",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am the leader of \"Side Project Squad\" with 5 members",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I remove the member \"Alex\" from the guild",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "\"Alex\" should no longer be a guild member",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "\"Alex\" should receive a notification about the removal",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "their contributions to guild quests should remain in history",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@done"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Leave a guild",
            "Slug": "leave-a-guild",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am a member of \"Study Group Alpha\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I am not the guild leader",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I choose to leave the guild",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should no longer be a member",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my past contributions should remain visible in guild history",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Leader leaves the guild",
            "Slug": "leader-leaves-the-guild",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am the leader of \"Side Project Squad\" with 3 members",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I choose to leave the guild",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should be prompted to transfer leadership",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I transfer leadership to \"Jordan\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "\"Jordan\" should become the new guild leader",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be removed from the guild",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Disband a guild",
            "Slug": "disband-a-guild",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am the leader of \"Side Project Squad\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I choose to disband the guild",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I confirm the disbandment",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "all members should be notified",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the guild should be archived",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "individual contributions should remain in each member's history",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "User can only lead a limited number of guilds",
            "Slug": "user-can-only-lead-a-limited-number-of-guilds",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am the leader of 3 guilds",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I attempt to create a new guild",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see a message that I have reached the maximum number of guilds I can lead",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be offered the option to disband or transfer leadership of an existing guild",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Edit guild details",
            "Slug": "edit-guild-details",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am the leader of \"Side Project Squad\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I update the guild details:",
                "TableArgument": {
                  "HeaderRow": [
                    "Field",
                    "Value"
                  ],
                  "DataRows": [
                    [
                      "Name",
                      "Side Project Champions"
                    ],
                    [
                      "Description",
                      "Shipping greatness together"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the guild details should be updated",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "all members should be notified of the changes",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Remove a member with in-progress guild quest tasks",
            "Slug": "remove-a-member-with-in-progress-guild-quest-tasks",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am the leader of \"Side Project Squad\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "\"Alex\" has 3 in-progress tasks on the guild quest \"Ship landing page\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I remove \"Alex\" from the guild",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "\"Alex\" should no longer be a guild member",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "their in-progress tasks should become unassigned on the guild quest board",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the remaining members should be notified of the unassigned tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Leader transfer is declined",
            "Slug": "leader-transfer-is-declined",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am the leader of \"Side Project Squad\" with 3 members",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I choose to leave the guild",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I attempt to transfer leadership to \"Jordan\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "\"Jordan\" declines the transfer",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should be prompted to select another member for leadership",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should remain the guild leader until the transfer is accepted",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Last non-leader member leaves the guild",
            "Slug": "last-non-leader-member-leaves-the-guild",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am the leader of \"Side Project Squad\" with 2 members",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the only other member is \"Jordan\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "\"Jordan\" leaves the guild",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should be the sole remaining member",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the guild should remain active",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be prompted to invite new members or disband",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Create a guild quest",
            "Slug": "create-a-guild-quest",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am a member of \"Side Project Squad\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I create a guild quest with the following details:",
                "TableArgument": {
                  "HeaderRow": [
                    "Field",
                    "Value"
                  ],
                  "DataRows": [
                    [
                      "Title",
                      "Ship landing page"
                    ],
                    [
                      "Description",
                      "Get the marketing site live"
                    ],
                    [
                      "Due Date",
                      "2026-05-01"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I add tasks and assign them to guild members:",
                "TableArgument": {
                  "HeaderRow": [
                    "Task",
                    "Assignee"
                  ],
                  "DataRows": [
                    [
                      "Write copy",
                      "Me"
                    ],
                    [
                      "Design mockups",
                      "Jordan"
                    ],
                    [
                      "Implement HTML/CSS",
                      "Alex"
                    ],
                    [
                      "Deploy to hosting",
                      "Me"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the guild quest should appear on the shared quest board",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each member should see their assigned tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View guild quest board",
            "Slug": "view-guild-quest-board",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my guild has 3 active quests",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the guild quest board",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see all 3 quests with their progress",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see which tasks are assigned to which members",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the overall guild activity feed",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Complete an assigned guild task",
            "Slug": "complete-an-assigned-guild-task",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have a guild task \"Write copy\" assigned to me",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I complete the task",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task should be marked as complete on the guild quest board",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should receive XP (both personal and guild contribution)",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "guild members should see the completion in the guild feed",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Guild quest completion",
            "Slug": "guild-quest-completion",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "all tasks in the guild quest \"Ship landing page\" are complete",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the final task is completed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the guild quest should be marked as complete",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "all contributing members should receive a guild quest bonus",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the completion should appear in the guild feed with a celebration",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View guild XP and level",
            "Slug": "view-guild-xp-and-level",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view my guild's profile",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see the guild's collective XP total",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the guild level",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see each member's contribution to guild XP",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Guild levels up",
            "Slug": "guild-levels-up",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "my guild has accumulated enough collective XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the guild XP threshold for the next level is reached",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "all guild members should receive a guild level-up notification",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the guild should unlock the next tier of guild perks",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View guild activity feed",
            "Slug": "view-guild-activity-feed",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the guild feed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see recent task completions by guild members",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see quest completions and milestones",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see members' level-ups and title achievements",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to react to feed items with encouragement",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@wip"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            },
            {
              "Keyword": "And",
              "NativeKeyword": "And ",
              "Name": "I have a premium subscription",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@social",
          "@guilds",
          "@premium"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "social/leaderboards.feature",
      "Feature": {
        "Name": "Leaderboards",
        "Description": "As a Waypoint user\nI want to see how my productivity compares to similar users\nSo that healthy competition keeps me motivated without being demoralising",
        "FeatureElements": [
          {
            "Examples": [],
            "Name": "View my leaderboard cohort",
            "Slug": "view-my-leaderboard-cohort",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am level 15",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the leaderboard",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should be placed in a cohort of users within 10 levels of my current level",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see my rank within this cohort",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Leaderboard ranks by weekly XP",
            "Slug": "leaderboard-ranks-by-weekly-xp",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am in a leaderboard cohort",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the weekly leaderboard",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "users should be ranked by XP earned in the current week",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see the top 10 users in my cohort",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see my own rank even if outside the top 10",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Leaderboard resets weekly",
            "Slug": "leaderboard-resets-weekly",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "it is the start of a new week",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the leaderboard",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "all weekly XP totals should be reset to zero",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "last week's final standings should be viewable in history",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Cohort assignment when levelling up mid-week",
            "Slug": "cohort-assignment-when-levelling-up-mid-week",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am level 19 and at the top of my cohort",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I level up to 20 during the current week",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should remain in my current cohort until the weekly reset",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my new cohort should take effect at the start of the next week",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Weekly leaderboard resets at a consistent time",
            "Slug": "weekly-leaderboard-resets-at-a-consistent-time",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "it is the start of a new week",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the weekly leaderboard resets",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the reset should occur at Monday 00:00 UTC",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "all users should see the new week begin at the same moment regardless of timezone",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Level-mismatched users never appear together",
            "Slug": "level-mismatched-users-never-appear-together",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am level 12",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "there is a user at level 45 who earned 500 XP this week",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I view the leaderboard",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the level-45 user should not appear in my cohort",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [
              {
                "Name": "",
                "TableArgument": {
                  "HeaderRow": [
                    "leaderboard",
                    "ranking_metric"
                  ],
                  "DataRows": [
                    [
                      "Weekly XP",
                      "XP earned this week",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Longest Streak",
                      "current active streak length",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ],
                    [
                      "Quest Closer",
                      "quests completed this season",
                      {
                        "WasExecuted": false,
                        "WasSuccessful": false,
                        "WasProvided": true
                      }
                    ]
                  ]
                },
                "Tags": [],
                "NativeKeyword": "Examples"
              }
            ],
            "Name": "View a leaderboard by type",
            "Slug": "view-a-leaderboard-by-type",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I select the \"<leaderboard>\" leaderboard",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see cohort members ranked by <ranking_metric>",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View guild leaderboard",
            "Slug": "view-guild-leaderboard",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am a member of a guild",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I select the \"Guild\" leaderboard",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should see all guild members ranked by contribution this week",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the guild leaderboard should be separate from the global cohort leaderboard",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Opt out of leaderboards",
            "Slug": "opt-out-of-leaderboards",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I navigate to my privacy settings",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I disable leaderboard participation",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my name should not appear on any leaderboard",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should still be able to view leaderboards as a spectator",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see a placeholder for my rank position",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Anonymous leaderboard participation",
            "Slug": "anonymous-leaderboard-participation",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I enable anonymous leaderboard mode",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my profile should appear on leaderboards as \"Anonymous Questor\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "my level and XP should be visible but not my username or title",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            },
            {
              "Keyword": "And",
              "NativeKeyword": "And ",
              "Name": "I have a premium subscription",
              "StepComments": [],
              "AfterLastStepComments": []
            },
            {
              "Keyword": "And",
              "NativeKeyword": "And ",
              "Name": "I have reached at least level 10",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@social",
          "@leaderboards",
          "@premium"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    },
    {
      "RelativeFolder": "social/shared-quests.feature",
      "Feature": {
        "Name": "Shared Quests",
        "Description": "As a Waypoint user\nI want to collaborate on quests where multiple people contribute tasks\nSo that we can work toward shared goals together",
        "FeatureElements": [
          {
            "Examples": [],
            "Name": "Create a shared quest and invite participants",
            "Slug": "create-a-shared-quest-and-invite-participants",
            "Description": "",
            "Steps": [
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I create a shared quest with the following details:",
                "TableArgument": {
                  "HeaderRow": [
                    "Field",
                    "Value"
                  ],
                  "DataRows": [
                    [
                      "Title",
                      "Plan summer road trip"
                    ],
                    [
                      "Description",
                      "Organise the group road trip"
                    ],
                    [
                      "Due Date",
                      "2026-06-15"
                    ]
                  ]
                },
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I invite users \"Jordan\" and \"Alex\" to collaborate",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the shared quest should be created",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "\"Jordan\" and \"Alex\" should receive invitations",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the quest should appear in all participants' quest lists once accepted",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Add tasks to a shared quest",
            "Slug": "add-tasks-to-a-shared-quest",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am a participant in the shared quest \"Plan summer road trip\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I add a task \"Book accommodation\" and assign it to \"Jordan\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I add a task \"Create packing list\" and assign it to myself",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "both tasks should appear on the shared quest",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "each participant should see their own assigned tasks highlighted",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Any participant can add tasks",
            "Slug": "any-participant-can-add-tasks",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "\"Jordan\" is a participant in the shared quest \"Plan summer road trip\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "\"Jordan\" adds a task \"Research restaurants\" and assigns it to \"Alex\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the task should appear on the shared quest board",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "\"Alex\" should be notified of the new assignment",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "View shared quest progress",
            "Slug": "view-shared-quest-progress",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the shared quest \"Plan summer road trip\" has 6 tasks across 3 participants",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "3 tasks are completed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "any participant views the quest",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "they should see 50% progress",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "they should see a breakdown of each participant's contributions",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "they should see which tasks are completed, in progress, and pending",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Shared quest has a maximum participant limit",
            "Slug": "shared-quest-has-a-maximum-participant-limit",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I have created a shared quest \"Plan summer road trip\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the quest already has 10 participants",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I invite another user \"Sam\" to collaborate",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "\"Sam\" should not be added",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should see a message that the quest has reached its maximum of 10 participants",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Quest creator removes a participant",
            "Slug": "quest-creator-removes-a-participant",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I created the shared quest \"Plan summer road trip\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "\"Alex\" is a participant with 2 assigned tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I remove \"Alex\" from the shared quest",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "\"Alex\" should no longer be a participant",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "\"Alex\" should be notified of the removal",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "their assigned tasks should become unassigned",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "\"Alex\" should retain XP for tasks they already completed",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "All participants leave a shared quest",
            "Slug": "all-participants-leave-a-shared-quest",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the shared quest \"Plan summer road trip\" has 3 participants",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I am the quest creator",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "all other participants leave the quest",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should remain as the sole participant",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the quest should continue as a personal quest",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to invite new participants",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Quest creator has management privileges",
            "Slug": "quest-creator-has-management-privileges",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I created the shared quest \"Plan summer road trip\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "I should be able to remove participants",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should be able to edit the quest title, description, and due date",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "other participants should be able to add and complete tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "But",
                "NativeKeyword": "But ",
                "Name": "other participants should not be able to remove fellow participants",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Shared quest completed",
            "Slug": "shared-quest-completed",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "the shared quest \"Plan summer road trip\" has 6 tasks",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "5 tasks are completed by various participants",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "the final task is completed",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "the shared quest should be marked as complete",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "all participants should receive shared quest completion bonus XP",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "all participants should see the completion on their journey timeline",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          },
          {
            "Examples": [],
            "Name": "Participant leaves a shared quest",
            "Slug": "participant-leaves-a-shared-quest",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "I am a participant in \"Plan summer road trip\"",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I have 2 tasks assigned to me",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "I leave the shared quest",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "my assigned tasks should become unassigned",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "the remaining participants should be notified",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "I should retain XP for tasks I already completed",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@todo"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false,
              "WasProvided": false
            }
          }
        ],
        "Background": {
          "Examples": [],
          "Name": "",
          "Description": "",
          "Steps": [
            {
              "Keyword": "Given",
              "NativeKeyword": "Given ",
              "Name": "I am an authenticated user",
              "StepComments": [],
              "AfterLastStepComments": []
            },
            {
              "Keyword": "And",
              "NativeKeyword": "And ",
              "Name": "I have a premium subscription",
              "StepComments": [],
              "AfterLastStepComments": []
            }
          ],
          "Tags": [],
          "Result": {
            "WasExecuted": false,
            "WasSuccessful": false,
            "WasProvided": false
          }
        },
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false,
          "WasProvided": false
        },
        "Tags": [
          "@social",
          "@shared-quests",
          "@premium"
        ]
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false,
        "WasProvided": false
      }
    }
  ],
  "Summary": {
    "Tags": [
      {
        "Tag": "@core",
        "Total": 97,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 97
      },
      {
        "Tag": "@boss-tasks",
        "Total": 16,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 16
      },
      {
        "Tag": "@notifications",
        "Total": 13,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 13
      },
      {
        "Tag": "@quests",
        "Total": 22,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 22
      },
      {
        "Tag": "@recurring",
        "Total": 17,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 17
      },
      {
        "Tag": "@tasks",
        "Total": 29,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 29
      },
      {
        "Tag": "@data",
        "Total": 16,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 16
      },
      {
        "Tag": "@local-first",
        "Total": 16,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 16
      },
      {
        "Tag": "@intelligence",
        "Total": 35,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 35
      },
      {
        "Tag": "@energy",
        "Total": 11,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 11
      },
      {
        "Tag": "@daily-brief",
        "Total": 11,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 11
      },
      {
        "Tag": "@estimation",
        "Total": 13,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 13
      },
      {
        "Tag": "@premium",
        "Total": 92,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 92
      },
      {
        "Tag": "@monetisation",
        "Total": 14,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 14
      },
      {
        "Tag": "@tiers",
        "Total": 14,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 14
      },
      {
        "Tag": "@progression",
        "Total": 71,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 71
      },
      {
        "Tag": "@xp",
        "Total": 14,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 14
      },
      {
        "Tag": "@levels",
        "Total": 10,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 10
      },
      {
        "Tag": "@seasons",
        "Total": 14,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 14
      },
      {
        "Tag": "@skill-trees",
        "Total": 10,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 10
      },
      {
        "Tag": "@streaks",
        "Total": 14,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 14
      },
      {
        "Tag": "@titles",
        "Total": 9,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 9
      },
      {
        "Tag": "@reflection",
        "Total": 44,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 44
      },
      {
        "Tag": "@wrapped",
        "Total": 8,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 8
      },
      {
        "Tag": "@insights",
        "Total": 10,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 10
      },
      {
        "Tag": "@timeline",
        "Total": 12,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 12
      },
      {
        "Tag": "@weekly-review",
        "Total": 14,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 14
      },
      {
        "Tag": "@social",
        "Total": 65,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 65
      },
      {
        "Tag": "@accountability",
        "Total": 12,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 12
      },
      {
        "Tag": "@challenges",
        "Total": 13,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 13
      },
      {
        "Tag": "@guilds",
        "Total": 20,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 20
      },
      {
        "Tag": "@leaderboards",
        "Total": 10,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 10
      },
      {
        "Tag": "@shared-quests",
        "Total": 10,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 10
      },
      {
        "Tag": "@todo",
        "Total": 123,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 123
      },
      {
        "Tag": "@wip",
        "Total": 154,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 154
      },
      {
        "Tag": "@done",
        "Total": 65,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 65
      }
    ],
    "Folders": [
      {
        "Folder": "core/boss-tasks.feature",
        "Total": 16,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 16
      },
      {
        "Folder": "core/notifications.feature",
        "Total": 13,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 13
      },
      {
        "Folder": "core/quest-hierarchy.feature",
        "Total": 22,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 22
      },
      {
        "Folder": "core/recurring-tasks.feature",
        "Total": 17,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 17
      },
      {
        "Folder": "core/task-management.feature",
        "Total": 29,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 29
      },
      {
        "Folder": "data/local-first-data.feature",
        "Total": 16,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 16
      },
      {
        "Folder": "intelligence/energy-scheduling.feature",
        "Total": 11,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 11
      },
      {
        "Folder": "intelligence/daily-brief.feature",
        "Total": 11,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 11
      },
      {
        "Folder": "intelligence/time-estimation.feature",
        "Total": 13,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 13
      },
      {
        "Folder": "monetisation/subscription-tiers.feature",
        "Total": 14,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 14
      },
      {
        "Folder": "progression/experience-points.feature",
        "Total": 14,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 14
      },
      {
        "Folder": "progression/levelling.feature",
        "Total": 10,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 10
      },
      {
        "Folder": "progression/seasons.feature",
        "Total": 14,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 14
      },
      {
        "Folder": "progression/skill-trees.feature",
        "Total": 10,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 10
      },
      {
        "Folder": "progression/streaks.feature",
        "Total": 14,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 14
      },
      {
        "Folder": "progression/titles-and-ranks.feature",
        "Total": 9,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 9
      },
      {
        "Folder": "reflection/annual-wrapped.feature",
        "Total": 8,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 8
      },
      {
        "Folder": "reflection/insight-cards.feature",
        "Total": 10,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 10
      },
      {
        "Folder": "reflection/journey-timeline.feature",
        "Total": 12,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 12
      },
      {
        "Folder": "reflection/weekly-review.feature",
        "Total": 14,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 14
      },
      {
        "Folder": "social/accountability-partners.feature",
        "Total": 12,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 12
      },
      {
        "Folder": "social/challenge-mode.feature",
        "Total": 13,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 13
      },
      {
        "Folder": "social/guilds.feature",
        "Total": 20,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 20
      },
      {
        "Folder": "social/leaderboards.feature",
        "Total": 10,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 10
      },
      {
        "Folder": "social/shared-quests.feature",
        "Total": 10,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 10
      }
    ],
    "NotTestedFolders": [
      {
        "Folder": "core/boss-tasks.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "core/notifications.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "core/quest-hierarchy.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "core/recurring-tasks.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "core/task-management.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "data/local-first-data.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "intelligence/energy-scheduling.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "intelligence/daily-brief.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "intelligence/time-estimation.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "monetisation/subscription-tiers.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "progression/experience-points.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "progression/levelling.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "progression/seasons.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "progression/skill-trees.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "progression/streaks.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "progression/titles-and-ranks.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "reflection/annual-wrapped.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "reflection/insight-cards.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "reflection/journey-timeline.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "reflection/weekly-review.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "social/accountability-partners.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "social/challenge-mode.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "social/guilds.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "social/leaderboards.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      },
      {
        "Folder": "social/shared-quests.feature",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      }
    ],
    "Scenarios": {
      "Total": 342,
      "Passing": 0,
      "Failing": 0,
      "Inconclusive": 342
    },
    "Features": {
      "Total": 25,
      "Passing": 0,
      "Failing": 0,
      "Inconclusive": 25
    },
    "FoldersWithTestKinds": [
      {
        "Folder": "core/boss-tasks.feature",
        "Total": 16,
        "Automated": 16,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "core/notifications.feature",
        "Total": 13,
        "Automated": 13,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "core/quest-hierarchy.feature",
        "Total": 22,
        "Automated": 22,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "core/recurring-tasks.feature",
        "Total": 17,
        "Automated": 17,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "core/task-management.feature",
        "Total": 29,
        "Automated": 29,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "data/local-first-data.feature",
        "Total": 16,
        "Automated": 16,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "intelligence/energy-scheduling.feature",
        "Total": 11,
        "Automated": 11,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "intelligence/daily-brief.feature",
        "Total": 11,
        "Automated": 11,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "intelligence/time-estimation.feature",
        "Total": 13,
        "Automated": 13,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "monetisation/subscription-tiers.feature",
        "Total": 14,
        "Automated": 14,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "progression/experience-points.feature",
        "Total": 14,
        "Automated": 14,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "progression/levelling.feature",
        "Total": 10,
        "Automated": 10,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "progression/seasons.feature",
        "Total": 14,
        "Automated": 14,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "progression/skill-trees.feature",
        "Total": 10,
        "Automated": 10,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "progression/streaks.feature",
        "Total": 14,
        "Automated": 14,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "progression/titles-and-ranks.feature",
        "Total": 9,
        "Automated": 9,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "reflection/annual-wrapped.feature",
        "Total": 8,
        "Automated": 8,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "reflection/insight-cards.feature",
        "Total": 10,
        "Automated": 10,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "reflection/journey-timeline.feature",
        "Total": 12,
        "Automated": 12,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "reflection/weekly-review.feature",
        "Total": 14,
        "Automated": 14,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "social/accountability-partners.feature",
        "Total": 12,
        "Automated": 12,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "social/challenge-mode.feature",
        "Total": 13,
        "Automated": 13,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "social/guilds.feature",
        "Total": 20,
        "Automated": 20,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "social/leaderboards.feature",
        "Total": 10,
        "Automated": 10,
        "Manual": 0,
        "NotTested": 0
      },
      {
        "Folder": "social/shared-quests.feature",
        "Total": 10,
        "Automated": 10,
        "Manual": 0,
        "NotTested": 0
      }
    ]
  },
  "Configuration": {
    "GeneratedOn": "29 March 2026 21:07:30"
  }
});
Feature: Enabled and disabled release stages

    Scenario: Enable custom release stage
        When I run the game in the "EnabledReleaseStage" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the exception "errorClass" equals "Exception"
        And the exception "message" equals "blorb"
        And the event "unhandled" is false
        And custom metadata is included in the event
        And the stack frame methods should match:
            | Main.DoNotify()      |
            | Main.RunScenario(string scenario)  |
            | Main.Start()        |

   
Scenario: Disable custom release stage
        When I run the game in the "DisabledReleaseStage" state
        Then I should receive no errors
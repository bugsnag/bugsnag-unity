Feature: Checking the context of requests


 Scenario: Manually setting the context and then loading a scene
        When I run the game in the "CheckForManualContextAfterSceneLoad" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the "Unity Bugsnag Notifier"
        And the event "context" equals "Manually-Set"
Feature: Checking the context of requests


 Scenario: Manually setting the context and then loading a scene
        When I run the game in the "CheckForManualContextAfterSceneLoad" state
        Then I should receive a request
        And the request is a valid for the error reporting API
        And the "Bugsnag-API-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
        And the payload field "notifier.name" equals "Unity Bugsnag Notifier"
        And the payload field "events" is an array with 1 element
        And the payload field "notifier.name" equals "Unity Bugsnag Notifier"
        And the event "context" equals "Manually-Set"
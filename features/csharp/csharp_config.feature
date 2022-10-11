Feature: csharp events

  Scenario: Launch duration set to 0
    When I run the game in the "InfiniteLaunchDuration" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "InfiniteLaunchDuration"
    And the event "app.isLaunching" equals "true"

Scenario: Call mark launch complete
    When I run the game in the "MarkLaunchComplete" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "MarkLaunchComplete"
    And the event "app.isLaunching" equals "false"


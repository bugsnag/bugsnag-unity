Feature: webgl duration in foreground

  Background:
    Given I clear the Bugsnag cache

  Scenario: durationInForeground supports values larger than 32-bit int
    When I run the game in the "WebGLDurationInForeground" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "WebGLDurationInForeground"
    And the event "app.inForeground" is false
    And the event "app.durationInForeground" equals 2147483648

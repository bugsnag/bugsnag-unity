Feature: Enabled Error Types

  Scenario: Disable All Error Types
    When I run the game in the "DisableAllErrorTypes" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And I discard the oldest error
    Then I should receive no requests

  Scenario: Only Enable Log Logs
    When I run the game in the "EnableLogLogs" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "UnityLogLog"
    And I discard the oldest error
    Then I should receive no requests

  Scenario: Only Enable Unhandled Exceptions
    When I run the game in the "EnableUnhandledExceptions" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "LogException"
    And I discard the oldest error
    Then I should receive no requests

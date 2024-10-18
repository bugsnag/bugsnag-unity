Feature: csharp events

  Background:
    Given I clear the Bugsnag cache

  Scenario: Example Csharp Error
    When I run the game in the "NotifySmokeTest" state
    And I wait to receive an error

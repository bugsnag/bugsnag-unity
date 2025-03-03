Feature: Android EDM4U smoke test

  Scenario: Android EDM4U smoke test
    When On Mobile I relaunch the app
    And I wait to receive at least 1 error
    And the exception "message" equals "EDM4U"


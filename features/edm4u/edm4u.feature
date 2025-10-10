Feature: Android EDM4U smoke test

  Scenario: Android EDM4U smoke test
    And I wait to receive at least 1 error
    And the exception "message" equals "EDM4U"


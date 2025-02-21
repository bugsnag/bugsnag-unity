Feature: Android EDM4U smoke test

  Background:
    Given I clear the Bugsnag cache

  Scenario: Android EDM4U smoke test
    When I run the game in the "UncaughtExceptionSmokeTest" state
    And I wait to receive an error
    And the exception "message" equals "UncaughtExceptionSmokeTest"


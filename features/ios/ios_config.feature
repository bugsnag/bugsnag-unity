Feature: iOS Config

  Background:
    Given I clear the Bugsnag cache

  Scenario: Telemetry
    When I run the game in the "IosNativeException" state
    And I wait for 2 seconds
    And I clear any error dialogue
    And On Mobile I relaunch the app
    And I run the game in the "StartSDKDefault" state
    And I wait to receive an error
    And the error payload field "events.0.usage.config.maxBreadcrumbs" equals 50
    And the error payload field "events.0.usage.callbacks.onSession" equals 1



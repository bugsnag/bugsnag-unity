Feature: Feature Flags

    Background:
        Given I wait for the mobile game to start
        And I clear all persistent data

    Scenario: Event Callback
        When I run the "Feature Flags In Config" mobile scenario
        And I wait for 2 seconds
        And I relaunch the Unity mobile app
        When I run the "Start SDK" mobile scenario
        Then I wait to receive an error
        
        And the exception "errorClass" equals "St13runtime_error"

        And the event "featureFlags.0.featureFlag" equals "testName2"
        And the event "featureFlags.0.variant" equals "testVarient2"
        And the event "featureFlags.1" is null
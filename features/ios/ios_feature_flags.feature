Feature: Feature Flags

    Background:
        Given I wait for the mobile game to start
        And I clear all persistent data

    Scenario: Feature Flag in config
        When I run the "Feature Flags In Config" mobile scenario
        And I wait for 2 seconds
        And I relaunch the Unity mobile app
        When I run the "Start SDK" mobile scenario
        Then I wait to receive an error
        
        And the exception "errorClass" equals "St13runtime_error"

        And the event "featureFlags.0.featureFlag" equals "testName2"
        And the event "featureFlags.0.variant" equals "testVariant2"
        And the event "featureFlags.1" is null

    Scenario: Feature Flag after init
        When I run the "Feature Flags After Init" mobile scenario
        And I wait for 2 seconds
        And I relaunch the Unity mobile app
        When I run the "Start SDK" mobile scenario
        Then I wait to receive an error
        
        And the exception "errorClass" equals "St13runtime_error"

        And the event "featureFlags.0.featureFlag" equals "testName2"
        And the event "featureFlags.0.variant" equals "testVariant2"
        And the event "featureFlags.1" is null

   Scenario: Feature Flag After Init Clear All
        When I run the "Feature Flags After Init Clear All" mobile scenario
        And I wait for 2 seconds

        And I relaunch the Unity mobile app
        When I run the "Start SDK" mobile scenario
        Then I wait to receive an error
        
        And the exception "errorClass" equals "St13runtime_error"

        And the event "featureFlags.0" is null
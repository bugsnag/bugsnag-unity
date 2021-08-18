Feature: Release stages

    Background:
        Given I wait for the mobile game to start
        And I clear all persistent data

    Scenario: Release stage enabled
        When I run the "Enabled Release Stage" mobile scenario
        And I wait for 8 seconds
        And I relaunch the Unity mobile app
        When I run the "Start SDK" mobile scenario
        Then I wait to receive 1 error
        And the event "app.releaseStage" equals "test"

    Scenario: Release stage Disabled
        When I run the "Disabled Release Stage" mobile scenario
        And I wait for 8 seconds
        And I relaunch the Unity mobile app
        When I run the "Start SDK" mobile scenario
        Then I should receive no errors


Feature: Enabled Error Types

    Background:
        Given I wait for the mobile game to start
        And I clear all persistent data

    Scenario: Disable Native Errors
        When I run the "Disable Native Errors" mobile scenario
        And I wait for 2 seconds
        And I relaunch the Unity mobile app
        When I run the "Start SDK no errors" mobile scenario
        Then I should receive no errors



Feature: Disgard Native Error Class

    Background:
        Given I wait for the mobile game to start
        And I clear all persistent data

    Scenario: Disable Native Errors
        When I run the "Disgard Error Class" mobile scenario
        And I wait for 2 seconds
        And I relaunch the Unity mobile app
        When I run the "Start SDK no errors" mobile scenario
        Then I should receive no errors



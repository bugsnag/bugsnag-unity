Feature: android breadcrumbs

    Background:
        Given I wait for the mobile game to start

    Scenario: Disable Unhandled Exceptions
        When I run the "Disable Uncaught Exceptions" mobile scenario
        And I wait for 8 seconds
        And I relaunch the Unity mobile app
        When I run the "Start SDK" mobile scenario
        Then I should receive no requests



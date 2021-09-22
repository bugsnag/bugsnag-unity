Feature: Native Errors

    Background:
        Given I wait for the mobile game to start
        And I clear all persistent data

    Scenario: Discard Native Errors
        When I run the "Discard Error Class" mobile scenario
        And I wait for 2 seconds
        And I relaunch the Unity mobile app
        When I run the "Start SDK no errors" mobile scenario
        Then I should receive no errors

    Scenario: Native Error With Threads
        When I run the "iOS Native Error" mobile scenario
        And I wait for 2 seconds
        And I relaunch the Unity mobile app
        When I run the "Start SDK" mobile scenario
        Then I wait to receive an error
        And the error payload field "events.0.threads" is a non-empty array

    Scenario: Native Error Without Threads
        When I run the "iOS Native Error No Threads" mobile scenario
        And I wait for 2 seconds
        And I relaunch the Unity mobile app
        When I run the "Start SDK" mobile scenario
        Then I wait to receive an error
        And the error payload field "events.0.threads.0" is null
        And the error payload field "notifier.dependencies.0.name" equals "iOS Bugsnag Notifier"

    Scenario: Last Run Info
        When I run the "iOS Native Error" mobile scenario
        And I wait for 2 seconds
        And I relaunch the Unity mobile app
        When I run the "Check Last Run Info" mobile scenario
        Then I wait to receive 2 errors
        And I discard the oldest error
        And the exception "message" equals "Last Run Info Correct"
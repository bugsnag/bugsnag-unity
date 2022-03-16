Feature: Android custom config setting

    Background:
        Given I wait for the mobile game to start

    Scenario: Android Persistence Directory
        When I run the "Android Persistence Directory" mobile scenario
        Then I wait to receive an error
        And the exception "message" equals "Directory Found"


     Scenario: Custom App Type
        When I run the "Custom App Type" mobile scenario
        Then I wait to receive an error
        And the event "app.type" equals "test"

     Scenario: Max Reported Threads
        When I run the "Max Reported Threads" mobile scenario
        And I wait for 8 seconds
        And I relaunch the Unity mobile app
        When I run the "Start SDK" mobile scenario
        Then I wait to receive an error
        And the error payload field "events.0.threads" is an array with 3 elements

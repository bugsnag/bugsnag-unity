Feature: Android user operations tests

    Background:
        Given I wait for the mobile game to start


    Scenario: Set User In Config Csharp error
        When I run the "Set User In Config Csharp error" mobile scenario
        Then I wait to receive an error
        And the exception "message" equals "You threw an exception!"
        # User
        And the event "user.id" equals "1"
        And the event "user.email" equals "2"
        And the event "user.name" equals "3"

    Scenario: Set User In Config Native Crash
        When I run the "Set User In Config Native Crash" mobile scenario
        And I wait for 8 seconds
        And I relaunch the Unity mobile app
        When I run the "Start SDK" mobile scenario
        Then I wait to receive an error

        # Exception details
        And the exception "errorClass" equals "java.lang.RuntimeException"
        And the exception "message" equals "Uncaught JVM exception from background thread"
       
        # User
        And the event "user.id" equals "1"
        And the event "user.email" equals "2"
        And the event "user.name" equals "3"


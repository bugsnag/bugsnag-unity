Feature: Android event persistence tests

    Background:
        Given I wait for the mobile game to start

    Scenario: Get Persisted event
        When I run the "Persist" mobile scenario
        And I wait for 4 seconds
        When I clear any error dialogue
        And I relaunch the Unity mobile app
        When I run the "throw Exception" mobile scenario
        Then I wait to receive 2 errors

        # Exception details
        And the exception "message" equals "You threw an exception!"

        And I discard the oldest error

        And the exception "message" equals "Persisted Exception"

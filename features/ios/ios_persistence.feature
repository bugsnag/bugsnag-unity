Feature: iOS event persistence tests

    Background:
        Given I wait for the mobile game to start
        And I clear all persistent data

    Scenario: Get Persisted event
        When I run the "Persist" mobile scenario
        Then I wait to receive a session
        And I close and relaunch the Unity mobile app
        When I run the "Persist Report" mobile scenario
        Then I wait to receive 2 errors

        # Exception details
        And the exception "message" equals "You threw an exception!"

        And I discard the oldest error

        And the exception "message" equals "Persisted Exception"

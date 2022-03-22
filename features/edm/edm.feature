Feature: Android EDM smoke test

    Background:
        Given I wait for the mobile game to start

    Scenario: Android EDM Smoke test
        When I wait to receive an error

        # Exception details
        And the error payload field "events" is an array with 1 elements
        And the exception "message" equals "EDM"

Feature: android breadcrumbs

    Background:
        Given I wait for the mobile game to start

    Scenario: Disable Unhandled Exceptions
        When I run the "Disable Uncaught Exceptions" mobile scenario
        And I wait to receive an error
        And I discard the oldest error
        Then I should receive no requests



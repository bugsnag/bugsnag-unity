Feature: ios breadcrumbs

    Background:
        Given I wait for the game to start

    Scenario: Disable Breadcrumbs
        When I tap the "Disable Breadcrumbs" button
        Then I wait to receive an error
        And the error payload field "events.0.breadcrumbs.0" is null       


    Scenario: Max Breadcrumbs
        When I tap the "Max Breadcrumbs" button
        Then I wait to receive an error
        And the error payload field "events.0.breadcrumbs" is an array with 5 elements

    Scenario: Bugsnag Loaded Breadcrumb
        When I tap the "throw Exception" button
        Then I wait to receive an error
        And the event has a "state" breadcrumb named "Bugsnag loaded"
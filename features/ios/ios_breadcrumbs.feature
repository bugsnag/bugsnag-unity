Feature: ios breadcrumbs

    Background:
        Given I clear all persistent data
        And I wait for the mobile game to start

    Scenario: Disable Breadcrumbs
        When I run the "Disable Breadcrumbs" mobile scenario
        Then I wait to receive an error
        And the error payload field "events.0.breadcrumbs.0" is null

    Scenario: Max Breadcrumbs
        When I run the "Max Breadcrumbs" mobile scenario
        Then I wait to receive an error
        And the error payload field "events.0.breadcrumbs" is an array with 5 elements

    Scenario: Bugsnag Loaded Breadcrumb
        When I run the "throw Exception" mobile scenario
        Then I wait to receive an error
        And the event has a "state" breadcrumb named "Bugsnag loaded"

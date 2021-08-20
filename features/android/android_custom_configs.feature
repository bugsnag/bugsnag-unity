Feature: Android custom config setting

    Background:
        Given I wait for the mobile game to start

    Scenario: Android Persistence Directory
        When I run the "Android Persistence Directory" mobile scenario
        Then I wait to receive an error

     Scenario: Custom App Type
        When I run the "Custom App Type" mobile scenario
        Then I wait to receive an error
        And the event "app.type" equals "test"

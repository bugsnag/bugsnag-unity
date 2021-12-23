Feature: Android smoke tests for Metadata

    Background:
        Given I wait for the mobile game to start
        And I clear all persistent data

    Scenario: Clear Metadata
        When I run the "Clear Metadata" mobile scenario
        Then I wait to receive an error
       
        # MetaData
        And the event "metaData.test.test1" equals "test1"
        And the event "metaData.test.test2" is null
        And the event "metaData.test3.test3" equals "test3"
        And the event "metaData.test4" is null

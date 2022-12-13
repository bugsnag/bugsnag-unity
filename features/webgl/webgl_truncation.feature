Feature: webgl truncation

  Background:
    Given I clear the Bugsnag cache

  Scenario: Max String Value Length
    When I run the game in the "WebGLTruncation" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "WebGLTruncation"
    And the event "metaData.m1" is null
    And the event "metaData.m50" is null
    And the event "metaData.m99" is null

    And the event "metaData.app" is not null
    And the event "metaData.device" is not null
    And the event "user.id" is not null

    And the event "breadcrumbs.0.name" equals "Removed, along with 100 older breadcrumbs, to reduce payload size"


Feature: Metadata

  Scenario: Metadata in config
    When I run the game in the "MetadataInConfig" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "MetadataInConfig"
    And the event "metaData.string.testKey" is null
    And the event "metaData.toClear" is null
    And the event "metaData.string.testKey2" equals "testValue2"
    And the event "metaData.numberArray.testKey.1" equals 2
    And the event "metaData.stringArray.testKey.1" equals "2"
    And the event "metaData.dictionary.foo" equals "bar"
    And the event "metaData.number.testKey" equals 123

  Scenario: Metadata after start
    When I run the game in the "MetadataAfterStart" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "MetadataAfterStart"
    And the event "metaData.string.testKey" is null
    And the event "metaData.toClear" is null
    And the event "metaData.string.testKey2" equals "testValue2"
    And the event "metaData.numberArray.testKey.1" equals 2
    And the event "metaData.stringArray.testKey.1" equals "2"
    And the event "metaData.dictionary.foo" equals "bar"
    And the event "metaData.number.testKey" equals 123

  Scenario: Metadata in callback
    When I run the game in the "MetadataInCallback" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "MetadataInCallback"
    And the event "metaData.string.testKey" is null
    And the event "metaData.toClear" is null
    And the event "metaData.string.testKey2" equals "testValue2"
    And the event "metaData.numberArray.testKey.1" equals 2
    And the event "metaData.stringArray.testKey.1" equals "2"
    And the event "metaData.dictionary.foo" equals "bar"
    And the event "metaData.number.testKey" equals 123
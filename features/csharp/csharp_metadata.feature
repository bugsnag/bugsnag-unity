Feature: Metadata

  Background:
    Given I clear the Bugsnag cache

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

  # these platform specific tests are smoke tests, if os name is wrong then it's a sign that the native information has not been properly retrieved from the native layer and the unity placeholder data is being used
  @ios_only
  Scenario: iOS specific metadata
    When I run the game in the "NotifySmokeTest" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And expected device metadata is included in the event
    And expected app metadata is included in the event
    And the event "device.osName" equals "iOS"
    
  @android_only
  Scenario: Android specific metadata
    When I run the game in the "NotifySmokeTest" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And expected device metadata is included in the event
    And expected app metadata is included in the event
    And the event "device.osName" equals "android"

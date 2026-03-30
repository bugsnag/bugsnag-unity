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

  @cocoa_only
  Scenario: Large long values in metadata on Cocoa platforms
    When I run the game in the "LargeLongMetadata" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "LargeLongMetadata"
    And the event "metaData.numeric.normalInt" equals 42
    And the event "metaData.numeric.normalDouble" equals 123.456
    And the event "metaData.numeric.validLong" is not null
    And the event "metaData.numeric.negativeLong" is not null
    And the event "metaData.numeric.largeLong" equals "12345678901234567890"

  Scenario: Complex metadata sanitization with various dictionary types
    When I run the game in the "ComplexMetadataSanitization" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "ComplexMetadataSanitization"
    And the event "metaData.generic.dict.small" equals 100
    And the event "metaData.generic.dict.large" equals "18446744073709551615"
    And the event "metaData.nonGeneric.hashtable.stringKey" equals "12345678901234567890"
    And the event "metaData.nonGeneric.hashtable.normalKey" equals "valueWithStringKey"
    And the event "metaData.nonGeneric.hashtable.nested.deep" equals "18446744073709551000"
    And the event "metaData.custom.dict.custom1" equals "18446744073709551615"
    And the event "metaData.custom.dict.custom2" equals "normalValue"
    And the event "metaData.nested.level1.level2.level3.level4" equals "18446744073709551615"
    And the event "metaData.nested.level1.level2.level3.array.1" equals "12345678901234567890"
    And the event "metaData.arrays.mixedArray.0" equals "12345678901234567890"
    And the event "metaData.arrays.mixedArray.1.inArray" equals "18446744073709551615"
    And the event "metaData.edgeCases.nullValue" is null
    And the event "metaData.edgeCases.zeroUlong" equals 0
    And the event "metaData.edgeCases.maxLong" is not null
    And the event "metaData.edgeCases.overMaxLong" equals "9223372036854775808"

  Scenario: Unserializable metadata gets warning
    When I run the game in the "UnserializableMetadata" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "UnserializableMetadata"
    And the event "metaData.unserializable.__bugsnag_unserializable_values.0" is not null

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

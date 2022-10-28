Feature: Switch Specific Tests

  Scenario: SwitchCacheType set to None
    When I run the game in the "SwitchPersistEvent" state
    And I wait for requests to fail
    And I close the Unity app
    And I run the game in the "SwitchCacheNone" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "SwitchCacheNone"
    And I discard the oldest error
    Then I should receive no requests

  Scenario: Max Cache Size
    When I run the game in the "MaxSwitchCacheSize" state
    And I wait for requests to fail
    And I close the Unity app
    And I run the game in the "StartSDKDefault" state
    And I wait to receive 1 errors
    And the exception "message" equals "LARGE PAYLOAD 2"

  Scenario: Switch Metadata
    When I run the game in the "SwitchMetadata" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "SwitchMetadata"
    And the event "app.type" equals "nintendo-switch"
    And the event "device.osName" equals "Nintendo Switch"
    And the event "device.model" equals "Switch"
    And the event "device.manufacturer" equals "Nintendo"
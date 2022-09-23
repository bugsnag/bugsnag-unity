Feature: Switch Specific Tests


  @switch_only
  Scenario: SwitchCacheType set to None
    When I clear the Bugsnag cache
    And I wait for 5 seconds
    And I close the Unity app
    And I run the game in the "PersistEvent" state
    And I wait for 5 seconds
    And I close the Unity app
    And I run the game in the "SwitchCacheNone" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "SwitchCacheNone"
    And I discard the oldest error
    Then I should receive no requests

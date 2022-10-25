Feature: Callbacks

  Scenario: OnError callbacks in config
    When I run the game in the "OnErrorInConfig" state
    And I wait to receive 2 errors
    And I sort the errors by the payload field "events.0.exceptions.0.message"

    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Error 1"
    And all possible parameters have been edited in a callback
    And I discard the oldest error

    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Error 2"
    And all possible parameters have been edited in a callback

  Scenario: OnError callbacks added after Start
    When I run the game in the "OnErrorAfterStart" state
    And I wait to receive 2 errors
    And I sort the errors by the payload field "events.0.exceptions.0.message"

    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Error 1"
    And all possible parameters have been edited in a callback
    And I discard the oldest error

    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Error 2"
    And all possible parameters have been edited in a callback

  Scenario: OnSend callbacks in config
    When I run the game in the "OnSendInConfig" state
    And I wait to receive 2 errors
    And I sort the errors by the payload field "events.0.exceptions.0.message"

    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Error 1"
    And all possible parameters have been edited in a callback
    And I discard the oldest error

    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Error 2"
    And all possible parameters have been edited in a callback

  @skip_android #pending PLAT-9092
  Scenario: Callback passed directly to Notify
    When I run the game in the "CallbackInNotify" state
    And I wait to receive 1 error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Error 1"
    And all possible parameters have been edited in a callback

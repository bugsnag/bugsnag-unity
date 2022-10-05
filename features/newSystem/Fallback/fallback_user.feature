Feature: User operations tests

  Scenario: Set User In Config Csharp error
    When I run the game in the "SetUserInConfig" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "SetUserInConfig"
       
    # User
    And the event "user.id" equals "1"
    And the event "user.email" equals "2"
    And the event "user.name" equals "3"

  Scenario: Set User After Init Csharp Error
    When I run the game in the "SetUserAfterStart" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "SetUserAfterStart"
       
    # User
    And the event "user.id" equals "1"
    And the event "user.email" equals "2"
    And the event "user.name" equals "3"

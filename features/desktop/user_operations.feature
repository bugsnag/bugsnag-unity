Feature: User operations tests

    Scenario: Set User In Config Csharp error
        When I run the game in the "SetUserInConfigCsharpError" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the exception "errorClass" equals "ExecutionEngineException"
       
         # User
        And the event "user.id" equals "1"
        And the event "user.email" equals "2"
        And the event "user.name" equals "3"

    @macos_only
    Scenario: Set User In Config native crash
        When I run the game in the "SetUserInConfigNativeCrash" state
        And I run the game in the "(noop)" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the native Unity notifier
        And the exception "errorClass" equals "SIGABRT"
        # User
        And the event "user.id" equals "1"
        And the event "user.email" equals "2"
        And the event "user.name" equals "3"

    Scenario: Set User After Init Csharp Error
        When I run the game in the "SetUserAfterInitCsharpError" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the exception "errorClass" equals "Exception"
       
         # User
        And the event "user.id" equals "1"
        And the event "user.email" equals "2"
        And the event "user.name" equals "3"

    @macos_only
    Scenario: Set User After Init Native Error
        When I run the game in the "SetUserAfterInitNativeError" state
        And I run the game in the "(noop)" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the native Unity notifier
        And the exception "errorClass" equals "SIGABRT"
        # User
        And the event "user.id" equals "1"
        And the event "user.email" equals "2"
        And the event "user.name" equals "3"
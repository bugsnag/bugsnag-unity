Feature: Handled Errors and Exceptions

    Scenario: Reporting a handled exception
        When I run the game in the "Notify" state
        Then I should receive a request
        And the request is a valid for the error reporting API
        And the "Bugsnag-API-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
        And the payload field "notifier.name" equals "Unity Bugsnag Notifier"
        And the payload field "events" is an array with 1 element
        And the exception "errorClass" equals "Exception"
        And the exception "message" equals "blorb"
        And the event "unhandled" is false
        And the first significant stack frame methods and files should match:
            | Main.DoNotify()      |
            | Main.LoadScenario()  |
            | Main.Update()        |

    Scenario: Logging an unthrown exception
        When I run the game in the "LogUnthrown" state
        Then I should receive a request
        And the request is a valid for the error reporting API
        And the "Bugsnag-API-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
        And the payload field "notifier.name" equals "Unity Bugsnag Notifier"
        And the payload field "events" is an array with 1 element
        And the exception "errorClass" equals "Exception"
        And the exception "message" equals "auth failed!"
        And the event "unhandled" is false
        And the first significant stack frame methods and files should match:
            | Main:DoLogUnthrown() |
            | Main:LoadScenario()  |
            | Main:Update()        |


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

    Scenario: Reporting a handled exception with a callback
        When I run the game in the "NotifyCallback" state
        Then I should receive a request
        And the request is a valid for the error reporting API
        And the "Bugsnag-API-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
        And the payload field "notifier.name" equals "Unity Bugsnag Notifier"
        And the payload field "events" is an array with 1 element
        And the exception "errorClass" equals "FunnyBusiness"
        And the exception "message" equals "cake"
        And the event "unhandled" is false
        And the first significant stack frame methods and files should match:
            | Main.DoNotifyWithCallback() |
            | Main.LoadScenario()         |
            | Main.Update()               |

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

    Scenario: Logging an unthrown exception as unhandled
        When I run the game in the "LogUnthrownAsUnhandled" state
        Then I should receive a request
        And the request is a valid for the error reporting API
        And the "Bugsnag-API-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
        And the payload field "notifier.name" equals "Unity Bugsnag Notifier"
        And the payload field "events" is an array with 1 element
        And the exception "errorClass" equals "Exception"
        And the exception "message" equals "WAT"
        And the event "unhandled" is true
        And the first significant stack frame methods and files should match:
            | Main:DoLogUnthrownAsUnhandled() |
            | Main:LoadScenario()  |
            | Main:Update()        |

    Scenario: Logging a warning to Bugsnag
        When I run the game in the "ReportLoggedWarning" state
        Then I should receive a request
        And the request is a valid for the error reporting API
        And the "Bugsnag-API-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
        And the payload field "notifier.name" equals "Unity Bugsnag Notifier"
        And the payload field "events" is an array with 1 element
        And the exception "errorClass" equals "UnityLogWarning"
        And the exception "message" equals "Something went terribly awry"
        And the event "unhandled" is false
        And the first significant stack frame methods and files should match:
            | Main.DoLogWarning()  |
            | Main.LoadScenario()  |
            | Main.Update()        |

    Scenario: Logging an error to Bugsnag
        When I run the game in the "ReportLoggedError" state
        Then I should receive a request
        And the request is a valid for the error reporting API
        And the "Bugsnag-API-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
        And the payload field "notifier.name" equals "Unity Bugsnag Notifier"
        And the payload field "events" is an array with 1 element
        And the exception "errorClass" equals "UnityLogError"
        And the exception "message" equals "Bad bad things"
        And the event "unhandled" is false
        And the first significant stack frame methods and files should match:
            | Main.DoLogError()  |
            | Main.LoadScenario()  |
            | Main.Update()        |

    Scenario: Logging a warning to Bugsnag with 'ReportAsHandled = false'
        When I run the game in the "ReportLoggedWarningWithHandledConfig" state
        Then I should receive a request
        And the request is a valid for the error reporting API
        And the "Bugsnag-API-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
        And the payload field "notifier.name" equals "Unity Bugsnag Notifier"
        And the payload field "events" is an array with 1 element
        And the exception "errorClass" equals "UnityLogWarning"
        And the exception "message" equals "Something went terribly awry"
        And the event "unhandled" is false
        And the first significant stack frame methods and files should match:
            | Main.DoLogWarningWithHandledConfig()  |
            | Main.LoadScenario()  |
            | Main.Update()        |


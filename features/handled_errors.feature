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
        And custom metadata is included in the event
        And the first significant stack frame methods and files should match:
            | Main.DoNotify()      |
            | Main.LoadScenario()  |
            | Main.Update()        |

    Scenario: Reporting a handled exception from a background thread
        When I run the game in the "NotifyBackground" state
        Then I should receive a request
        And the request is a valid for the error reporting API
        And the "Bugsnag-API-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
        And the payload field "notifier.name" equals "Unity Bugsnag Notifier"
        And the payload field "events" is an array with 1 element
        And the exception "errorClass" equals "Exception"
        And the exception "message" equals "blorb"
        And the event "unhandled" is false
        And custom metadata is included in the event
        And the event "device.runtimeVersions.unity" is not null
        And the event "device.runtimeVersions.unityScriptingBackend" is not null
        And the event "device.runtimeVersions.dotnetScriptingRuntime" is not null
        And the event "device.runtimeVersions.dotnetApiCompatibility" is not null
        And the event "metaData.Unity.platform" equals "OSXPlayer"
        And the first significant stack frame methods and files should match:
            | Main.DoNotify()           | |
            | Main.<LoadScenario>m__0() | Main.<LoadScenario>b__5_0() |

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
        And custom metadata is included in the event
        And the event "metaData.shape.arc" equals "yes"
        And the first significant stack frame methods and files should match:
            | Main.DoNotifyWithCallback() |
            | Main.LoadScenario()         |
            | Main.Update()               |

    Scenario: Reporting a handled exception with a custom severity
        When I run the game in the "NotifySeverity" state
        Then I should receive a request
        And the request is a valid for the error reporting API
        And the "Bugsnag-API-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
        And the payload field "notifier.name" equals "Unity Bugsnag Notifier"
        And the payload field "events" is an array with 1 element
        And the exception "errorClass" equals "Exception"
        And the exception "message" equals "blorb"
        And the event "severity" equals "info"
        And the event "severityReason.type" equals "userSpecifiedSeverity"
        And the event "unhandled" is false
        And custom metadata is included in the event
        And the first significant stack frame methods and files should match:
            | Main.DoNotifyWithSeverity() |
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
        And custom metadata is included in the event
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
        And custom metadata is included in the event
        And the first significant stack frame methods and files should match:
            | Main:DoLogUnthrownAsUnhandled() |
            | Main:LoadScenario()  |
            | Main:Update()        |

    Scenario: Logging a warning from a background thread to Bugsnag
        When I run the game in the "ReportLoggedWarningThreaded" state
        Then I should receive a request
        And the request is a valid for the error reporting API
        And the "Bugsnag-API-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
        And the payload field "notifier.name" equals "Unity Bugsnag Notifier"
        And the payload field "events" is an array with 1 element
        And the exception "errorClass" equals "UnityLogWarning"
        And the exception "message" equals "Something went terribly awry"
        And the event "unhandled" is false
        And custom metadata is included in the event
        And the first significant stack frame methods and files should match:
            | Main.DoLogWarning()       |

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
        And custom metadata is included in the event
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
        And custom metadata is included in the event
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
        And custom metadata is included in the event
        And the first significant stack frame methods and files should match:
            | Main.DoLogWarningWithHandledConfig()  |
            | Main.LoadScenario()  |
            | Main.Update()        |

    Scenario: Notifying when the current release stage is not in "notify release stages"
        When I run the game in the "NotifyOutsideNotifyReleaseStages" state
        Then I should receive no requests

    Scenario: Logging an exception when the current release stage is not in "notify release stages"
        When I run the game in the "LogExceptionOutsideNotifyReleaseStages" state
        Then I should receive no requests

    Scenario: Reporting a handled exception when AutoNotify = false
        When I run the game in the "NotifyWithoutAutoNotify" state
        Then I should receive a request
        And the request is a valid for the error reporting API
        And the "Bugsnag-API-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
        And the payload field "notifier.name" equals "Unity Bugsnag Notifier"
        And the payload field "events" is an array with 1 element
        And the exception "errorClass" equals "Exception"
        And the exception "message" equals "blorb"
        And the event "unhandled" is false
        And custom metadata is included in the event
        And the first significant stack frame methods and files should match:
            | Main.DoNotify()      |
            | Main.LoadScenario()  |
            | Main.Update()        |


    Scenario: Logging an exception when AutoNotify = false
        When I run the game in the "LoggedExceptionWithoutAutoNotify" state
        Then I should receive no requests

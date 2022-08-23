Feature: Handled Errors and Exceptions

  Scenario: Reporting a handled exception
    When I run the game in the "Notify" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "Exception"
    And the exception "message" equals "blorb"
    And the event "unhandled" is false
    And the event "device.runtimeVersions.unity" is not null
    And the event "device.runtimeVersions.unityScriptingBackend" is not null
    And the event "device.runtimeVersions.dotnetScriptingRuntime" is not null
    And the event "device.runtimeVersions.dotnetApiCompatibility" is not null
    And custom metadata is included in the event
    And the stack frame methods should match:
      | Main.DoNotify()                   |
      | Main.RunScenario(string scenario) |
      | UnityEngine.SetupCoroutine.InvokeMoveNext(IEnumerator enumerator, IntPtr returnValueAddress) |

  Scenario: Calling notify with a custom stacktrace
    When I run the game in the "CustomStacktrace" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "Exception"
    And the exception "message" equals "CUSTOM"
    And the event "unhandled" is false
    And the stack frame methods should match:
      | Main.CUSTOM() |
      | Main.CUSTOM() |

  Scenario: Calling notify with only strings
    When I run the game in the "NotifyWithStrings" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "CUSTOM"
    And the exception "message" equals "CUSTOM"
    And the event "unhandled" is false
    And the stack frame methods should match:
      | Main.CUSTOM() |
      | Main.CUSTOM() |

  @skip_webgl
  Scenario: Reporting a handled exception from a background thread
    When I run the game in the "NotifyBackground" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "Exception"
    And the exception "message" equals "blorb"
    And the event "unhandled" is false
    And custom metadata is included in the event
    And the event "device.runtimeVersions.unity" is not null
    And the event "device.runtimeVersions.unityScriptingBackend" is not null
    And the event "device.runtimeVersions.dotnetScriptingRuntime" is not null
    And the event "device.runtimeVersions.dotnetApiCompatibility" is not null
    And the event "app.type" equals the platform-dependent string:
      | macos   | MacOS   |
      | windows | Windows |
      | switch  | Switch |
    And the stack frame methods should match:
      | Main.DoNotify() |

  Scenario: Custom App Type
    When I run the game in the "CustomAppType" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the event "app.type" equals "test"
    And the stack frame methods should match:
      | Main.DoNotify() |

  Scenario: Redacted Keys
    When I run the game in the "RedactedKeys" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the event "metaData.User.test" equals "[REDACTED]"
    And the event "metaData.User.password" equals "[REDACTED]"


  Scenario: Reporting a handled exception with a callback
    When I run the game in the "NotifyCallback" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "FunnyBusiness"
    And the exception "message" equals "cake"
    And the event "unhandled" is false
    And custom metadata is included in the event
    And the event "metaData.shape.arc" equals "yes"
    And the stack frame methods should match:
      | Main.DoNotifyWithCallback()       |
      | Main.RunScenario(string scenario) |
      | UnityEngine.SetupCoroutine.InvokeMoveNext(IEnumerator enumerator, IntPtr returnValueAddress) |

  Scenario: Reporting a handled exception with a custom severity
    When I run the game in the "NotifySeverity" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "Exception"
    And the exception "message" equals "blorb"
    And the event "severity" equals "info"
    And the event "severityReason.type" equals "userSpecifiedSeverity"
    And the event "unhandled" is false
    And custom metadata is included in the event
    And the stack frame methods should match:
      | Main.DoNotifyWithSeverity()       |
      | Main.RunScenario(string scenario) |
      | UnityEngine.SetupCoroutine.InvokeMoveNext(IEnumerator enumerator, IntPtr returnValueAddress) |

  Scenario: Logging an unthrown exception
    When I run the game in the "LogUnthrown" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "Exception"
    And the exception "message" equals "auth failed!"
    And the event "unhandled" is false
    And custom metadata is included in the event
    And the stack frame methods should match:
      | Main:DoLogUnthrown()     | Main.DoLogUnthrown()              |
      | Main:RunScenario(String) | Main.RunScenario(string scenario) |
      | UnityEngine.SetupCoroutine:InvokeMoveNext(IEnumerator, IntPtr) | UnityEngine.SetupCoroutine.InvokeMoveNext(IEnumerator enumerator, IntPtr returnValueAddress) |

  Scenario: Logging an unthrown exception as unhandled
    When I run the game in the "LogUnthrownAsUnhandled" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "Exception"
    And the exception "message" equals "WAT"
    And the event "unhandled" is true
    And custom metadata is included in the event
    And the stack frame methods should match:
      | Main:DebugLogException() | Main.DebugLogException()          |
      | Main:RunScenario(String) | Main.RunScenario(string scenario) |
      | UnityEngine.SetupCoroutine:InvokeMoveNext(IEnumerator, IntPtr) | UnityEngine.SetupCoroutine.InvokeMoveNext(IEnumerator enumerator, IntPtr returnValueAddress) |

  @skip_webgl
  Scenario: Logging a warning from a background thread to Bugsnag
    When I run the game in the "ReportLoggedWarningThreaded" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "UnityLogWarning"
    And the exception "message" equals "Something went terribly awry"
    And the event "unhandled" is false
    And custom metadata is included in the event
    And the stack frame methods should match:
      | Main.DoLogWarning() | Main:DoLogWarning() |

  Scenario: Notifying when the current release stage is not in "notify release stages"
    When I run the game in the "NotifyOutsideNotifyReleaseStages" state
    Then I should receive no errors

  Scenario: Logging an exception when the current release stage is not in "notify release stages"
    When I run the game in the "LogExceptionOutsideNotifyReleaseStages" state
    Then I should receive no errors

  Scenario: Reporting a handled exception when AutoNotify = false
    When I run the game in the "NotifyWithoutAutoNotify" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "Exception"
    And the exception "message" equals "blorb"
    And the event "unhandled" is false
    And custom metadata is included in the event
    And the stack frame methods should match:
      | Main.DoNotify()                   |
      | Main.RunScenario(string scenario) |
      | UnityEngine.SetupCoroutine.InvokeMoveNext(IEnumerator enumerator, IntPtr returnValueAddress) |

  Scenario: Logging an exception when AutoNotify = false
    When I run the game in the "LoggedExceptionWithoutAutoNotify" state
    And I wait for 5 seconds
    Then I should receive no errors

  Scenario: Logging a warning to Bugsnag
    When I run the game in the "ReportLoggedWarning" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "UnityLogWarning"
    And the exception "message" equals "Something went terribly awry"
    And the event "unhandled" is false
    And custom metadata is included in the event
    And the stack frame methods should match:
      | Main.DoLogWarning()               | Main:DoLogWarning()      |
      | Main.RunScenario(string scenario) | Main:RunScenario(String) |
      | UnityEngine.SetupCoroutine.InvokeMoveNext(IEnumerator enumerator, IntPtr returnValueAddress) | UnityEngine.SetupCoroutine:InvokeMoveNext(IEnumerator, IntPtr) |

  Scenario: Logging an error to Bugsnag
    When I run the game in the "ReportLoggedError" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "UnityLogError"
    And the exception "message" equals "Bad bad things"
    And the event "unhandled" is false
    And custom metadata is included in the event
    And the stack frame methods should match:
      | Main.DoLogError()                 | Main:DoLogError()        |
      | Main.RunScenario(string scenario) | Main:RunScenario(String) |
      | UnityEngine.SetupCoroutine.InvokeMoveNext(IEnumerator enumerator, IntPtr returnValueAddress) | UnityEngine.SetupCoroutine:InvokeMoveNext(IEnumerator, IntPtr) |

  Scenario: Logging a warning to Bugsnag with 'ReportAsHandled = false'
    When I run the game in the "ReportLoggedWarningWithHandledConfig" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "UnityLogWarning"
    And the exception "message" equals "Something went terribly awry"
    And the event "unhandled" is false
    And custom metadata is included in the event
    And the stack frame methods should match:
      | Main.DoLogWarningWithHandledConfig() | Main:DoLogWarningWithHandledConfig() |
      | Main.RunScenario(string scenario)    | Main:RunScenario(String)             |
      | UnityEngine.SetupCoroutine.InvokeMoveNext(IEnumerator enumerator, IntPtr returnValueAddress) | UnityEngine.SetupCoroutine:InvokeMoveNext(IEnumerator, IntPtr) |

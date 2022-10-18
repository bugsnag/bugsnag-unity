Feature: MacOS native crashes

  @macos_only
  Scenario: Reporting a MacOS native crash
    When I run the game in the "MacOSNativeCrash" state
    And I wait for 2 seconds
    And I run the game in the "StartSDKDefault" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the native Unity notifier
    And the exception "errorClass" equals "SIGABRT"
    And the event "unhandled" is true
    And the stack frame methods should match:
      | __pthread_kill       |
      | abort                |
      | crashy_signal_runner |
    And the error payload field "notifier.name" equals "Unity Bugsnag Notifier"
    And expected device metadata is included in the event
    And custom metadata is included in the event
    And feature flags are included in the event
    And the event "breadcrumbs.0.name" equals "Bugsnag loaded"
    And the event "breadcrumbs.1.name" equals "test"
    And the event "user.id" equals "1"
    And the event "user.email" equals "2"
    And the event "user.name" equals "3"


  @macos_only
  Scenario: Reporting a MacOS native crash with an onsend callback
    When I run the game in the "MacOSNativeCrash" state
    And I wait for 2 seconds
    And I run the game in the "MacOSNativeCrashCallback" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the native Unity notifier
    And the exception "errorClass" equals "ErrorClass"
    And the event "unhandled" is true
    And the error payload field "notifier.name" equals "Unity Bugsnag Notifier"
    
     # Device metadata
    And the event "device.osName" equals "OsName"
    And the event "device.osVersion" equals "OsVersion"
    And the event "device.id" equals "Id"
    And the event "device.model" equals "Model"
    And the event "device.orientation" equals "Orientation"
    And the event "device.manufacturer" equals "Manufacturer"
    And the event "device.freeDisk" equals 123
    And the event "device.freeMemory" equals 456
    And the event "device.jailbroken" is true
    And the event "device.locale" equals "Locale"

    # App metadata
    And the event "app.id" equals "Id"
    And the event "app.releaseStage" equals "ReleaseStage"
    And the event "app.type" equals "Type"
    And the event "app.version" equals "Version"
    And the event "app.bundleVersion" equals "BundleVersion"
    And the event "app.binaryArch" equals "BinaryArch"
    And the event "app.codeBundleId" equals "CodeBundleId"
    And the event "app.dsymUUIDs" is not null
    And the event "app.inForeground" is false
    And the event "app.isLaunching" is false

    # Exception data
    And the event "exceptions.0.errorClass" equals "ErrorClass"
    And the event "exceptions.0.stacktrace.0.method" equals "Method"
    #And the event "exceptions.0.stacktrace.0.frameAddress" equals "FrameAddress"
    And the event "exceptions.0.stacktrace.0.isLR" is true
    And the event "exceptions.0.stacktrace.0.isPC" is true
    And the event "exceptions.0.stacktrace.0.machoFile" equals "MachoFile"
    #And the event "exceptions.0.stacktrace.0.machoLoadAddress" equals "MachoLoadAddress"
    And the event "exceptions.0.stacktrace.0.machoUUID" equals "MachoUuid"
    #And the event "exceptions.0.stacktrace.0.machoVmAddress" equals "MachoVmAddress"
    #And the event "exceptions.0.stacktrace.0.symbolAddress" equals "SymbolAddress"

    # Breadcrumbs
    And the event "breadcrumbs.0.type" equals "request"
    And the event "breadcrumbs.0.name" equals "Custom Message"
    And the event "breadcrumbs.0.metaData.test" equals "test"

    # Feature flags
    And the event "featureFlags.2.featureFlag" equals "fromCallback"
    And the event "featureFlags.2.variant" equals "a"

    # Metadata
    And the event "metaData.test1.test" equals "test"
    And the event "metaData.test2" is null

    # User
    And the event "user.id" equals "4"
    And the event "user.email" equals "5"
    And the event "user.name" equals "6"

  @macos_only
  Scenario: Set User After Init Native Error
    When I run the game in the "MacOSSetUserAfterInitNativeCrash" state
    And I wait for 2 seconds
    And I run the game in the "StartSDKDefault" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the native Unity notifier
    And the exception "errorClass" equals "SIGABRT"

    # User
    And the event "user.id" equals "1"
    And the event "user.email" equals "2"
    And the event "user.name" equals "3"

  @macos_only
  Scenario: Native crash outside of release stage
    When I run the game in the "MacOSNativeCrashOutsideReleaseStages" state
    And I wait for 2 seconds
    And I run the game in the "StartSDKDefault" state
    Then I should receive no errors

  @macos_only
  Scenario: Reporting a native crash when AutoDetectErrors = false
    When I run the game in the "MacOSNativeCrashAutoDetectErrorsFalse" state
    And I wait for 2 seconds
    And I run the game in the "StartSDKDefault" state
    Then I should receive no errors

   @macos_only
  Scenario: Reporting a native crash when EnabledErrorTypes.Crashes = false
    When I run the game in the "MacOSNativeCrashEnabledErrorTypes" state
    And I wait for 2 seconds
    And I run the game in the "StartSDKDefault" state
    Then I should receive no errors


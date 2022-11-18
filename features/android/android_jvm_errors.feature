Feature: Android JVM Exceptions

  Background:
    Given I clear the Bugsnag cache

  Scenario: Android JVM Smoke Test
    When I run the game in the "AndroidJVMSmokeTest" state
    And I wait to receive an error
    And the error payload field "notifier.name" equals "Unity Bugsnag Notifier"
    And expected device metadata is included in the event
    And expected app metadata is included in the event
    And custom metadata is included in the event
    And feature flags are included in the event
    And the event "breadcrumbs.0.name" equals "Bugsnag loaded"
    And the event "breadcrumbs.1.name" equals "test"
    And the event "user.id" equals "1"
    And the event "user.email" equals "2"
    And the event "user.name" equals "3"

        # Exception details
    And the error payload field "events" is an array with 1 elements
    And the exception "errorClass" equals "java.lang.ArrayIndexOutOfBoundsException"
    And the exception "message" equals "length=2; index=2"
    And the event "unhandled" is true
    And the event "severity" equals "error"
    And the error payload field "events.0.projectPackages" is a non-empty array
    And the event "severityReason.type" equals "unhandledException"

        # Stacktrace validation
    And the error payload field "events.0.exceptions.0.stacktrace" is a non-empty array
    And the event "exceptions.0.stacktrace.0.method" equals "com.example.bugsnagcrashplugin.CrashHelper.triggerJvmException()"
    And the exception "stacktrace.0.file" equals "CrashHelper.java"
    And the event "exceptions.0.stacktrace.0.lineNumber" equals 13
    And the error payload field "events.0.threads" is null

    #NOTE: Metadata testing will be improved in this scenario after PLAT-9127
  Scenario: Android JVM Background Thread Smoke Test
    When I run the game in the "AndroidBackgroundJVMSmokeTest" state
    And I wait for 2 seconds
    And I clear any error dialogue
    And On Mobile I relaunch the app
    And I run the game in the "StartSDKDefault" state
    And I wait to receive an error
    And expected device metadata is included in the event
    And expected app metadata is included in the event
    And feature flags are included in the event
    And the event "breadcrumbs.0.name" equals "Bugsnag loaded"
    And the event "breadcrumbs.1.name" equals "test"
    And the event "user.id" equals "1"
    And the event "user.email" equals "2"
    And the event "user.name" equals "3"

        # Exception details
    And the error payload field "events" is an array with 1 elements
    And the exception "errorClass" equals "java.lang.RuntimeException"
    And the exception "message" equals "Uncaught JVM exception from background thread"
    And the event "unhandled" is true
    And the event "severity" equals "error"
    And the error payload field "events.0.projectPackages" is a non-empty array
    And the event "severityReason.type" equals "unhandledException"

        # Stacktrace validation
    And the error payload field "events.0.exceptions.0.stacktrace" is a non-empty array
    And the exception "stacktrace.0.file" equals "CrashHelper.java"
    And the event "exceptions.0.stacktrace.0.lineNumber" equals 19
    And the error payload field "events.0.threads" is not null

        # Metadata
    And the event "metaData.init" is null
    And the event "metaData.custom.letter" equals "QX"
    And the event "metaData.custom.better" equals "400"
    And the event "metaData.test.test1" equals "test1"
    And the event "metaData.test.test2" is null

    And the error payload field "events.0.usage.config" is not null
    And the error payload field "events.0.usage.callbacks.onSession" equals 1

  Scenario: Last Run Info
    When I run the game in the "AndroidBackgroundJVMSmokeTest" state
    And I wait for 2 seconds
    And I clear any error dialogue
    And On Mobile I relaunch the app
    And I run the game in the "AndroidLastRunInfo" state
    And I wait to receive 2 errors
    And I discard the oldest error
    And the exception "message" equals "Last Run Info Correct"

  Scenario: Disable Crashes
    When I run the game in the "AndroidDisableCrashes" state
    And I wait for 2 seconds
    And I clear any error dialogue
    And On Mobile I relaunch the app
    And I run the game in the "StartSDKDefault" state
    And I should receive no errors
       



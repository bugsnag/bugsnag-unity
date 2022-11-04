Feature: iOS Native Errors

  Background:
    Given I clear the Bugsnag cache

  Scenario: Disable Crashes
    When I run the game in the "IosDisableCrashes" state
    And I wait for 2 seconds
    And On Mobile I relaunch the app
    And I run the game in the "StartSDKDefault" state
    And I wait to receive an error
    And the exception "message" equals "The app was likely terminated by the operating system while in the foreground"

  Scenario: Last Run Info
    When I run the game in the "IosNativeException" state
    And I wait for 2 seconds
    And I clear any error dialogue
    And On Mobile I relaunch the app
    And I run the game in the "IosLastRunInfo" state
    And I wait for 3 seconds
    And I wait to receive 2 errors
    And I discard the oldest error
    And the exception "message" equals "Last Run Info Correct"

  Scenario: iOS native exception Smoke Test
    When I run the game in the "IosNativeException" state
    And I wait for 2 seconds
    And I clear any error dialogue
    And On Mobile I relaunch the app
    And I run the game in the "StartSDKDefault" state
    And I wait to receive an error
    And expected device metadata is included in the event
    And feature flags are included in the event
    And the event "breadcrumbs.0.name" equals "Bugsnag loaded"
    And the event "breadcrumbs.1.name" equals "test"
    And the event "user.id" equals "1"
    And the event "user.email" equals "2"
    And the event "user.name" equals "3"

    #App metadata
    And the event "app.duration" is greater than 0
    And the event "app.durationInForeground" is not null
    And the event "app.inForeground" is not null
    And the event "app.isLaunching" is not null
    And the event "app.releaseStage" is not null
    And the event "app.type" is not null
    And the event "app.version" is not null
    And the event "metaData.app.companyName" equals "bugsnag"
    And the event "metaData.app.name" matches ".azerunner"
    And the event "metaData.app.buildno" is not null

    # Exception details
    And the error payload field "events" is an array with 1 elements
    And the exception "errorClass" equals "St13runtime_error"
    And the exception "message" equals "CocoaCppException"
    And the event "unhandled" is true
    And the event "severity" equals "error"

    # Metadata
    And the event "metaData.init" is null
    And the event "metaData.custom.letter" equals "QX"
    And the event "metaData.custom.better" equals 400
    And the event "metaData.test.test1" equals "test1"
    And the event "metaData.test.test2" is null

  Scenario: iOS signal Smoke Test
    When I run the game in the "IosSignal" state
    And I wait for 2 seconds
    And I clear any error dialogue
    And On Mobile I relaunch the app
    And I run the game in the "StartSDKDefault" state
    And I wait to receive an error
    And expected device metadata is included in the event
    And feature flags are included in the event
    And the event "breadcrumbs.0.name" equals "Bugsnag loaded"
    And the event "breadcrumbs.1.name" equals "test"
    And the event "user.id" equals "1"
    And the event "user.email" equals "2"
    And the event "user.name" equals "3"

    #App metadata
    And the event "app.duration" is greater than 0
    And the event "app.durationInForeground" is not null
    And the event "app.inForeground" is not null
    And the event "app.isLaunching" is not null
    And the event "app.releaseStage" is not null
    And the event "app.type" is not null
    And the event "app.version" is not null
    And the event "metaData.app.companyName" equals "bugsnag"
    And the event "metaData.app.name" matches ".azerunner"
    And the event "metaData.app.buildno" is not null

    # Exception details
    And the error payload field "events" is an array with 1 elements
    And the exception "errorClass" equals "SIGABRT"
    And the exception "message" equals ""
    And the event "unhandled" is true
    And the event "severity" equals "error"
    And the error payload field "events.0.exceptions.0.stacktrace" is a non-empty array
    And the event "exceptions.0.stacktrace.0.method" equals "__pthread_kill"

    # Metadata
    And the event "metaData.init" is null
    And the event "metaData.custom.letter" equals "QX"
    And the event "metaData.custom.better" equals 400
    And the event "metaData.test.test1" equals "test1"
    And the event "metaData.test.test2" is null





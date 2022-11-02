Feature: Callbacks

  Background:
    Given I clear the Bugsnag cache

  Scenario: On Send Native Callback

    When I run the game in the "AndroidBackgroundJVMSmokeTest" state
    And I wait for 2 seconds
    And I clear any error dialogue
    And On Mobile I relaunch the app
    And I run the game in the "AndroidOnSendCallback" state
    And I wait to receive an error

    And the error payload field "apiKey" equals "Custom ApiKey"

        # misc data
    And the event "context" equals "Custom Context"
    And the event "severity" equals "info"
    And the event "unhandled" is false

        # metadata
    And the event "metaData.test.scoop" equals "dewoop"
    And the event "metaData.test2" is null

        # user
    And the event "user.id" equals "1"
    And the event "user.email" equals "2"
    And the event "user.name" equals "3"

        # App Data
    And the event "app.binaryArch" equals "Custom BinaryArch"
    And the event "app.buildUUID" equals "Custom BuildUuid"
    And the event "app.codeBundleId" equals "Custom CodeBundleId"
    And the event "app.id" equals "Custom Id"
    And the event "app.releaseStage" equals "Custom ReleaseStage"
    And the event "app.type" equals "Custom Type"
    And the event "app.version" equals "Custom Version"
    And the event "app.versionCode" equals 999
    And the event "app.duration" equals 1000
    And the event "app.durationInForeground" equals 2000
    And the event "app.inForeground" is false
    And the event "app.isLaunching" is false

        # Device data
    And the error payload field "events.0.device.cpuAbi" is an array with 2 elements
    And the event "device.jailbroken" is true
    And the event "device.id" equals "Custom Device Id"
    And the event "device.locale" equals "Custom Locale"
    And the event "device.manufacturer" equals "Custom Manufacturer"
    And the event "device.model" equals "Custom Model"
    And the event "device.osName" equals "Custom OsName"
    And the event "device.osVersion" equals "Custom OsVersion"
    And the event "device.totalMemory" equals 555

    And the event "device.runtimeVersions.scoop" equals "dewoop"
    And the event "device.time" equals "1985-08-21T01:01:01.000Z"
    And the event "device.orientation" equals "Custom Orientation"

        # Breadcrumbs
    And the event "breadcrumbs.0.name" equals "Custom Message"
    And the event "breadcrumbs.0.type" equals "user"
    And the event "breadcrumbs.0.metaData.Custom" equals "Metadata"

        # Feature flags
    And the event "featureFlags.0.featureFlag" equals "flag1"
    And the event "featureFlags.0.variant" equals "variant1"
    And the event "featureFlags.2.featureFlag" equals "test"
    And the event "featureFlags.2.variant" equals "variant"
    And the event "featureFlags.3" is null

        # threads
    And the event "threads.0.name" equals "Custom Name"

        # errors
    And the event "exceptions.0.errorClass" equals "Custom ErrorClass"
    And the event "exceptions.0.message" equals "Custom ErrorMessage"

        # stacktrace
    And the event "exceptions.0.stacktrace.0.method" equals "Custom Method"
    And the event "exceptions.0.stacktrace.0.file" equals "Custom File"
    And the event "exceptions.0.stacktrace.0.lineNumber" equals 123123
    And the event "exceptions.0.stacktrace.0.inProject" is false

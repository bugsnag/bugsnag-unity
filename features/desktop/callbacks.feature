Feature: Callbacks

  Scenario: CSharp event callback
    When I run the game in the "EventCallbacks" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "ErrorClass"
    And the exception "message" equals "blorb"

    # Device metadata
    And the event "device.osName" equals "OsName"
    And the event "device.osVersion" equals "OsVersion"
    And the event "device.id" equals "Id"
    And the event "device.model" equals "Model"
    And the event "device.orientation" equals "Orientation"
    And the event "device.manufacturer" equals "Manufacturer"
    And the event "device.freeDisk" equals 123
    And the event "device.freeMemory" equals 123
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
    And the event "app.dsymUuid" equals "DsymUuid"
    And the event "app.inForeground" is false
    And the event "app.isLaunching" is false

    # Exception data
    And the event "exceptions.0.errorClass" equals "ErrorClass"
    And the event "exceptions.0.stacktrace.0.method" equals "Method"
    And the event "exceptions.0.stacktrace.0.lineNumber" equals 0

    # Breadcrumbs
    And the event "breadcrumbs.0.type" equals "request"
    And the event "breadcrumbs.0.name" equals "Custom Message"
    And the event "breadcrumbs.0.metaData.test" equals "test"

    # Metadata
    And the event "metaData.test1.test" equals "test"
    And the event "metaData.test2" is null

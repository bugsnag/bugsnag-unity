Feature: Android NDK crash

    #NOTE: Metadata testing will be improved in this scenario after PLAT-9127

    Scenario: NDK Signal raised
        When I run the game in the "AndroidNDKSignal" state
        And I wait for 3 seconds
        And I clear any error dialogue
        And On Mobile I relaunch the app
        And I run the game in the "StartSDKDefault" state
        # Intentionally adding long wait times here - a core component of this
        # feature is ensuring that only a SINGLE event is sent. Unity includes
        # a handler for NDK events which are delivered immediately and should
        # be intentionally ignored and the event discarded, since the same
        # event has already been captured and will be sent next launch
        And I wait for 5 seconds
        Then I wait to receive 1 error

        # Exception details
        And the error payload field "events" is an array with 1 elements
        And the exception "errorClass" equals "SIGSEGV"
        And the exception "message" equals "Segmentation violation (invalid memory reference)"
        And the exception "type" equals "c"
        And the event "unhandled" is true
        And the event "severity" equals "error"
        And the event "severityReason.type" equals "signal"
        And the event "severityReason.attributes.signalType" equals "SIGSEGV"
        And the event "severityReason.unhandledOverridden" is false
        And expected app metadata is included in the event
        # Stacktrace validation
        And the error payload field "events.0.exceptions.0.stacktrace" is a non-empty array
        And the event "exceptions.0.stacktrace.0.method" is not null
        And the error payload field "events.0.exceptions.0.stacktrace.0.frameAddress" starts with "0x"

        #breadcrumbs
        And the event "breadcrumbs.0.name" equals "Bugsnag loaded"
        And the event "breadcrumbs.1.name" equals "test"

        # User
        And the event "user.id" is not null

        # Native context override
        And the event "context" equals "My Context"

        # Metadata
        And the event "metaData.init" is null
        And the event "metaData.custom.letter" equals "QX"
        And the event "metaData.custom.better" equals "400"
        And the event "metaData.test.test1" equals "test1"
        And the event "metaData.test.test2" is null

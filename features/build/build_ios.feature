Feature: Build iOS

  Scenario: Auto Symbol Upload
  	When I run the script "features/scripts/prepare_fixture.sh" synchronously
    When I run the script "features/scripts/generate_xcode_project.sh release" synchronously
  	When I run the script "features/scripts/build_ios.sh release" synchronously
  	Then I wait to receive 3 sourcemaps
    Then the sourcemaps Content-Type header is valid multipart form-data
    And the sourcemap payload field "apiKey" equals "a35a2a72bd230ac0aa0f52715bbdc6aa"

    And I discard the oldest sourcemaps

    Then the sourcemaps Content-Type header is valid multipart form-data
    And the sourcemap payload field "apiKey" equals "a35a2a72bd230ac0aa0f52715bbdc6aa"

    And I discard the oldest sourcemaps

    Then the sourcemap is valid for the Unity Line Mapping API
    Then the sourcemaps Content-Type header is valid multipart form-data
    And the sourcemap payload field "appVersion" equals "1.0"
    And the sourcemap payload field "appBundleVersion" equals "333"
    And the sourcemap payload field "dsymUUID" is not null
    And the sourcemap payload field "appId" equals "com.apple.xcode.dsym.com.bugsnag.fixtures.unity.notifier.ios"

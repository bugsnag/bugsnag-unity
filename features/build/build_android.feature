Feature: Build Android

  @skip_unity_2020 # Unity 2020 only produces 4 symbol files
  Scenario: Auto Symbol Upload
  	When I run the script "features/scripts/prepare_fixture.sh android" synchronously
    And I wait for the build to succeed

  	When I run the script "features/scripts/build_android.sh release" synchronously
    And I wait for the build to succeed

  	Then I wait to receive 6 sourcemaps
    Then the sourcemaps Content-Type header is valid multipart form-data
    And the sourcemap payload field "apiKey" equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    And the sourcemap payload field "versionCode" equals "123"
    And the sourcemap payload field "versionName" equals "1.2.3"
    And the sourcemap payload field "appId" equals "com.bugsnag.fixtures.unity.notifier.android"
    

  @unity_2020_only
  Scenario: Auto Symbol Upload Unity 2020
    When I run the script "features/scripts/prepare_fixture.sh android" synchronously
    And I wait for the build to succeed

    When I run the script "features/scripts/build_android.sh release" synchronously
    And I wait for the build to succeed

    Then I wait to receive 4 sourcemaps
    Then the sourcemaps Content-Type header is valid multipart form-data
    And the sourcemap payload field "apiKey" equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    And the sourcemap payload field "versionCode" equals "123"
    And the sourcemap payload field "versionName" equals "1.2.3"
    And the sourcemap payload field "appId" equals "com.bugsnag.fixtures.unity.notifier.android"

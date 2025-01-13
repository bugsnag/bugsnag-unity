Feature: Build Android

  Scenario: Auto Symbol Upload
  	When I run the script "features/scripts/prepare_fixture.sh" synchronously
  	When I run the script "features/scripts/build_android.sh release" synchronously
  	Then I wait to receive 6 sourcemaps
    Then the sourcemaps Content-Type header is valid multipart form-data
    And the sourcemap payload field "apiKey" equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    And the sourcemap payload field "versionCode" equals "123"
    And the sourcemap payload field "versionName" equals "1.2.3"

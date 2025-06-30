Feature: Build iOS

  Scenario: Auto Symbol Upload
  	When I run the script "features/scripts/prepare_fixture.sh ios" synchronously
    When I run the script "features/scripts/generate_xcode_project.sh release" synchronously
  	When I run the script "features/scripts/build_ios.sh release" synchronously
  	Then I wait to receive 2 sourcemaps
    Then the sourcemaps Content-Type header is valid multipart form-data
    And the sourcemap payload field "apiKey" equals "a35a2a72bd230ac0aa0f52715bbdc6aa"

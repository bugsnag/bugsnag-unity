Feature: Build iOS

  Scenario: Auto Symbol Upload
  	When I run the script "features/scripts/prepare_fixture.sh ios" synchronously
    And I wait for the build to succeed

    When I run the script "features/scripts/generate_xcode_project.sh release" synchronously
    And I wait for the build to succeed

  	When I run the script "features/scripts/build_ios.sh release" synchronously
    And I wait for the build to succeed

  	Then I wait to receive 2 sourcemaps
    Then the sourcemaps Content-Type header is valid multipart form-data
    And the sourcemap payload field "apiKey" equals "a35a2a72bd230ac0aa0f52715bbdc6aa"

Feature: Build Android

  Scenario: Auto Symbol Upload
  	When I run the script "features/scripts/prepare_fixture.sh" synchronously
  	When I run the script "features/scripts/build_android.sh release" synchronously
  	Then I wait to receive 6 uploads
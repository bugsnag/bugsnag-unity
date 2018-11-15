When("I run the game in the {string} state") do |state|
  steps %Q{
    When I set environment variable "BUGSNAG_SCENARIO" to "#{state}"
    When I set environment variable "MAZE_ENDPOINT" to "http://localhost:#{MOCK_API_PORT}"
    And I run the script "features/scripts/launch_unity_application.sh"
    And I wait for 8 seconds
  }
end
Then("the first significant stack frame methods and files should match:") do |expected_values|
  stacktrace = read_key_path(find_request(0)[:body], "events.0.exceptions.0.stacktrace")
  expected_frame_values = expected_values.raw
  expected_index = 0
  stacktrace.each_with_index do |item, index|
    next if expected_index >= expected_frame_values.length
    expected_frame = expected_frame_values[expected_index]
    next if item["method"].start_with? "UnityEngine"

    assert_equal(expected_frame[0], item["method"])
    expected_index += 1
  end
end

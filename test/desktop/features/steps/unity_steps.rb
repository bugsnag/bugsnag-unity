When("I run the game in the {string} state") do |state|
  Maze::Runner.environment['BUGSNAG_SCENARIO'] = state
  Maze::Runner.environment['BUGSNAG_APIKEY'] = $api_key
  Maze::Runner.environment['MAZE_ENDPOINT'] = 'http://localhost:9339'
  command = %(
    open -W #{Maze.config.app} --args -batchmode -nographics
  )
  Maze::Runner.run_command(command)
end

Then("the error is valid for the error reporting API sent by the {string}") do |notifier_name|
  steps %(
    Then the error "Bugsnag-Api-Key" header equals "#{$api_key}"
    And the error payload field "apiKey" equals "#{$api_key}"
    And the error "Bugsnag-Payload-Version" header equals "4.0"
    And the error "Content-Type" header equals "application/json"
    And the error "Bugsnag-Sent-At" header is a timestamp
    And the error Bugsnag-Integrity header is valid

    And the error payload field "notifier.name" equals "#{notifier_name}"
    And the error payload field "notifier.url" is not null
    And the error payload field "notifier.version" is not null
    And the error payload field "events" is a non-empty array

    And each element in error payload field "events" has "severity"
    And each element in error payload field "events" has "severityReason.type"
    And each element in error payload field "events" has "unhandled"
    And each element in error payload field "events" has "exceptions"
  )
end

Then("the errors are valid for the error reporting API sent by the {string}") do |notifier_name|

end

Then("the first significant stack frame methods and files should match:") do |expected_values|
  stacktrace = Maze::Helper.read_key_path(Maze::Server.errors.current[:body], "events.0.exceptions.0.stacktrace")
  expected_frame_values = expected_values.raw
  expected_index = 0
  flunk("The stacktrace is empty") if stacktrace.length == 0
  stacktrace.each_with_index do |item, index|
    next if expected_index >= expected_frame_values.length
    expected_frames = expected_frame_values[expected_index]
    next if item["method"].start_with? "UnityEngine"
    next if item["method"].start_with? "BugsnagUnity"

    assert(expected_frames.any? { |frame| frame == item["method"] }, "None of the given methods match the frame #{item["method"]}")
    expected_index += 1
  end
end

Then("the current error request events match one of:") do |table|
  events = Maze::Server.errors.all.map do |error|
    Maze::Helper.read_key_path(error[:body], "events")
  end.flatten
  table.hashes.each do |values|
    assert_not_nil(events.detect do |event|
      handled_count = Maze::Helper.read_key_path(event, "session.events.handled")
      unhandled_count = Maze::Helper.read_key_path(event, "session.events.unhandled")
      message = Maze::Helper.read_key_path(event, "exceptions.0.message")
      handled_count == values["handled"].to_i &&
        unhandled_count == values["unhandled"].to_i &&
        message == values["message"]
    end, "No event matches the following values: #{values}")
  end
end

Then("the event {string} matches one of:") do |path, table|
  payload_value = Maze::Helper.read_key_path(Maze::Server.errors.current[:body], "events.0.#{path}")
  valid_values = table.raw.flat_map { |e| e }
  assert(valid_values.any? { |frame| frame == payload_value }, "Value #{payload_value} did not match any of the expected values")
end

Then("custom metadata is included in the event") do
  steps %Q{
    Then the event "metaData.app.buildno" equals "0.1"
    And the event "metaData.app.cache" is null
    And the event "metaData.init" is null
    And the event "metaData.custom.letter" equals "QX"
    And the event "metaData.custom.better" equals "400"
    And the event "metaData.custom.setter" equals "more stuff"
  }
end

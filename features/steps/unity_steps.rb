require 'cgi'

#
# Mobile steps
#

When("I wait for the mobile game to start") do
  # Wait for a fixed time period
  # TODO: PLAT-6655 Remove the Unity splash screen so we don't have to wait so long
  sleep 3
end

When('I clear all persistent data') do
  step('I run the "Clear iOS Data" command')
end

When('I relaunch the Unity mobile app') do
  Maze.driver.launch_app
  # Wait for a fixed time period
  sleep 3
end

def dial_number_for(name)
  lookup = {
      # Scenarios
      "throw Exception" => 1,
      "Log error" => 2,
      "Native exception" => 3,
      "Log caught exception" => 4,
      "NDK signal" => 5,
      "Notify caught exception" => 6,
      "Notify with callback" => 7,
      "Change scene" => 8,
      "Disable Breadcrumbs" => 9,
      "Start SDK" => 10,
      "Max Breadcrumbs" => 11,
      "Disable Native Errors" => 12,
      "throw Exception with breadcrumbs" => 13,
      "Start SDK no errors" => 14,
      "Discard Error Class" => 15,
      "Java Background Crash" => 16,
      "Custom App Type" => 17,


      # Commands
      "Clear iOS Data" => 90
  }
  number = lookup[name]
  $logger.debug "Command/scenario '#{name}' has dial-in code #{number}"

  step("I dial #{number / 10}")
  sleep 1
  step("I dial #{number % 10}")
  sleep 1
end

When("I run the {string} command") do |command|
  dial_number_for command
  step("I press Run Command")
end

When("I run the {string} mobile scenario") do |scenario|
  dial_number_for scenario
  step("I press Run Scenario")
end

When("I dial {int}") do |number|
  $logger.debug "Dialling #{number}"
  press_at 40 + (number * 80)
  sleep 1
end

When("I press Run Scenario") do
  press_at 840
end

When("I press Run Command") do
  press_at 920
end

def press_at(y)

  # Ensure we tap in the button
  viewport = Maze.driver.session_capabilities['viewportRect']
  x = viewport['width'] / 2

  $logger.debug "Press at: #{x},#{y}"

  # TODO: PLAT-6654 Figure out why the scale is different on iOS
  factor = if Maze.driver.capabilities['os'] == 'ios'
             0.5
           else
             1
           end

  touch_action = Appium::TouchAction.new
  touch_action.tap({:x => x * factor, :y => y * factor})
  touch_action.perform
end

#
# Desktop steps
#
When("I run the game in the {string} state") do |state|
  endpoint = "http://localhost:#{Maze.config.port}"

  if Maze.config.os == 'macos'
    Maze::Runner.environment['BUGSNAG_SCENARIO'] = state
    Maze::Runner.environment['BUGSNAG_APIKEY'] = $api_key
    Maze::Runner.environment['MAZE_ENDPOINT'] = endpoint

    # Call executable directly rather than use open, which flakes on CI
    command = "#{Maze.config.app}/Contents/MacOS/Mazerunner --args -batchmode -nographics"
    Maze::Runner.run_command(command)

  elsif Maze.config.os == 'windows'
    command = "#{Maze.config.app} -batchmode -nographics"
    env = {
        'BUGSNAG_SCENARIO' => state,
        'BUGSNAG_APIKEY' => $api_key,
        'MAZE_ENDPOINT' => endpoint
    }
    system(env, command)

  else
    # WebGL in a browser
    # endpoint = CGI.escape endpoint
    fixture_host = "http://localhost:#{Maze.config.document_server_port}"
    url = "#{fixture_host}/index.html?BUGSNAG_SCENARIO=#{state}&BUGSNAG_APIKEY=#{$api_key}&MAZE_ENDPOINT=#{endpoint}"
    $logger.debug "Navigating to URL: #{url}"
    step("I navigate to the URL \"#{url}\"")
  end
end

def check_error_reporting_api(notifier_name)
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

Then("the error is valid for the error reporting API sent by the native Unity notifier") do
  # This step currently only applies to native errors on macOS macOS
  check_error_reporting_api 'OSX Bugsnag Notifier'
end

Then("the error is valid for the error reporting API sent by the Unity notifier") do

  if Maze.config.farm == :bs
    # Mobile - could be ios or android
    os = Maze.config.capabilities['os']
  else
    # Could be windows or macos
    os = Maze.config.os
  end

  if os == 'ios'
    notifier_name = 'iOS Bugsnag Notifier'
  elsif os == 'android'
    notifier_name = 'Android Bugsnag Notifier'
  else
    notifier_name = 'Unity Bugsnag Notifier'
  end

  check_error_reporting_api notifier_name
end

Then("the first significant stack frame methods and files should match:") do |expected_values|
  stacktrace = Maze::Helper.read_key_path(Maze::Server.errors.current[:body], "events.0.exceptions.0.stacktrace")
  expected_frame_values = expected_values.raw
  expected_index = 0

  flunk("The stacktrace is empty") if stacktrace.length == 0
  flunk("The stacktrace is not long enough") if stacktrace.length < expected_frame_values.length

  stacktrace.each_with_index do |item, index|
    next if expected_index >= expected_frame_values.length
    expected_frames = expected_frame_values[expected_index]

    method = item['method']
    next if method.start_with? 'UnityEngine' or method.start_with? 'BugsnagUnity'

    frame_matches = expected_frames.any? { |frame| frame == method }
    assert(frame_matches, "None of the given methods match the frame #{method}")
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

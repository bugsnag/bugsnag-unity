require 'cgi'

#
# Common steps
#
def execute_command(action, scenario_name = '')
  command = {
    action: action,
    scenarioName: scenario_name
  }
  Maze::Server.commands.add command

  # Ensure fixture has read the command
  count = 300
  sleep 0.1 until Maze::Server.commands.remaining.empty? || (count -= 1) < 1
  raise 'Test fixture did not GET /command' unless Maze::Server.commands.remaining.empty?
end

When('I clear the Bugsnag cache') do
  case Maze::Helper.get_current_platform
  when 'macos', 'webgl'
    # Call executable directly rather than use open, which flakes on CI
    log = File.join(Dir.pwd, 'mazerunner.log')
    command = "#{Maze.config.app}/Contents/MacOS/Mazerunner --args -logfile #{log} > /dev/null"
    Maze::Runner.run_command(command, blocking: false)

    execute_command('clear_cache')

  when 'windows'
    wsl_log = File.join(Dir.pwd, 'mazerunner.log')
    win_log = `wslpath -w #{wsl_log}`
    command = "#{Maze.config.app} --args -logfile #{win_log}"
    Maze::Runner.run_command(command, blocking: false)

    execute_command('clear_cache')

  when 'android', 'ios'
    execute_command('clear_cache')

  else
    url = "http://localhost:#{Maze.config.document_server_port}/index.html"
    $logger.debug "Navigating to URL: #{url}"
    step("I navigate to the URL \"#{url}\"")
    execute_command('clear_cache')
  end
end

When('I close the Unity app') do
  case Maze::Helper.get_current_platform
  when 'macos','webgl','windows'
    execute_command('close_application')
  when 'android', 'ios'
    execute_command('close_application')
  end
end

When('I run the game in the {string} state') do |state|
  case Maze::Helper.get_current_platform
  when 'macos'
    # Call executable directly rather than use open, which flakes on CI
    log = File.join(Dir.pwd, 'mazerunner.log')
    command = "#{Maze.config.app}/Contents/MacOS/Mazerunner --args -logfile #{log} > /dev/null"
    Maze::Runner.run_command(command, blocking: false)

    execute_command('run_scenario', state)

  when 'windows'
    wsl_log = File.join(Dir.pwd, 'mazerunner.log')
    win_log = `wslpath -w #{wsl_log}`
    command = "#{Maze.config.app} --args -logfile #{win_log}"
    Maze::Runner.run_command(command, blocking: false)

    execute_command('run_scenario', state)

  when 'android', 'ios'
    execute_command('run_scenario', state)

  when 'browser'
    # WebGL in a browser
    url = "http://localhost:#{Maze.config.document_server_port}/index.html"
    $logger.debug "Navigating to URL: #{url}"
    step("I navigate to the URL \"#{url}\"")
    execute_command('run_scenario', state)
  end
end


#
# Mobile steps
#

When('I wait for the mobile game to start') do
  # Wait for a fixed time period
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

When('I close and relaunch the Unity mobile app') do
  Maze.driver.close_app
  Maze.driver.launch_app
  # Wait for a fixed time period
  sleep 3
end

When('I run the {string} mobile scenario') do |scenario|
  step("I run the game in the \"#{scenario}\" state")
end

#
# Desktop steps
#
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

Then('the error is valid for the error reporting API sent by the native Unity notifier') do
  # This step currently only applies to native errors on macOS macOS
  check_error_reporting_api 'Unity Bugsnag Notifier'
end

Then('the error is valid for the error reporting API sent by the Unity notifier') do

  os = if Maze.config.farm == :bs
    # Mobile - could be ios or android
    Maze.config.capabilities['os']
  else
    # Could be windows or macos
    Maze.config.os
  end

  notifier_name = case os
                  when 'ios'
                    'Unity Bugsnag Notifier'
                  when 'android'
                    'Android Bugsnag Notifier'
                  else
                    'Unity Bugsnag Notifier'
                  end

  check_error_reporting_api notifier_name
end

Then('the stack frame methods should match:') do |expected_values|
  stacktrace = Maze::Helper.read_key_path(Maze::Server.errors.current[:body], 'events.0.exceptions.0.stacktrace')
  expected_frame_values = expected_values.raw

  flunk('The stacktrace is empty') if stacktrace.length == 0
  flunk('The stacktrace is not long enough') if stacktrace.length < expected_frame_values.length

  methods = stacktrace.map { |item| item['method'] }
  method_index = 0

  expected_frame_values.each do |expected_frames|
    frame_matches = false
    until frame_matches || method_index.eql?(methods.size)
      method = methods[method_index]
      frame_matches = expected_frames.any? { |frame| frame == method }
      method_index += 1
    end
    Maze.check.true(frame_matches, "None of the methods match the expected frames #{expected_frames}")
  end
end

Then('the current error request events match one of:') do |table|
  events = Maze::Server.errors.all.map do |error|
    Maze::Helper.read_key_path(error[:body], 'events')
  end.flatten
  table.hashes.each do |values|
    Maze.check.not_nil(events.detect do |event|
      handled_count = Maze::Helper.read_key_path(event, 'session.events.handled')
      unhandled_count = Maze::Helper.read_key_path(event, 'session.events.unhandled')
      message = Maze::Helper.read_key_path(event, 'exceptions.0.message')
      handled_count == values['handled'].to_i &&
        unhandled_count == values['unhandled'].to_i &&
        message == values['message']
    end, "No event matches the following values: #{values}")
  end
end

Then('the event {string} matches one of:') do |path, table|
  payload_value = Maze::Helper.read_key_path(Maze::Server.errors.current[:body], "events.0.#{path}")
  valid_values = table.raw.flat_map { |e| e }
  Maze.check.true(valid_values.any? { |frame| frame == payload_value }, "Value #{payload_value} did not match any of the expected values")
end

Then("custom metadata is included in the event") do
  steps %Q{
    Then the event "metaData.app.buildno" equals "0.1"
    And the event "metaData.app.cache" is null
    And the event "metaData.init" is null
    And the event "metaData.custom.letter" equals "QX"
    And the event "metaData.custom.better" equals 400
    And the event "metaData.test.test1" equals "test1"
    And the event "metaData.test.test2" is null
    And the error payload field "events.0.metaData.custom.int-array" is a non-empty array
    And the error payload field "events.0.metaData.custom.string-array" is a non-empty array
    And the error payload field "events.0.metaData.custom.dict" is not null
  }
end

When("I clear any error dialogue") do
  click_if_present 'android:id/button1'
  click_if_present 'android:id/aerr_close'
  click_if_present 'android:id/aerr_restart'
end

def click_if_present(element)
  return false unless Maze.driver.wait_for_element(element, 1)

  Maze.driver.click_element_if_present(element)
rescue Selenium::WebDriver::Error::UnknownError
  # Ignore Appium errors (e.g. during an ANR)
  return false
end

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
    win_log = File.join(Dir.pwd, 'mazerunner.log')
    command = "#{Maze.config.app} --args -logfile #{win_log}"
    Maze::Runner.run_command(command, blocking: false)
    execute_command('clear_cache')

  when 'android', 'ios'
    # TODO: Come back to this

  when 'webgl'
    url = "http://localhost:#{Maze.config.document_server_port}/index.html"
    $logger.debug "Navigating to URL: #{url}"
    step("I navigate to the URL \"#{url}\"")
    execute_command('clear_cache')

  when 'switch'

    # TODO. Example command: RunOnTarget.exe 0x01004B9000490000 --no-wait -- --mazeIp 192.168.0.11 --cacheType i --cacheIndex 3 --mountName BugsnagCache
    `RunOnTarget.exe #{Maze.config.app} --no-wait -- --mazeIp ADD_ARG_HERE --cacheType ADD_ARG_HERE --cacheIndex ADD_ARG_HERE --mountName ADD_ARG_HERE`

    execute_command('clear_cache')

  else
    raise "Platform #{platform} has not been considered"
  end
end

When('I close the Unity app') do
  case Maze::Helper.get_current_platform
  when 'macos','webgl','windows','switch'
    execute_command('close_application')
  when 'android', 'ios'
    # TODO: Come back to this
  end
end

When('I run the game in the {string} state') do |state|
  platform = Maze::Helper.get_current_platform
  case platform
  when 'macos'
    # Call executable directly rather than use open, which flakes on CI
    log = File.join(Dir.pwd, 'mazerunner.log')
    command = "#{Maze.config.app}/Contents/MacOS/Mazerunner"
    Maze::Runner.run_command(command, blocking: false)

    execute_command('run_scenario', state)

  when 'windows'
    win_log = File.join(Dir.pwd, 'mazerunner.log')
    command = "#{Maze.config.app} --args -logfile #{win_log}"
    Maze::Runner.run_command(command, blocking: false)

    execute_command('run_scenario', state)

  when 'android', 'ios'
    # TODO: Come back to this

  when 'browser'
    # WebGL in a browser
    url = "http://localhost:#{Maze.config.document_server_port}/index.html"
    $logger.debug "Navigating to URL: #{url}"
    step("I navigate to the URL \"#{url}\"")
    execute_command('run_scenario', state)

  when 'switch'

    # TODO. Example command: RunOnTarget.exe 0x01004B9000490000 --no-wait -- --mazeIp 192.168.0.11 --cacheType i --cacheIndex 3 --mountName BugsnagCache
    `RunOnTarget.exe #{Maze.config.app} --no-wait -- --mazeIp ADD_ARG_HERE --cacheType ADD_ARG_HERE --cacheIndex ADD_ARG_HERE --mountName ADD_ARG_HERE`

    execute_command('run_scenario', state)

  else
    raise "Platform #{platform} has not been considered"
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
      "Android Persistence Directory" => 18,
      "Disabled Release Stage" => 19,
      "Enabled Release Stage" => 20,
      "Java Background Crash No Threads" => 21,
      "iOS Native Error" => 22,
      "iOS Native Error No Threads" => 23,
      "Mark Launch Complete" => 24,
      "Check Last Run Info" => 25,
      "Native Event Callback" => 26,
      "Ios Signal" => 27,
      "Session Callback" => 28,
      "On Send Native Callback" => 29,
      "Inf Launch Duration" => 30,
      "Clear Metadata" => 31,
      "Set User In Config Csharp error" => 32,
      "Set User In Config Native Crash" => 33,
      "Set User After Init Csharp Error" => 34,
      "Set User After Init Native Error" => 35,
      "Set User After Init NDK Error" => 36,
      "Feature Flags In Config" => 37,
      "Feature Flags After Init" => 38,
      "Feature Flags After Init Clear All" => 39,
      "Feature Flags In Callback" => 40,
      "Clear Feature Flags In Callback" => 41,
      "Max Reported Threads" => 42,
      "Persist" => 43,
      "Persist Report" => 44,
      "Breadcrumb Null Metadata Value" => 45,
      "Launch Exception Session" => 46,

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

When('I run the {string} command') do |command|
  dial_number_for command
  step('I press Run Command')
end

When('I run the {string} mobile scenario') do |scenario|
  dial_number_for scenario
  step('I press Run Scenario')
end

When('I dial {int}') do |number|
  $logger.debug "Dialling #{number}"
  press_at 40 + (number * 80)
  sleep 1
end

When('I press Run Scenario') do
  press_at 840
end

When('I press Run Command') do
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

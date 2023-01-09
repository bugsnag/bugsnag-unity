require 'cgi'

When('On Mobile I relaunch the app') do
  next unless %w[android ios].include? Maze::Helper.get_current_platform 
  Maze.driver.launch_app
  sleep 3
end

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
    execute_command('clear_cache')
  when 'browser'
    url = "http://localhost:#{Maze.config.document_server_port}/index.html"
    $logger.debug "Navigating to URL: #{url}"
    step("I navigate to the URL \"#{url}\"")
    execute_command('clear_cache')

  when 'switch'
    switch_run_on_target
    execute_command('clear_cache')

  else
    raise "Platform #{platform} has not been considered"
  end

  sleep 2

end

When('I wait for requests to persist') do
  sleep 1
end

When('I close the Unity app') do
  execute_command('close_application')
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
    execute_command('run_scenario', state)

  when 'browser'
    # WebGL in a browser
    url = "http://localhost:#{Maze.config.document_server_port}/index.html"
    $logger.debug "Navigating to URL: #{url}"
    step("I navigate to the URL \"#{url}\"")
    execute_command('run_scenario', state)

  when 'switch'

    switch_run_on_target
    execute_command('run_scenario', state)

  else
    raise "Platform #{platform} has not been considered"
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

  methods = stacktrace.map { |item| item['method'] }

  expected_frame_values.each do |expected_frames|
    method_index = 0
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


#    And the event "metaData.custom.long" equals 12345678901234567890 should be readded after PLAT-9426

Then("custom metadata is included in the event") do
  steps %Q{
    Then the event "metaData.custom.int" equals 123
    And the event "metaData.custom.float" equals 123.123 to 3 decimal places
    And the event "metaData.custom.double" equals 123.456 to 3 decimal places
    And the event "metaData.custom.stringArray.0" equals "1"
    And the event "metaData.custom.emptyStringArray.0" is null
    And the event "metaData.custom.intList.2" equals 3
    And the event "metaData.custom.intArray.1" equals 5
    And the event "metaData.custom.stringDict.hello" equals "goodbye"
    And the event "metaData.clearMe" is null
    And the event "metaData.test.test1" equals "test2"
    And the event "metaData.test.nullMe" is null
    And the event "metaData.app.extra" equals "inApp"
    And the event "metaData.device.extra" equals "inDevice"
  }
end

Then("feature flags are included in the event") do
  steps %Q{
    And the event "featureFlags.0.featureFlag" equals "flag1"
    And the event "featureFlags.0.variant" equals "variant1"
    And the event "featureFlags.1.featureFlag" equals "flag3"
    And the event "featureFlags.1.variant" equals "variant3"
    And the event "featureFlags.2" is null
  }
end

Then("all possible parameters have been edited in a callback") do
  steps %Q{

    # Device metadata
    And the event "device.osName" equals "OsName"
    And the event "device.osVersion" equals "OsVersion"
    And the event "device.id" equals "Id"
    And the event "device.model" equals "Model"
    And the event "device.orientation" equals "Orientation"
    And the event "device.manufacturer" equals "Manufacturer"
    And the event "device.freeDisk" equals 123
    And the event "device.freeMemory" equals 456
    And the event "device.jailbroken" is true
    And the event "device.locale" equals "Locale"

    # App metadata
    And the event "app.id" equals "Id"
    And the event "app.releaseStage" equals "ReleaseStage"
    And the event "app.type" equals "Type"
    And the event "app.version" equals "Version"
    And the event "app.bundleVersion" equals "BundleVersion"
    And the event "app.binaryArch" equals "BinaryArch"
    And the event "app.codeBundleId" equals "CodeBundleId"
    And the event "app.dsymUuid" equals "DsymUuid"
    And the event "app.inForeground" is false
    And the event "app.isLaunching" is false

    # Exception data
    And the event "exceptions.0.errorClass" equals "ErrorClass"
    And the event "exceptions.0.stacktrace.0.method" equals "Method"
    And the event "exceptions.0.stacktrace.0.lineNumber" equals 22

    # Breadcrumbs
    And the event "breadcrumbs.0.type" equals "request"
    And the event "breadcrumbs.0.name" equals "Custom Message"
    And the event "breadcrumbs.0.metaData.test" equals "test"

  # Feature flags
    And the event "featureFlags.0.featureFlag" equals "fromCallback"
    And the event "featureFlags.0.variant" equals "a"

    # Metadata
    And the event "metaData.test1.test" equals "test"
    And the event "metaData.test2" is null
  }
end

Then("all possible parameters have been edited in a session callback") do
  steps %Q{

    And the session payload field "sessions.0.id" equals "Custom Id"
    And the session payload field "sessions.0.startedAt" matches the regex "1985-08-21T01:01:01(.000)?Z"
    And the session payload field "sessions.0.user.id" equals "1"
    And the session payload field "sessions.0.user.email" equals "2"
    And the session payload field "sessions.0.user.name" equals "3"


    # Device metadata
    And the session payload field "device.osName" equals "OsName"
    And the session payload field "device.osVersion" equals "OsVersion"
    And the session payload field "device.id" equals "Id"
    And the session payload field "device.model" equals "Model"
    And the session payload field "device.manufacturer" equals "Manufacturer"
    And the session payload field "device.jailbroken" is true
    And the session payload field "device.locale" equals "Locale"

    # App metadata
    And the session payload field "app.id" equals "Id"
    And the session payload field "app.releaseStage" equals "ReleaseStage"
    And the session payload field "app.type" equals "Type"
    And the session payload field "app.version" equals "Version"
    And the session payload field "app.binaryArch" equals "BinaryArch"
    And the session payload field "app.codeBundleId" equals "CodeBundleId"
  }
end

Then("expected device metadata is included in the event") do
  steps %Q{
    And the event "device.id" is not null
    And the event "device.locale" is not null
    And the event "device.model" is not null
    And the event "device.osName" is not null
    And the event "device.osVersion" is not null
    And the event "device.runtimeVersions" is not null
    And the event "device.time" is a timestamp
    And the event "device.totalMemory" is not null
    And the event "metaData.device.screenDensity" is not null
    And the event "metaData.device.screenResolution" is not null
    And the event "metaData.device.osLanguage" equals "English"
    And the event "metaData.device.graphicsDeviceVersion" is not null
    And the event "metaData.device.graphicsMemorySize" is not null
    And the event "metaData.device.processorType" is not null
  }
end

Then("expected app metadata is included in the event") do
  steps %Q{
    And the event "app.duration" is greater than 0
    And the event "app.durationInForeground" is not null
    And the event "app.inForeground" is not null
    And the event "app.isLaunching" is not null
    And the event "app.releaseStage" is not null
    And the event "app.type" is not null
    And the event "app.version" is not null
    And the event "metaData.app.companyName" equals "bugsnag"
    And the event "metaData.app.name" equals "Mazerunner"
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

def switch_run_on_target
  # Maze IP must always be provided
  maze_ip_arg = "--mazeIp #{ENV['SWITCH_MAZE_IP']}"

  # Other args are optional
  cache_type_arg = $switch_cache_type ? "--cacheType #{$switch_cache_type}" : ''
  cache_index_arg = $switch_cache_index ? "--cacheIndex #{$switch_cache_index}" : ''
  cache_mount_name_arg = $switch_cache_mount_name ? "--cacheMountName #{$switch_cache_mount_name}" : ''

  command = "RunOnTarget.exe #{Maze.config.app} --no-wait -- #{maze_ip_arg} #{cache_type_arg} #{cache_index_arg} " \
              "#{cache_mount_name_arg}"
  Maze::Runner.run_command(command)
end

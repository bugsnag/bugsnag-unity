require 'fileutils'

Before('@skip_unity_2018') do |_scenario|
  if ENV['UNITY_VERSION']
    unity_version = ENV['UNITY_VERSION'][0..3].to_i
    if unity_version == 2018
      skip_this_scenario('Skipping scenario on Unity 2018')
    end
  end
end


Before('@skip_webgl') do |_scenario|
  skip_this_scenario('Skipping scenario') unless Maze.config.browser.nil?
end

Before('@webgl_only') do |_scenario|
  skip_this_scenario('Skipping scenario') if Maze.config.browser.nil?
end


Before('@skip_macos') do |_scenario|
  skip_this_scenario("Skipping scenario") if Maze.config.os == 'macos'
end

Before('@macos_only') do |_scenario|
  skip_this_scenario('Skipping scenario') unless Maze.config.os == 'macos'
end


Before('@cocoa_only') do |_scenario|
  skip_this_scenario('Skipping scenario') unless Maze.config.os == 'macos' || Maze::Helper.get_current_platform == 'ios'
end

Before('@skip_cocoa') do |_scenario|
  skip_this_scenario("Skipping scenario") if Maze.config.os == 'macos' || Maze::Helper.get_current_platform == 'ios'
end


Before('@windows_only') do |_scenario|
  skip_this_scenario('Skipping scenario') unless Maze.config.os == 'windows'
end
Before('@skip_windows') do |_scenario|
  skip_this_scenario("Skipping scenario") if Maze.config.os == 'windows'
end


Before('@switch_only') do |_scenario|
  skip_this_scenario('Skipping scenario') unless Maze.config.os == 'switch'
end

Before('@skip_android') do |_scenario|
  skip_this_scenario("Skipping scenario") if Maze::Helper.get_current_platform == 'android'
end


BeforeAll do
  $api_key = 'a35a2a72bd230ac0aa0f52715bbdc6aa'
  Maze.config.enforce_bugsnag_integrity = false

  if Maze.config.os&.downcase == 'macos'
    # The default macOS Crash Reporter "#{app_name} quit unexpectedly" alert grabs focus which can cause tests to flake.
    # This option, which appears to have been introduced in macOS 10.11, displays a notification instead of the alert.
    Maze::Runner.run_command('defaults write com.apple.CrashReporter UseUNC 1')
  elsif Maze.config.os&.downcase == 'windows'
    # Allow the necessary environment variables to be passed from Ubuntu (under WSL) to the Windows test fixture
    ENV['WSLENV'] = 'BUGSNAG_SCENARIO:BUGSNAG_APIKEY:MAZE_ENDPOINT'
  elsif Maze.config.browser != nil # WebGL
    Maze.config.document_server_root = 'features/fixtures/maze_runner/build/WebGL/Mazerunner'
  elsif Maze.config.os&.downcase == 'switch'
    maze_ip = ENV['SWITCH_MAZE_IP']
    raise 'SWITCH_MAZE_IP must be set' unless maze_ip

    cache_type = ENV['SWITCH_CACHE_TYPE']
    case cache_type
    when nil, 'r'
      $logger.info 'Running tests for regular cache'
    when 'i'
      $logger.info 'Running tests for indexed cache'
      $switch_cache_type = 'i'
      $switch_cache_index = 3
      $switch_cache_mount_name = 'BugsnagCache'
    else
      raise "SWITCH_CACHE_TYPE must be 'r', or 'i', given: #{cache_type}"
    end


  elsif Maze.config.device.nil?
    raise '--browser (WebGL), --device (for Android/iOS) or --os (for desktop or switch) option must be set'
  end
end

Maze.hooks.before do
  if Maze.config.os == 'macos'
    support_dir = File.expand_path '~/Library/Application Support/com.bugsnag.Bugsnag'
    $logger.info "Clearing #{support_dir}"
    FileUtils.rm_rf(support_dir)
    $logger.info 'Clearing User defaults'
    Maze::Runner.run_command('defaults delete com.bugsnag.Mazerunner');
    Maze::Runner.run_command('defaults write com.bugsnag.Mazerunner ApplePersistenceIgnoreState YES');

    # This is to get around a strange macos bug where clearing prefs does not work 
    $logger.info 'Killing defaults service'
    Maze::Runner.run_command("killall -u #{ENV['USER']} cfprefsd")
  end
end

After do |scenario|
  next if scenario.status == :skipped

  case Maze::Helper.get_current_platform
  when 'macos'
    `killall Mazerunner`
  when 'webgl','windows'
    execute_command('close_application')
  when 'switch'
    # Terminate the app
    Maze::Runner.run_command('ControlTarget.exe terminate')
  end
end

AfterAll do
  case Maze::Helper.get_current_platform
  when 'macos'
    app_name = Maze.config.app.gsub /\.app$/, ''
    Maze::Runner.run_command("log show --predicate '(process == \"#{app_name}\")' --style syslog --start '#{Maze.start_time}' > #{app_name}.log")
  end
end

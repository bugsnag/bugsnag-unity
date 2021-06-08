Maze.config.enforce_bugsnag_integrity = false

Before('@skip_unity_android_2020') do |_scenario|
  if Maze.driver.capabilities['os'] == 'android' && ENV['UNITY_VERSION'] && ENV['UNITY_VERSION'].include?('2020')
    skip_this_scenario('Skipping Unity 2020 on Android, see PLAT-6645')
  end
end

When("I wait for the game to start") do
  # Wait for a fixed time period
  sleep 10
end

When('I relaunch the Unity app') do
  Maze.driver.launch_app
  # Wait for a fixed time period
  sleep 10
end

When("I tap the {string} button") do |button|
  # TODO: Better screen res support. Uses 1920 by default, or 2160 for ANDROID_9_0 device (Google Pixel 3)
  middle = 1920 / 2
  if Maze.config.device == "ANDROID_9_0"
    middle = 2160 / 2
  end
  case button
  when "throw Exception"
    press_at 540, middle - 750
  when "Assertion failure"
    press_at 540, middle - 650
  when "Native exception"
    press_at 540, middle - 550
  when "Log caught exception"
    press_at 540, middle - 450
  when "Log with class prefix"
    press_at 540, middle - 350
  when "Notify caught exception"
    press_at 540, middle - 250
  when "Notify with callback"
    press_at 540, middle - 150
  end
end

def press_at(x, y)
  STDOUT.puts "tap #{x}. #{y}"
  touch_action = Appium::TouchAction.new
  touch_action.tap({:x => x, :y => y})
  touch_action.perform
end

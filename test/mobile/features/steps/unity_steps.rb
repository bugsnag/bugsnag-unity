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
  viewport = Maze.driver.session_capabilities['viewportRect']
  center = viewport['width'] / 2

  case button
  when "throw Exception"
    press_at center, 50
  when "Log error"
    press_at center, 150
  when "Native exception"
    press_at center, 250
  when "Log caught exception"
    press_at center, 350
  when "NDK signal"
    press_at center, 450
  when "Notify caught exception"
    press_at center, 550
  when "Notify with callback"
    press_at center, 650
  when "Change scene"
    press_at center, 750
  end
end

def press_at(x, y)
  STDOUT.puts "tap: #{x},#{y}"

  touch_action = Appium::TouchAction.new
  touch_action.tap({:x => x, :y => y})
  touch_action.perform
end

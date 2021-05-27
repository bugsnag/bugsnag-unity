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
  width = viewport['left'] + viewport['width']
  height = viewport['top'] + viewport['height']

  STDOUT.puts "width: #{width}"
  STDOUT.puts "height: #{height}"

  center = width / 2

  case button
  when "throw Exception"
    press_at center, height - 750
  when "Assertion failure"
    press_at center, height - 650
  when "Native exception"
    press_at center, height - 550
  when "Log caught exception"
    press_at center, height - 450
  when "NDK signal"
    press_at center, height - 350
  when "Notify caught exception"
    press_at center, height - 250
  when "Notify with callback"
    press_at center, height - 150
  when "Change scene"
    press_at center, height - 50
  end
end

def press_at(x, y)
  STDOUT.puts "tap: #{x},#{y}"

  touch_action = Appium::TouchAction.new
  touch_action.tap({:x => x, :y => y})
  touch_action.perform
end

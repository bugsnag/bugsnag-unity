When("I wait for the game to start") do
  # Wait for a fixed time period
  sleep 10
end

When('I relaunch the Unity app') do
  Maze.driver.launch_app
  # Wait for a fixed time period
  sleep 10
end

When("I tap all buttons") do

  press_at 300, 25
  press_at 300, 75
  # press_at 300, 125
  press_at 300, 175
  # press_at 300, 225
  press_at 300, 275
  press_at 300, 325

end

When("I tap the {string} button") do |button|
  if STDOUT.puts Maze.config.capabilities.nil?
    STDOUT.puts "Please press #{button} for me"

    next
  end

  # Ensure we tap in the button
  viewport = Maze.driver.session_capabilities['viewportRect']

  # TODO Figure out why the scale is different on iOS
  factor = if Maze.driver.capabilities['os'] == 'ios'
             0.5
           else
             1
           end

  center = viewport['width'] / (2 / factor)

  case button
  when "throw Exception"
    press_at center, 50 * factor
  when "Log error"
    press_at center, 150 * factor
  when "Native exception"
    press_at center, 250 * factor
  when "Log caught exception"
    press_at center, 350 * factor
  when "NDK signal"
    press_at center, 450 * factor
  when "Notify caught exception"
    press_at center, 550 * factor
  when "Notify with callback"
    press_at center, 650 * factor
  when "Change scene"
    press_at center, 750 * factor
  end
end

def press_at(x, y)

  STDOUT.puts "tap: #{x},#{y}"

  # Appium::TouchAction.new.press(x: x, y: y).wait(1).release.perform

  touch_action = Appium::TouchAction.new
  touch_action.tap({:x => x, :y => y})
  touch_action.perform
end

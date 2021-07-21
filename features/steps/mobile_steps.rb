When("I wait for the mobile game to start") do
  # Wait for a fixed time period
  # TODO: PLAT-6655 Remove the Unity splash screen so we don't have to wait so long
  sleep 5
end

When('I relaunch the Unity mobile app') do
  Maze.driver.launch_app
  # Wait for a fixed time period
  sleep 5
end

When("I run the {string} mobile scenario") do |scenario|
  # Ensure we tap in the button
  # viewport = Maze.driver.session_capabilities['viewportRect']
  #
  # center = viewport['width'] / 2
  # middle = viewport['height'] / 2

  press_at 100, 100
  sleep 1
  Maze.driver.send_keys(scenario)

end

def press_at(x, y)

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

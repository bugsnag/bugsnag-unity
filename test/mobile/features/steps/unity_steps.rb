When("I wait for the game to start") do
  # Wait for a fixed time period
  # TODO: PLAT-6655 Remove the Unity splash screen so we don't have to wait so long
  sleep 5
end

When('I relaunch the Unity app') do
  Maze.driver.launch_app
  # Wait for a fixed time period
  sleep 5
end

When("I tap the {string} button") do |button|
  # Ensure we tap in the button
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
  when "Disable Breadcrumbs"
    press_at center, 850
  when "Start SDK"
    press_at center, 950
  end
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

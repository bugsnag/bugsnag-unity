When("I wait for the game to start") do
  # Wait for a fixed time period
  sleep 10
end

When("I tap the {string} button") do |button|
  # TODO Currently specific to the ANDROID_9_0 device (Google Pixel 3), which has a screen resolution of 1080x2160
  middle = 2160 / 2
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

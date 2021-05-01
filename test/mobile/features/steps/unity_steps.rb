When("I say {string}") do |message|
  # input = STDIN.gets message
  STDOUT.puts "#{message} and hit enter"
  STDIN.gets
  puts 'Thanks'
end

When("I tap the {string} button") do |button|
  # Specific to the ANDROID_9_0 Pixel 3
  STDOUT.puts button
  middle = 2160 / 2
  case button
  when "throw Exception"
    STDOUT.puts "tap it"
    press_at 540, middle - 750
  end
end

def press_at(x, y)
  STDOUT.puts "tap #{x}. #{y}"
  touch_action = Appium::TouchAction.new
  touch_action.tap({:x => x, :y => y})
  touch_action.perform
end

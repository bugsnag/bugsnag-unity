When("I say {string}") do |message|
  # input = STDIN.gets message
  STDOUT.puts "#{message} and hit enter"
  STDIN.gets
  puts 'Thanks'
end

When("I tap the {string} button") do |button|
  case button
  when "throw Exception"
    tap 150, 150
  end
end

def tap(x, y)
  touch_action = Appium::TouchAction.new
  touch_action.tap({:x => x, :y => y})
  touch_action.perform
end

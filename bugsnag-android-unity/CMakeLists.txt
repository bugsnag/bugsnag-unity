cmake_minimum_required(VERSION 3.4.1)
add_library(bugsnag-unity SHARED src/main/jni/bugsnag_unity.c)

find_library(log-lib log)

target_link_libraries(bugsnag-unity ${log-lib})


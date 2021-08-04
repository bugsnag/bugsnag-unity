#include <jni.h>

extern "C" {

static void __attribute__((used)) somefakefunc(void) {}

int crash_write_read_only() {
  // Write to a read-only page
  volatile char *ptr = (char *)somefakefunc;
  *ptr = 0;

  return 5;
}

JNIEXPORT void JNICALL
Java_com_example_bugsnagcrashplugin_CrashHelper_raiseNdkSignal(JNIEnv* env, jclass clazz) {
  crash_write_read_only();
}

}

#include <jni.h>
#include <stdexcept>

bool __attribute__((noinline)) run_away(bool value) {
  if (value)
    throw new std::runtime_error("How about NO");

  return false;
}

extern "C" {

static void __attribute__((used)) somefakefunc(void) {}

int crash_write_read_only() {
    // Write to a read-only page
    volatile char *ptr = (char *)somefakefunc;
    *ptr = 0;

    return 5;
}

JNIEXPORT void JNICALL Java_com_example_lib_BugsnagCrash_NdkSignal(JNIEnv *env, jobject instance) {
    crash_write_read_only();
}

JNIEXPORT void JNICALL Java_com_example_lib_BugsnagCrash_NdkException(JNIEnv *env, jobject instance) {
    run_away(true);
}
}

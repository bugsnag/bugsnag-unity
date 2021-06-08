#include <jni.h>
#include <stdexcept>

extern "C" {

static void __attribute__((used)) somefakefunc(void) {}

int crash_write_read_only() {
    // Write to a read-only page
    volatile char *ptr = (char *)somefakefunc;
    *ptr = 0;

    return 5;
}

JNIEXPORT void JNICALL Java_com_example_lib_BugsnagCrash_raiseNdkSignal(JNIEnv *env, jobject instance) {
    crash_write_read_only();
}

JNIEXPORT void JNICALL Java_com_example_lib_BugsnagCrash_throwCppException(JNIEnv *env, jobject instance) {
    throw new std::runtime_error("C++ exception from Android");
}
}

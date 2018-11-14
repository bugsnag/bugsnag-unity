#include <jni.h>
#include <stdbool.h>

JavaVM *bsg_unity_JVM;

jint JNI_OnLoad(JavaVM *vm, void *reserved) {
  bsg_unity_JVM = vm;
  return JNI_VERSION_1_6;
}

#ifdef __cplusplus
extern "C" {
#endif
bool bsg_unity_isJNIAttached() {
  JNIEnv *env;
  int status = (*bsg_unity_JVM)->GetEnv(bsg_unity_JVM, (void **)&env, JNI_VERSION_1_6);

  return status == JNI_OK;
}

// Return true if attached to the JVM
JNIEXPORT jboolean JNICALL Java_com_bugsnag_android_unity_BugsnagUnity_isJNIAttached() {
  return bsg_unity_isJNIAttached();
}
#ifdef __cplusplus
}
#endif

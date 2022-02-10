#include <stdexcept>
#include <stdlib.h>

extern "C" {
  void RaiseCocoaSignal();
  void TriggerCocoaCppException();
  void TriggerCocoaAppHang();
  void ClearPersistentData();
}

void ClearPersistentData() {
    NSLog(@"Clear persistent data");
    [NSUserDefaults.standardUserDefaults removePersistentDomainForName:NSBundle.mainBundle.bundleIdentifier];
    NSString *appSupportDir = NSSearchPathForDirectoriesInDomains(NSApplicationSupportDirectory, NSUserDomainMask, YES).firstObject;
    NSString *rootDir = [appSupportDir stringByAppendingPathComponent:@"com.bugsnag.Bugsnag"];
    NSError *error = nil;
    if (![NSFileManager.defaultManager removeItemAtPath:rootDir error:&error]) {
        if (![error.domain isEqual:NSCocoaErrorDomain] && error.code != NSFileNoSuchFileError) {
            NSLog(@"%@", error);
        }
    }
}

void RaiseCocoaSignal() {
    NSLog(@"RaiseCocoaSignal");

    abort();
}

void TriggerCocoaCppException() {
    throw std::runtime_error("CocoaCppException");
}

void TriggerCocoaAppHang() {
    dispatch_async(dispatch_get_main_queue(), ^{
        sleep(10000);
    });
}


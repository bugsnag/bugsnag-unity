#include <stdexcept>
#include <stdlib.h>
#include <unistd.h>

#import <dispatch/dispatch.h>


extern "C" {
  void RaiseCocoaSignal();
  void TriggerCocoaCppException();
  void TriggerCocoaAppHang();
}

void RaiseCocoaSignal() {
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


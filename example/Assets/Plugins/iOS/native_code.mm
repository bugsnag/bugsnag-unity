#include <stdexcept>
#include <stdlib.h>

extern "C" {
  void RaiseCocoaSignal();
  void TriggerCocoaCppException();
  void TriggerCocoaAppHang();
}

void RaiseCocoaSignal() {
    abort();
}

void TriggerCocoaCppException() {
	throw "CocoaCppException";
}

void TriggerCocoaAppHang() {
    sleep(10000);
}

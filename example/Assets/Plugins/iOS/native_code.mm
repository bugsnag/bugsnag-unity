#include <stdexcept>
#include <stdlib.h>

extern "C" {
  void TriggerSignal();
  void TriggerCPPException();
  void Crash();
  void CrashInBackground();
}

void TriggerSignal() {
  abort();
}


void TriggerCPPException() {
  throw std::invalid_argument( "Oops!" );
}

void Crash() {
  [NSArray new][1];
}

void CrashInBackground() {
  dispatch_async(dispatch_get_global_queue(0, 0), ^{
    Crash();
  });
}

#include <stdexcept>
#include <stdlib.h>

extern "C" {
  void IosSignal();
}

void IosSignal() {
    abort();
}


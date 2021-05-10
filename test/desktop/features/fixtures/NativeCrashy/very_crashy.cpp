#include <signal.h>
#include <stdlib.h>

extern "C" void crashy_signal_runner(float num) {
  if (num > 2) {
    abort();
  }
}

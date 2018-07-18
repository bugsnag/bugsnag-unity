extern "C" {
  void Crash();
  void CrashInBackground();
}

void Crash() {
  [NSArray new][1];
}

void CrashInBackground() {
  dispatch_async(dispatch_get_global_queue(0, 0), ^{
    Crash();
  });
}

extern "C" {
  void RaiseCocoaSignal();
  void TriggerCocoaCppException();
  void TriggerCocoaOom();
  void TriggerCocoaAppHang();
}

void RaiseCocoaSignal() {
  [NSArray new][1];
}

void TriggerCocoaCppException() {
  // TODO implement me
}

void TriggerCocoaOom() {
  // TODO implement me
}

void TriggerCocoaAppHang() {
  // TODO implement me
}

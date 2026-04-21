// Stub implementations for example app DllImports
// These are only needed for test scenarios and should do nothing in release builds

extern "C" {
    void RaiseCocoaSignal() {
        // Stub - no-op for example builds
    }
    
    void TriggerCocoaCppException() {
        // Stub - no-op for example builds
    }
    
    void TriggerCocoaAppHang() {
        // Stub - no-op for example builds
    }
}

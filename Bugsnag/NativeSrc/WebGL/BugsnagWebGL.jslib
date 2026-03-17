mergeInto(LibraryManager.library, {
  BugsnagWebGL_Initialize: function() {
    // Initialize WebGL foreground/background state tracking and foreground duration
    if (!window.BugsnagWebGL) {
      var now = Date.now();
      window.BugsnagWebGL = {
        isInForeground: !document.hidden,
        totalForegroundMs: 0,
        lastChangeTime: now
      };
    }

    function bugsnagUpdateDuration() {
      var now = Date.now();
      if (window.BugsnagWebGL.isInForeground) {
        window.BugsnagWebGL.totalForegroundMs += now - window.BugsnagWebGL.lastChangeTime;
      }
      window.BugsnagWebGL.lastChangeTime = now;
    }

    function bugsnagNotifyStateChange(newState) {
      // Send state change message directly to C#
      var gameObjectName = "Bugsnag app lifecycle tracker";
      var methodName = "SetApplicationStateFromWebGL";
      var stateValue = newState ? "1" : "0";
      
      try {
        SendMessage(gameObjectName, methodName, stateValue);
      } catch (e) {
        // Silently ignore if not yet initialized
      }
    }

    // Listen for visibility change events
    document.addEventListener('visibilitychange', function() {
      bugsnagUpdateDuration();
      var newState = !document.hidden;
      window.BugsnagWebGL.isInForeground = newState;
      bugsnagNotifyStateChange(newState);
    });

    // Listen for blur/focus events as fallback
    window.addEventListener('blur', function() {
      bugsnagUpdateDuration();
      window.BugsnagWebGL.isInForeground = false;
      bugsnagNotifyStateChange(false);
    });

    window.addEventListener('focus', function() {
      bugsnagUpdateDuration();
      window.BugsnagWebGL.isInForeground = true;
      bugsnagNotifyStateChange(true);
    });

    // Listen for page visibility events
    window.addEventListener('pageshow', function() {
      bugsnagUpdateDuration();
      window.BugsnagWebGL.isInForeground = true;
      bugsnagNotifyStateChange(true);
    });

    window.addEventListener('pagehide', function() {
      bugsnagUpdateDuration();
      window.BugsnagWebGL.isInForeground = false;
      bugsnagNotifyStateChange(false);
    });
  },

  BugsnagWebGL_IsInForeground: function() {
    // Returns 1 if in foreground, 0 if in background
    if (!window.BugsnagWebGL) {
      // If not initialized, return based on document.hidden
      var inferred = document.hidden ? 0 : 1;
      return inferred;
    }
    var value = window.BugsnagWebGL.isInForeground ? 1 : 0;
    return value;
  },

  BugsnagWebGL_GetDurationInForegroundMs: function() {
    if (!window.BugsnagWebGL) {
      return 0;
    }
    var now = Date.now();
    var total = window.BugsnagWebGL.totalForegroundMs;
    if (window.BugsnagWebGL.isInForeground) {
      total += now - window.BugsnagWebGL.lastChangeTime;
    }
    // Keep non-negative for stability; return as a JS Number (double).
    if (total < 0) total = 0;
    return total;
  }
});


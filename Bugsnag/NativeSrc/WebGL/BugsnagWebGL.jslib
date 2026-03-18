mergeInto(LibraryManager.library, {
  BugsnagWebGL_Initialize: function() {
    var bugsnagWebGLKey = (typeof Symbol === 'function' && Symbol.for)
      ? Symbol.for('bugsnag.webgl')
      : '__bugsnag_webgl__';

    var bugsnagWebGL = window[bugsnagWebGLKey];

    // Initialize WebGL foreground/background state tracking and foreground duration
    if (!bugsnagWebGL) {
      var now = Date.now();
      bugsnagWebGL = {
        isInForeground: !document.hidden,
        totalForegroundMs: 0,
        lastChangeTime: now,
        listenersRegistered: false
      };
      window[bugsnagWebGLKey] = bugsnagWebGL;
    }

    // Guard against duplicate listener registration
    if (bugsnagWebGL.listenersRegistered) {
      return;
    }
    bugsnagWebGL.listenersRegistered = true;

    function bugsnagUpdateDuration() {
      var now = Date.now();
      if (bugsnagWebGL.isInForeground) {
        bugsnagWebGL.totalForegroundMs += now - bugsnagWebGL.lastChangeTime;
      }
      bugsnagWebGL.lastChangeTime = now;
    }

    function bugsnagNotifyStateChange(newState) {
      // Send state change message directly to C#
      var gameObjectName = 'Bugsnag app lifecycle tracker';
      var methodName = 'SetApplicationStateFromWebGL';
      var stateValue = newState ? '1' : '0';

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
      bugsnagWebGL.isInForeground = newState;
      bugsnagNotifyStateChange(newState);
    });

    // Listen for blur/focus events as fallback
    window.addEventListener('blur', function() {
      bugsnagUpdateDuration();
      bugsnagWebGL.isInForeground = false;
      bugsnagNotifyStateChange(false);
    });

    window.addEventListener('focus', function() {
      bugsnagUpdateDuration();
      var newState = !document.hidden;
      bugsnagWebGL.isInForeground = newState;
      bugsnagNotifyStateChange(newState);
    });

    // Listen for page visibility events
    window.addEventListener('pageshow', function() {
      bugsnagUpdateDuration();
      var newState = !document.hidden;
      bugsnagWebGL.isInForeground = newState;
      bugsnagNotifyStateChange(newState);
    });

    window.addEventListener('pagehide', function() {
      bugsnagUpdateDuration();
      bugsnagWebGL.isInForeground = false;
      bugsnagNotifyStateChange(false);
    });
  },

  BugsnagWebGL_IsInForeground: function() {
    var bugsnagWebGLKey = (typeof Symbol === 'function' && Symbol.for)
      ? Symbol.for('bugsnag.webgl')
      : '__bugsnag_webgl__';
    var bugsnagWebGL = window[bugsnagWebGLKey];

    // Returns 1 if in foreground, 0 if in background
    if (!bugsnagWebGL) {
      // If not initialized, return based on document.hidden
      var inferred = document.hidden ? 0 : 1;
      return inferred;
    }
    var value = bugsnagWebGL.isInForeground ? 1 : 0;
    return value;
  },

  BugsnagWebGL_GetDurationInForegroundMs: function() {
    var bugsnagWebGLKey = (typeof Symbol === 'function' && Symbol.for)
      ? Symbol.for('bugsnag.webgl')
      : '__bugsnag_webgl__';
    var bugsnagWebGL = window[bugsnagWebGLKey];

    if (!bugsnagWebGL) {
      return 0;
    }
    var now = Date.now();
    var total = bugsnagWebGL.totalForegroundMs;
    if (bugsnagWebGL.isInForeground) {
      total += now - bugsnagWebGL.lastChangeTime;
    }
    // Keep non-negative for stability; return as a JS Number (double).
    if (total < 0) total = 0;
    return total;
  }
});

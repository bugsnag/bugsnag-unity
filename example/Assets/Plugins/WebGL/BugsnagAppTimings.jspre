(function () {

    /*
     * This class holds and computes some values relating to the how long the app
     * has been running, and how long it spent in the foreground of the device.
     */
    function AppTimings() {
        var now = new Date()
        this.start = now;
        this.inForeground = true;
        this.lastEnteredForeground = now;
    }

    /*
     * Sets up listeners for the window#focus/blur events, which are fired when
     * the browser window/tab enters and leaves the foreground.
     */
    AppTimings.prototype.listen = function () {
      window.addEventListener('focus', this._onfocus.bind(this));
      window.addEventListener('blur', this._onblur.bind(this));
    }

    AppTimings.prototype._onfocus = function () {
      this.inForeground = true;
      this.lastEnteredForeground = new Date();
    }

    AppTimings.prototype._onblur = function () {
        this.inForeground = false;
    }

    /*
     * Generate a summary of the data held which matches the `app` portion of the
     * payload for the Bugsnag error reporting API:
     *
     *    { duration: <int>, inForeground: <bool>, durationInForeground: <int>  }
     *
     */
    AppTimings.prototype.summary = function () {
      var now = new Date()
      return {
        inForeground: this.inForeground,
        duration: now - this.start,
        durationInForeground: this.inForeground ? now - this.lastEnteredForeground : 0
      }
    }

    // Since the class adds global event handlers, we only want one instantiation
    // of it, so the constructor is hidden in this closure, and one instance is exposed
    // on the window as `__bugsnag__app_timings__`
    var appTimings = new AppTimings();
    appTimings.listen();
    window.__bugsnag__app_timings__ = appTimings;
}());

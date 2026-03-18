using System.Collections;
using UnityEngine;

public class WebGLDurationInForeground : Scenario
{
    private const double TargetDurationMs = 2147483648; // 2^31, larger than Int32.MaxValue

    public override void Run()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        EnsureWebGLForegroundDuration((long)TargetDurationMs);
        StartCoroutine(NotifyNextFrame());
#else
        DoSimpleNotify("WebGLDurationInForeground");
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    private static void EnsureWebGLForegroundDuration(long totalForegroundMs)
    {
        // Keep the page in the background state so the computed value is stable and equals totalForegroundMs.
        // We set lastChangeTime to Date.now() to avoid additional time being added.
        var js = $@"
(function() {{
  var now = Date.now();
  var bugsnagWebGLKey = (typeof Symbol === 'function' && Symbol.for)
    ? Symbol.for('bugsnag.webgl')
    : '__bugsnag_webgl__';
  
  var bugsnagWebGL = window[bugsnagWebGLKey];
  if (!bugsnagWebGL) {{
    bugsnagWebGL = {{ isInForeground: false, totalForegroundMs: 0, lastChangeTime: now, listenersRegistered: false }};
    window[bugsnagWebGLKey] = bugsnagWebGL;
  }}
  bugsnagWebGL.isInForeground = false;
  bugsnagWebGL.totalForegroundMs = {totalForegroundMs};
  bugsnagWebGL.lastChangeTime = now;
}})();";

#pragma warning disable CS0618
        Application.ExternalEval(js);
#pragma warning restore CS0618
    }

    private IEnumerator NotifyNextFrame()
    {
        yield return null;
        DoSimpleNotify("WebGLDurationInForeground");
    }
#endif
}

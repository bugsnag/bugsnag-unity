using System.Collections;
using System.Collections.Generic;
#if UNITY_IOS || UNITY_STANDALONE_OSX 

using System.Runtime.InteropServices;
#endif
using UnityEngine;

public class Testing : MonoBehaviour
{
    // Start is called before the first frame update
    public void Throw()
    {
        throw new System.Exception("This is an exception");
    }

    public void DoTriggerCocoaCppException()
    {
#if UNITY_IOS
      TriggerCocoaCppException();
#endif

#if UNITY_STANDALONE_OSX
        crashy_signal_runner(8);
#endif
    }

#if UNITY_STANDALONE_OSX

    [DllImport("NativeCrashy")]
    private static extern void crashy_signal_runner(float num);

#endif

#if UNITY_IOS 
   [DllImport("__Internal")]
   private static extern void RaiseCocoaSignal();

   [DllImport("__Internal")]
   private static extern void TriggerCocoaCppException();

   [DllImport("__Internal")]
   private static extern void TriggerCocoaAppHang();
#endif
}

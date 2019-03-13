namespace UnityEngine.Events {
    public delegate void UnityAction();
    public delegate void UnityAction<T0> (T0 arg0);
    public delegate void UnityAction<T0, T1> (T0 arg0, T1 arg1);
    public delegate void UnityAction<T0, T1, T2> (T0 arg0, T1 arg1, T2 arg2);
    public delegate void UnityAction<T0, T1, T2, T3> (T0 arg0, T1 arg1, T2 arg2, T3 arg3);
}

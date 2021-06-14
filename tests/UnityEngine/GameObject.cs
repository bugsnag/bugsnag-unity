namespace UnityEngine
{
    public class Component
    {
    }
    public class GameObject
    {
        public GameObject(string name) { }
        public T AddComponent<T>() where T : Component
        {
            return null;
        }
    }
}

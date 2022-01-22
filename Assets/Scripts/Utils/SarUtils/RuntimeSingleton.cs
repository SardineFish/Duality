using UnityEngine;

namespace SardineFish.Utils
{
    public class RuntimeSingleton<T> : MonoBehaviour where T : RuntimeSingleton<T>
    {
        [UnityEngine.SerializeField]
        bool m_DontDestroyOnLoad = true;

        private static T instance = null;
        public static T Instance
        {
            get
            {
                return instance;
            }
        }

        public RuntimeSingleton()
        {
            instance = this as T;
        }

        protected virtual void Awake()
        {
            if (instance && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                if (m_DontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
                instance = this as T;
            }
        }
    }
}
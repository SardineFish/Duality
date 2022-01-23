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
            {
                if (m_DontDestroyOnLoad)
                {
                    if (instance && instance != this)
                    {
                        Destroy(gameObject);
                        return;
                    }
                    DontDestroyOnLoad(gameObject);
                }
                if (gameObject.activeInHierarchy)
                {
                    Debug.Log($"singleton {name} initialized");
                    instance = this as T;
                }
            }
        }

        protected virtual void Start()
        {
            Debug.Log($"singleton {name} initialized");
            instance = this as T;
        }
    }
}
using UnityEngine;

namespace SingletonEngine
{
    public abstract class Singleton<Type> : MonoBehaviour where Type : Component
    {
        [Tooltip("If true, the singleton instance will persist across scene loads. If false, it will be destroyed when a new scene is loaded.")]
        public bool dontDestroyOnLoad = true;

        protected static Type s_instance;

        public static Type instance 
        {
            get 
            {
                if (s_instance == null)
                {
                    EnsureInstance();
                }

                return s_instance;
            } 
        }

        #region Unity
        protected virtual void Awake()
        {
            Awake_Internal();
            DestroyAll_Internal();
        }

        protected virtual void OnDestroy()
        {
            if (s_instance != this)
            {
                return;
            }

            s_instance = null;
        }
        #endregion

        #region Internal
        private void Awake_Internal() 
        {
            if (s_instance == null)
            {
                s_instance = this as Type;

                if(dontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (instance.gameObject == this.gameObject)
                    return;

                if (dontDestroyOnLoad)
                    Destroy(gameObject);
            }
        }

        /// <summary>
        /// Ensure the singleton instance exists. This can be called from static code (e.g. from `instance` getter)
        /// and will attempt to find an existing instance in the scene or create a new GameObject with the component.
        /// </summary>
        private static void EnsureInstance()
        {
            if (s_instance != null)
                return;

            // Try to find an existing instance in the scene. For Unity 2020.1+ include inactive objects.
#if UNITY_2020_1_OR_NEWER
            s_instance = FindAnyObjectByType<Type>(FindObjectsInactive.Include);
#else
            s_instance = Object.FindObjectOfType<Type>(true);
#endif

            if (s_instance == null)
            {
                var go = new GameObject(typeof(Type).Name + " (Singleton)");
                s_instance = go.AddComponent<Type>();
            }
        }

        private void DestroyAll_Internal()
        {
            Behaviour[] instances;

#if UNITY_2020_1_OR_NEWER
            instances = FindObjectsByType<Behaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            instances = Object.FindObjectsOfType<Behaviour>(true);
#endif
            foreach (Behaviour instance in instances)
            {
                if(instance.gameObject != this.gameObject)
                {
                    Destroy(instance.gameObject);
                }
            }
        }
        #endregion
    }
}

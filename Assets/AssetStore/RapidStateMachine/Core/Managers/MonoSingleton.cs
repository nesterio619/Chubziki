using UnityEngine;

namespace RSM
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField] private SingletonType _singletonType;
        private static T _Instance;

        private void Awake()
        {
            if (!Application.isPlaying) return;
            if (_Instance != null)
            {
                if (_Instance.gameObject == gameObject) return;
                if (_singletonType == SingletonType.KeepNewest)
                {
                    Destroy(_Instance.gameObject);
                }
                else
                {
                    Destroy(gameObject);
                    return;
                }
            }

            _Instance = GetComponent<T>();
            DontDestroyOnLoad(this);
            Initialize();
        }

        protected static T Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindObjectOfType<T>();

                    if (_Instance == null)
                    {
                        GameObject newGameObject = new GameObject(typeof(T).Name);
                        _Instance = newGameObject.AddComponent<T>();
                    }
                }
                return _Instance;
            }
        }

        protected virtual void Initialize() { }

        private enum SingletonType
        {
            KeepOldest,
            KeepNewest
        }
    }
}
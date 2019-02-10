using System;
using UnityEngine;

namespace Project2018CodeSamples.Helpers
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static object _lock = new object();
        private static bool _applicationIsQuitting;

        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)                
                    return null;                

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (T) FindObjectOfType(typeof(T));                        
                        if (_instance == null)                        
                            CreateInstance();                           
                    }

                    return _instance;
                }
            }
        }

        private static void CreateInstance()
        {
            var singleton = new GameObject();
            _instance = singleton.AddComponent<T>();
            singleton.name = $"(singleton)  {typeof(T)}";
            DontDestroyOnLoad(singleton);
        }

        protected virtual void Awake() => DontDestroyOnLoad(this);
        protected void OnDestroy() => _applicationIsQuitting = true;
        protected void OnApplicationQuit() => _applicationIsQuitting = true;
    }
}
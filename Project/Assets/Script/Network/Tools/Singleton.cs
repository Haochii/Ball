using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BaseFramework
{
    /// <summary>
    /// 挂载在GameManagers身上的管理类单例对象
    /// </summary>
    /// <typeparam name="T">管理器类型</typeparam>
    public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        static T instance;

        private static bool applicationIsQuitting = false;

        private static readonly object _lock = new object();

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    return null;
                }

                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = CoroutineRunner.Instance.gameObject.GetComponent<T>();
                            if (instance == null)
                            {
                                instance = CoroutineRunner.Instance.gameObject.AddComponent<T>();
                            }
                        }
                    }
                }
                return instance;
            }
        }

        protected virtual void OnDestroy()
        {
            instance = null;
            applicationIsQuitting = true;
        }

        protected virtual void OnApplicationQuit()
        {
            instance = null;
            applicationIsQuitting = true;
        }
    }

    public class Singleton<T> where T : class, new()
    {
        static T instance;

        private static readonly object _lock = new object();

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                            instance = new T();
                    }
                }
                return instance;
            }
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineRunner : MonoBehaviour
{
    static CoroutineRunner instance;

    public static CoroutineRunner Instance
    {
        get
        {
            if (null == instance)
            {
                var go = new GameObject("_CoroutineRunner");                
                instance = go.AddComponent<CoroutineRunner>();
            }
            return instance;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

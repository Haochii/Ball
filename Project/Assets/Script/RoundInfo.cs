using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundInfo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public float timer = 2.0f; // 定时2秒
    private bool isShowing = false;
    public void ShowRoundInfo()
    {
        isShowing = true;
    }

    void Update()
    {
        if(isShowing)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                isShowing = false;
                GetComponent<Image>().enabled = false;
                timer = 2.0f;
            }
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundInfo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        isShowing = true;
    }
    public float timer = 2.0f; // 定时2秒
    public bool isShowing;
    public void ShowRoundInfo()
    {
        isShowing = true;
        GetComponent<Image>().enabled = true;
    }

    void Update()
    {
        
        if(isShowing)
        {
            bool isFading = false;
            Debug.Log(timer.ToString());
            timer -= Time.deltaTime;
            if (timer <=0.5 && !isFading)
            {
                GetComponent<Image>().CrossFadeAlpha(0, 0.3f, true);
                isFading = true;
            }
            if (timer <= 0)
            {
                isShowing = false;
                GetComponent<Image>().enabled = false;
                timer = 2.0f;
            }
        }
    }

}

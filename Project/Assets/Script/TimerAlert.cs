using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerAlert : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if((int)float.Parse(GetComponent<Text>().text) <= 3)
        {
            GetComponent<Text>().fontSize = 200;
            GetComponent<Text>().color = Color.red;
        }
        else
        {
            GetComponent<Text>().fontSize = 160;
            GetComponent<Text>().color = Color.white;
        }
    }
}

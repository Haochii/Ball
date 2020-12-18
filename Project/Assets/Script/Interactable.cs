using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool triggered;

    // 暂时只有即死区域，需要扩展
    void Start()
    {
        triggered = false;
    }

	private void OnTriggerEnter(Collider other)
	{
		if(other.transform.CompareTag("Player") && !triggered)
		{
			triggered = true;
			//other.gameObject.GetComponent<Ball>().Damage(999f);
		}
	}
}

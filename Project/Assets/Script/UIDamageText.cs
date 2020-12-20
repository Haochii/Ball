using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDamageText : MonoBehaviour
{
	public bool isDamage;
	public bool floating;
	public float floatSpeed = 100f;
	public Gradient damage;
	public Gradient heal;

	private float timer;
	private Text text;

	private void Start()
	{
		timer = 0f;
		floating = true;
		text = GetComponent<Text>();
	}

	void Update()
	{
		if (floating)
		{
			transform.position = new Vector3(transform.position.x, transform.position.y + Time.deltaTime * floatSpeed, transform.position.z);
			timer += Time.deltaTime;
			if(isDamage)
			{
				text.color = damage.Evaluate(timer);
			}
			else
			{
				text.color = heal.Evaluate(timer);
			}
		}

		if(timer > 1f)
		{
			Destroy(gameObject);
		}
	}
}

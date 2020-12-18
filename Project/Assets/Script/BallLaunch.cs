using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallLaunch : MonoBehaviour
{
	private Rigidbody2D rb;

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	public void Launch(float radian, float force)
	{
		rb.velocity = new Vector2(-Mathf.Cos(radian), -Mathf.Sin(radian)) * force * GetComponent<Ball>().launchSpeed;
	}
}

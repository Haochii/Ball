using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Deprecated, for now.
public class CameraFollow : MonoBehaviour
{
	public Transform target;
	public Vector3 offset = new Vector3(0f, 15f, 0f);
	[Range(0f, 1f)]
	public float moveSpeed = 0.5f;

	private Vector3 targetPos;
	/*
	void Start()
	{
		target = RoundManager.Instance.currentBall.transform;
		transform.position = target.position + offset;
	}
	*/
	void FixedUpdate()
	{
		/*
		target = RoundManager.Instance.currentBall.transform;
		targetPos = Vector3.Lerp(transform.position, target.position + offset, moveSpeed);
		transform.position = targetPos;
		*/
	}
}

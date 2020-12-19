using BaseFramework.Network;
using Newtonsoft.Json;
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

	public void Launch(int degree, int force)
	{
		if(!GameManager.Instance.isPlayerA)
		{
			degree += 180 * 10000;
		}
		float deg = (float)degree / 10000;
		float rad = deg * Mathf.Deg2Rad;
		float len = (float)force / 10000;

		rb.velocity = new Vector2(-Mathf.Cos(rad), -Mathf.Sin(rad)) * len * GetComponent<Ball>().launchSpeed;
		if (RoundManager.Instance.isCurrentPlayerA == GetComponent<Ball>().isPlayerA)
		{
			/*
			if(!GetComponent<Ball>().isPlayerA)
			{
				degree += 180;
			}
			*/
			//BallLaunchClass ballLaunchClass = new BallLaunchClass(GetComponent<Ball>().id, -1, force, degree + 180 * 10000);
			BallLaunchClass ballLaunchClass = new BallLaunchClass(GetComponent<Ball>().id, -1, force, degree);
			LoginRequist.ucl.rpcCall("play.id_ball_launch", JsonConvert.SerializeObject(ballLaunchClass), null);
		}
	}
}

class BallLaunchClass
{
	public BallLaunchClass(int bi, int si, int fo, int ra)
	{
		ball_id = bi;
		skill_id = si;
		force = fo;
		radian = ra;
	}
	public int ball_id;
	public int skill_id;
	public int force;
	public int radian;
}

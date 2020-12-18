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

	public void Launch(float radian, float force)
	{
		rb.velocity = new Vector2(-Mathf.Cos(radian), -Mathf.Sin(radian)) * force * GetComponent<Ball>().launchSpeed;
		if(RoundManager.Instance.isCurrentPlayerA == GetComponent<Ball>().isPlayerA)
		{
			BallLaunchClass ballLaunchClass = new BallLaunchClass(GetComponent<Ball>().id, -1, radian, force);
			LoginRequist.ucl.rpcCall("play.id_ball_launch", JsonConvert.SerializeObject(ballLaunchClass), null);
		}
	}
}

class BallLaunchClass
{
	public BallLaunchClass(int bi, int si, float fo, float ra)
	{
		ball_id = bi;
		skill_id = si;
		force = fo;
		radian = ra;
	}
	public int ball_id;
	public int skill_id;
	public float force;
	public float radian;
}

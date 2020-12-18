using BaseFramework.Network;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RoundManager : MonoBehaviour
{
	public static RoundManager Instance;

	public int round;
	public int movesLeft;
	public int maxMoves = 1;
	public float roundTime;
	public float countDown = 999f;
	public float roundInterval = 1f;
	public bool deploying;
	public bool firing;
	public bool isCurrentPlayerA;       //If the controlling player is player A.
										//public bool isPlayerA;			//If player himself is player A.
	public bool gameStop;
	public List<Ball> ballListA;
	public List<Ball> ballListB;
	public Ball current;
	public Sprite[] sprites;
	public Transform[] spawnPoints;
	public Color colorSelf;
	public Color colorEnemy;

	[HideInInspector]
	public UnityEvent nextRound;

	private bool spawnReady;
	private bool deployReady;
	private int curSpawnPoint;
	private bool ticking;   //If the round countdown is ticking.
	private bool waiting;   //If the interruption is going on after all the balls have stopped.
	private bool rolling;   //If there're still any balls rolling after a ball has been fired.
	private float timer;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	void Start()
	{
		curSpawnPoint = 0;
		countDown = roundTime;
		deploying = spawnReady = deployReady = false;
		rolling = false;
		waiting = false;
		//Initialize();
	}

	private void Update()
	{
		CheckTimer();

		if (deploying)
		{

		}

		if (waiting || firing)
		{
			timer += Time.deltaTime;
		}

		if (ticking)
		{
			countDown -= Time.deltaTime;
		}
		if (ticking && countDown <= 0f)
		{
			ticking = false;
			GameManager.Instance.uILaunch.Halt();
			GameManager.Instance.uIHUD.Halt();
			LoginRequist.ucl.rpcCall("play.round_over", null, null);
		}
	}

	public void Initialize()
	{
		gameStop = false;
		round = 0;
		isCurrentPlayerA = false;
		current = null;
		firing = false;

		//ballListA.Clear();
		//ballListB.Clear();
	}

	public void StartDeploy()
	{
		Initialize();
		//Start deploy countdown.
		deploying = true;
	}

	public void SelectSpawnPoint(int id)
	{
		if (!spawnReady)
		{
			spawnReady = true;
		}
		GameManager.Instance.uIHUD.spawnPoints[curSpawnPoint].color = GameManager.Instance.uIHUD.uncheckColor;
		curSpawnPoint = id;
		GameManager.Instance.uIHUD.spawnPoints[id].color = GameManager.Instance.uIHUD.checkColor;
	}

	public void PlaceBall(int type)
	{
		if (!spawnReady)
		{
			return;
		}
		List<Ball> list;
		list = GameManager.Instance.isPlayerA ? ballListA : ballListB;
		Ball ball = list[curSpawnPoint];
		ball.gameObject.SetActive(true);
		ball.transform.position = spawnPoints[curSpawnPoint].position;
		ball.type = type;
		ball.GetComponent<SpriteRenderer>().sprite = sprites[type];
		ball.GetComponent<SpriteRenderer>().color = colorSelf;

		deployReady = true;
		foreach (Ball b in list)
		{
			if (!b.gameObject.activeSelf)
			{
				deployReady = false;
				break;
			}
		}
		if (deployReady)
		{
			GameManager.Instance.uIHUD.deployButton.gameObject.SetActive(true);
		}
	}

	public void DeploySelf()
	{
		deploying = false;
		GameManager.Instance.uIHUD.footerDeploy.SetActive(false);
		GameManager.Instance.uIHUD.spawnPoints[0].transform.parent.gameObject.SetActive(false);

		//Send ready message.
		//Get server respond.

		List<Ball> list = GameManager.Instance.isPlayerA ? ballListA : ballListB;
		BallRdArr ballRdArr = new BallRdArr(list[0].type, list[1].type, list[2].type);
		LoginRequist.ucl.rpcCall("play.ball_ready", JsonConvert.SerializeObject(ballRdArr), null);
	}

	public void DeployEnemy(int[] types)
	{
		List<Ball> list = GameManager.Instance.isPlayerA ? ballListB : ballListA;
		for (int i = 0; i < list.Count; i++)
		{
			list[i].GetComponent<SpriteRenderer>().sprite = sprites[types[i]];
			list[i].GetComponent<SpriteRenderer>().color = colorEnemy;
			list[i].transform.position = spawnPoints[i + 3].position;
		}
	}

	public void GameStart()
	{
		//Deprecated. Replaced by manual override.
		/*
		Ball b;
		GameObject[] balls = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject ball in balls)
		{
			b = ball.GetComponent<Ball>();
			if (b.isPlayerA)
			{
				ballListA.Add(b);
			}
			else
			{
				ballListB.Add(b);
			}
		}
		*/
		List<Ball> list = GameManager.Instance.isPlayerA ? ballListB : ballListA;
		for (int i = 0; i < list.Count; i++)
		{
			list[i].gameObject.SetActive(true);
		}

		for (int i = 0; i < GameManager.Instance.uIHUD.healthHolder.childCount; i++)
		{
			GameManager.Instance.uIHUD.healthHolder.GetChild(i).gameObject.SetActive(true);
		}
		NextRound();
	}

	public void GetBall(int id)
	{
		if (id < 0)
		{
			current = null;
		}
		else
		{
			current = GameManager.Instance.isPlayerA ? ballListA[id] : ballListB[id];
			GameManager.Instance.uILaunch.ready = true;
			GameManager.Instance.uILaunch.buttonPressed = true;
		}
	}

	//All balls launched, switching players.
	public void NextRound()
	{

		//Server switching player.

		if (gameStop)
		{
			return;
		}

		isCurrentPlayerA = !isCurrentPlayerA;
		//movesLeft = maxMoves;
		//movesLeft = isCurrentPlayerA ? ballListA.Count : ballListB.Count;
		if (isCurrentPlayerA)
		{
			round++;
			if (round > 1)
			{
				nextRound.Invoke();
			}
		}

		countDown = roundTime;
		ticking = true;

		current = null;
		//movesLeft--;
		/*
		if (movesLeft < 0 || countDown <= 0f)
		{
			NextRound();
			return;
		}
		*/
		if (!isCurrentPlayerA)
		{
			//GetServerMsg(); //For local use only.
			return;
		}

		GameManager.Instance.uILaunch.active = true;
		GameManager.Instance.uILaunch.SwitchButton(true);
	}

	public void CheckBallList()
	{
		if (ballListA.Count == 0)
		{
			GameManager.Instance.Win(false);
		}
		else if (ballListB.Count == 0)
		{
			GameManager.Instance.Win(true);
		}
	}

	private void CheckTimer()
	{
		if (timer > 0.1f && firing)
		{
			ticking = false;
			rolling = false;
			foreach (Ball b in ballListA)
			{
				if (!b.stopped)
				{
					rolling = true;
					break;
				}
			}
			foreach (Ball b in ballListB)
			{
				if (!b.stopped)
				{
					rolling = true;
					break;
				}
			}
			if (!rolling)
			{
				firing = false;
				timer = 0f;

				//Active player send position check.
				//Inactive player validate the positions.
				//Or just send an empty pack.
				LoginRequist.ucl.rpcCall("play.round_over", null, null);
			}
		}
	}

	public void GetServerMsg(int id, float rad, float length)
	{
		print(rad);
		print(length);
		if (GameManager.Instance.isPlayerA)
			print(id - 3);
		//float rad = UnityEngine.Random.value * Mathf.PI * 2;
		//float length = UnityEngine.Random.value;
		//current = ballListB[(int)(ballListB.Count * UnityEngine.Random.value)];
		current = GameManager.Instance.isPlayerA ? ballListB[id - 3] : ballListA[id];
		current.bl.Launch(rad + Mathf.PI, length);
		//firing = true;
	}
}

class BallRdArr
{
	public BallRdArr() { }
	public BallRdArr(int f, int s, int t)
	{
		first = f;
		second = s;
		third = t;
	}
	public int first;
	public int second;
	public int third;
}

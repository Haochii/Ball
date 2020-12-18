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
			StartInterval();
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

	public void Deploy()
	{
		GameManager.Instance.uIHUD.footerDeploy.SetActive(false);
		GameManager.Instance.uIHUD.spawnPoints[0].transform.parent.gameObject.SetActive(false);

		//Send ready message.
		//Get server respond.

		foreach (Ball b in ballListB)
		{
			b.GetComponent<SpriteRenderer>().sprite = sprites[(int)(UnityEngine.Random.value * sprites.Length)];
			b.gameObject.SetActive(true);
		}

		GameStart();
	}

	public void GameStart()
	{
		Initialize();
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
		NextRound();
	}

	//For player moving the next ball.
	public void NextMove()
	{
		if (gameStop)
		{
			return;
		}

		current = null;
		movesLeft--;
		if (movesLeft < 0 || countDown <= 0f)
		{
			NextRound();
			return;
		}

		if (!isCurrentPlayerA)
		{
			GetServerMsg(); //Or something like that.
			return;
		}

		GameManager.Instance.uILaunch.active = true;
		GameManager.Instance.uILaunch.SwitchButton(true);
	}

	//All balls launched, switching players.
	public void NextRound()
	{

		//Server switching player.

		isCurrentPlayerA = !isCurrentPlayerA;
		movesLeft = maxMoves;
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
		NextMove();
	}

	public void StartInterval()
	{
		waiting = true;
		timer = 0f;
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
				StartInterval();
			}
		}
		else if (timer > roundInterval && waiting)
		{
			timer = 0f;
			waiting = false;
			NextMove();
		}
	}

	private void GetServerMsg()
	{
		float rad = UnityEngine.Random.value * Mathf.PI * 2;
		float length = UnityEngine.Random.value;
		current = ballListB[(int)(ballListB.Count * UnityEngine.Random.value)];
		current.bl.Launch(rad, length);
		firing = true;
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

	public void PlaceBall(int id)
	{
		if (!spawnReady)
		{
			return;
		}
		Ball ball = ballListA[curSpawnPoint];
		ball.gameObject.SetActive(true);
		ball.GetComponent<SpriteRenderer>().sprite = sprites[id];

		deployReady = true;
		foreach (Ball b in ballListA)
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
}

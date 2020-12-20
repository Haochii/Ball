﻿using BaseFramework.Network;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RoundManager : MonoBehaviour
{
	public static RoundManager Instance;

	public int round;
	public int movesLeft;
	public int maxMoves = 1;
	public float deployTime = 30f;
	public float roundTime = 15f;
	public float countDown = 999f;
	public float deployCountdown = 30f;
	public float roundInterval = 1f;
	public bool deploying;
	public bool firing;
	public bool isCurrentPlayerA = true;       //If the controlling player is player A.
											   //public bool isPlayerA;			//If player himself is player A.
	public bool gameStop;
	public List<Ball> ballListA;
	public List<Ball> ballListB;
	public Ball current;
	public Sprite[] sprites;
	public Sprite[] buttonSprites;
	public Transform[] spawnPoints;
	public Sprite healthBarSelf;
	public Sprite healthBarEnemy;
	public Sprite sphereSelf;
	public Sprite sphereEnemy;
	public Color colorUncheck;
	public Color colorCheck;
	public GameObject[] infos;

	public int skillPointPerCollision;
	public int skillPointPerDamage;
	public int skillPointA;
	public int skillPointB;
	public int maxSkillPoints = 150;
	public int skillGaugeCount;

	public float damageFormulaCoefficient;
	public int maxBuffLevel;
	public float attackBuff;
	public float speedBuff;
	public float aimLineCoefficient;
	public int aimDragLen;
	public float enterAimTime;

	[HideInInspector]
	public UnityEvent nextRound;

	private bool spawnReady;
	private bool deployReady;
	private int curSpawnPoint;
	private int curSpawnType;
	private bool ticking;   //If the round countdown is ticking.
	private bool waiting;   //If the interruption is going on after all the balls have stopped.
	private bool rolling;   //If there're still any balls rolling after a ball has been fired.
	private float timer;
	private List<List<string>> globalLists;
	private List<List<string>> ballTypeLists;
	private Ball spawningBall;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	void Start()
	{
		isCurrentPlayerA = true;
		curSpawnPoint = 0;
		deployTime = 30f;
		deployCountdown = deployTime;
		deploying = spawnReady = deployReady = false;
		rolling = false;
		waiting = false;
		GameManager.Instance.uIHUD.spawnPoints[0].transform.parent.gameObject.SetActive(false);
		//Initialize();
	}

	private void Update()
	{
		CheckBallMove();

		if (deploying)
		{
			deployCountdown -= Time.deltaTime;
			GameManager.Instance.uIHUD.countDown.text = ((int)timer).ToString();
		}
		if (deploying && deployCountdown <= 0f)
		{
			deploying = false;
			List<Ball> list = GameManager.Instance.isPlayerA ? ballListA : ballListB;
			foreach (Ball b in list)
			{
				if (!b.gameObject.activeSelf)
				{
					curSpawnPoint = list.IndexOf(b);
					curSpawnType = (UnityEngine.Random.Range(0, sprites.Length - 1));
					SelectSpawnPoint(curSpawnPoint);
				}
			}
			DeploySelf();
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
			if (isCurrentPlayerA == GameManager.Instance.isPlayerA)
			{
				LoginRequist.ucl.rpcCall("play.round_over", null, null);
			}
		}
	}

	public void Initialize()
	{
		if (!GameManager.Instance.isPlayerA)
		{
			Camera.main.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
			GameManager.Instance.uIHUD.MoveSpawnICon();
			foreach (Ball b in ballListA)
			{
				b.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
			}
			foreach (Ball b in ballListB)
			{
				b.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
			}
		}
		ballTypeLists = CSVManager.Instance.BallTypeLists;
		globalLists = CSVManager.Instance.GlobalLists;
		deployTime = int.Parse(globalLists[1][0]);
		roundTime = int.Parse(globalLists[1][1]);
		damageFormulaCoefficient = float.Parse(globalLists[1][2]);
		maxBuffLevel = int.Parse(globalLists[1][3]);
		attackBuff = int.Parse(globalLists[1][4]) / 100f;
		speedBuff = int.Parse(globalLists[1][5]) / 100f;
		skillPointPerCollision = int.Parse(globalLists[1][6]);
		skillPointPerDamage = int.Parse(globalLists[1][7]);
		maxSkillPoints = int.Parse(globalLists[1][8]);
		skillGaugeCount = int.Parse(globalLists[1][9]);
		aimLineCoefficient = int.Parse(globalLists[1][10]);
		aimDragLen = int.Parse(globalLists[1][11]);
		enterAimTime = float.Parse(globalLists[1][12]);

		foreach (Ball b in ballListA)
		{
			b.damageFormulaCoefficient = damageFormulaCoefficient;
			b.maxBuff = maxBuffLevel;
		}
		foreach (Ball b in ballListB)
		{
			b.damageFormulaCoefficient = damageFormulaCoefficient;
			b.maxBuff = maxBuffLevel;
		}
		GameManager.Instance.uILaunch.aimRingRadius = aimDragLen;
		GameManager.Instance.uILaunch.waitTime = enterAimTime;

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
		infos[0].GetComponent<RoundInfo>().ShowRoundInfo();
		//Start deploy countdown.
		deploying = true;
	}

	public void SelectSpawnPoint(int id)
	{
		if (!spawnReady)
		{
			return;
		}
		curSpawnPoint = id;

		List<Ball> list;
		list = GameManager.Instance.isPlayerA ? ballListA : ballListB;
		spawningBall = list[curSpawnPoint];
		spawningBall.type = curSpawnType;
		spawningBall.gameObject.SetActive(true);
		spawningBall.transform.position = GameManager.Instance.uIHUD.spawnPoints[curSpawnPoint].transform.position;
		spawningBall.GetComponent<SpriteRenderer>().sprite = sprites[curSpawnType];
		spawningBall.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = sphereSelf;
		//spawningBall.healthFill.color = colorSelf;
		spawningBall.healthFill.sprite = healthBarSelf;

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

		//GameManager.Instance.uIHUD.spawnPoints[curSpawnPoint].color = GameManager.Instance.uIHUD.uncheckColor;
		//curSpawnPoint = id;
		//GameManager.Instance.uIHUD.spawnPoints[id].color = GameManager.Instance.uIHUD.checkColor;
	}

	public void PlaceBall(int type)
	{
		AudioManager.Instance.playMusic(AudioManager.Instance.chooseBall);
		if (!spawnReady)
		{
			spawnReady = true;
			GameManager.Instance.uIHUD.spawnPoints[0].transform.parent.gameObject.SetActive(true);
		}
		foreach (Button i in GameManager.Instance.uIHUD.selectButtons)
		{
			i.image.color = colorUncheck;
		}
		GameManager.Instance.uIHUD.selectButtons[type].image.color = colorCheck;
		curSpawnType = type;
		/*
		List<Ball> list;
		list = GameManager.Instance.isPlayerA ? ballListA : ballListB;
		spawningBall = list[curSpawnPoint];
		spawningBall.gameObject.SetActive(true);
		spawningBall.transform.position = GameManager.Instance.uIHUD.spawnPoints[curSpawnPoint].transform.position;
		spawningBall.type = type;
		spawningBall.GetComponent<SpriteRenderer>().sprite = sprites[type];
		spawningBall.GetComponent<SpriteRenderer>().color = colorSelf;
		spawningBall.healthFill.color = colorSelf;

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
		*/
	}

	public void DeploySelf()
	{
		deploying = false;
		GameManager.Instance.uIHUD.footerDeploy.SetActive(false);
		GameManager.Instance.uIHUD.spawnPoints[0].transform.parent.gameObject.SetActive(false);

		//Send ready message.
		//Get server respond.

		List<Ball> list = GameManager.Instance.isPlayerA ? ballListA : ballListB;
		foreach (Ball b in list)
		{
			b.launchSpeed = float.Parse(ballTypeLists[b.type + 1][4]);
			b.speedReduce = float.Parse(ballTypeLists[b.type + 1][5]);
			b.maxHealth = int.Parse(ballTypeLists[b.type + 1][6]);
			b.baseAttack = float.Parse(ballTypeLists[b.type + 1][7]);

			/*
			switch (b.type)
			{
				case 0:
					b.launchSpeed = float.Parse(ballTypeLists[1][4]);
					b.speedReduce = float.Parse(ballTypeLists[1][5]);
					b.maxHealth = int.Parse(ballTypeLists[1][6]);
					b.baseAttack = float.Parse(ballTypeLists[1][7]);
					//b.skill
					break;
				case 1:
					b.launchSpeed = float.Parse(ballTypeLists[2][4]);
					b.speedReduce = float.Parse(ballTypeLists[2][5]);
					b.maxHealth = int.Parse(ballTypeLists[2][6]);
					b.baseAttack = float.Parse(ballTypeLists[2][7]);
					//b.skill
					break;
				case 2:
					b.launchSpeed = float.Parse(ballTypeLists[3][4]);
					b.speedReduce = float.Parse(ballTypeLists[3][5]);
					b.maxHealth = int.Parse(ballTypeLists[3][6]);
					b.baseAttack = float.Parse(ballTypeLists[3][7]);
					//b.skill
					break;
				case 3:
					b.launchSpeed = float.Parse(ballTypeLists[4][4]);
					b.speedReduce = float.Parse(ballTypeLists[4][5]);
					b.maxHealth = int.Parse(ballTypeLists[4][6]);
					b.baseAttack = float.Parse(ballTypeLists[4][7]);
					//b.skill
					break;
				case 4:
					b.launchSpeed = float.Parse(ballTypeLists[5][4]);
					b.speedReduce = float.Parse(ballTypeLists[5][5]);
					b.maxHealth = int.Parse(ballTypeLists[5][6]);
					b.baseAttack = float.Parse(ballTypeLists[5][7]);
					//b.skill
					break;
			}*/
		}
		for (int i = 0; i < list.Count; i++)
		{
			GameManager.Instance.uILaunch.ballSelectButtons[i].GetComponent<Image>().sprite = buttonSprites[list[i].type];
		}

		BallRdArr ballRdArr = new BallRdArr(list[0].type, list[1].type, list[2].type);
		LoginRequist.ucl.rpcCall("play.ball_ready", JsonConvert.SerializeObject(ballRdArr), null);
	}

	public void DeployEnemy(int[] types)
	{
		List<Ball> list = GameManager.Instance.isPlayerA ? ballListB : ballListA;
		foreach (Ball b in list)
		{
			b.launchSpeed = float.Parse(ballTypeLists[b.type + 1][4]);
			b.speedReduce = float.Parse(ballTypeLists[b.type + 1][5]);
			b.maxHealth = int.Parse(ballTypeLists[b.type + 1][6]);
			b.baseAttack = float.Parse(ballTypeLists[b.type + 1][7]);
			/*
			switch (b.type)
			{
				case 0:
					b.launchSpeed = float.Parse(ballTypeLists[1][4]);
					b.speedReduce = float.Parse(ballTypeLists[1][5]);
					b.maxHealth = int.Parse(ballTypeLists[1][6]);
					b.baseAttack = float.Parse(ballTypeLists[1][7]);
					//b.skill
					break;
				case 1:
					b.launchSpeed = float.Parse(ballTypeLists[2][4]);
					b.speedReduce = float.Parse(ballTypeLists[2][5]);
					b.maxHealth = int.Parse(ballTypeLists[2][6]);
					b.baseAttack = float.Parse(ballTypeLists[2][7]);
					//b.skill
					break;
				case 2:
					b.launchSpeed = float.Parse(ballTypeLists[3][4]);
					b.speedReduce = float.Parse(ballTypeLists[3][5]);
					b.maxHealth = int.Parse(ballTypeLists[3][6]);
					b.baseAttack = float.Parse(ballTypeLists[3][7]);
					//b.skill
					break;
				case 3:
					b.launchSpeed = float.Parse(ballTypeLists[4][4]);
					b.speedReduce = float.Parse(ballTypeLists[4][5]);
					b.maxHealth = int.Parse(ballTypeLists[4][6]);
					b.baseAttack = float.Parse(ballTypeLists[4][7]);
					//b.skill
					break;
				case 4:
					b.launchSpeed = float.Parse(ballTypeLists[5][4]);
					b.speedReduce = float.Parse(ballTypeLists[5][5]);
					b.maxHealth = int.Parse(ballTypeLists[5][6]);
					b.baseAttack = float.Parse(ballTypeLists[5][7]);
					//b.skill
					break;
			}*/
		}
		for (int i = 0; i < list.Count; i++)
		{
			list[i].GetComponent<SpriteRenderer>().sprite = sprites[types[i]];
			list[i].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = sphereEnemy;
			list[i].healthFill.sprite = healthBarEnemy;
			//list[i].healthFill.color = colorEnemy;
			list[i].transform.position = spawnPoints[list[i].id].position;
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

		AudioManager.Instance.playMusic(AudioManager.Instance.start);

		List<Ball> list = GameManager.Instance.isPlayerA ? ballListB : ballListA;
		for (int i = 0; i < list.Count; i++)
		{
			list[i].gameObject.SetActive(true);
		}

		for (int i = 0; i < GameManager.Instance.uIHUD.healthHolder.childCount; i++)
		{
			GameManager.Instance.uIHUD.healthHolder.GetChild(i).gameObject.SetActive(true);
		}

		foreach (Ball b in ballListA)
		{
			b.health = b.maxHealth;
		}
		foreach (Ball b in ballListB)
		{
			b.health = b.maxHealth;
		}

		infos[1].GetComponent<RoundInfo>().ShowRoundInfo();
		NextRound();
	}

	public void GetBall(int id)
	{
		if (id < 0)
		{
			current = null;
			GameManager.Instance.uILaunch.ballLocIcon.gameObject.SetActive(false);
			GameManager.Instance.uILaunch.ChangeBallButton(id);
			return;
		}
		else
		{
			current = GameManager.Instance.isPlayerA ? ballListA[id] : ballListB[id];
			if (!current.gameObject.activeSelf)
			{
				current = null;
				GameManager.Instance.uILaunch.ballLocIcon.gameObject.SetActive(false);
				GameManager.Instance.uILaunch.ChangeBallButton(-1);
				return;
			}
			GameManager.Instance.uILaunch.ballLocIcon.gameObject.SetActive(true);
			GameManager.Instance.uILaunch.ballLocIcon.transform.position = current.transform.position;
			GameManager.Instance.uILaunch.ChangeBallButton(id);
		}
	}

	//All balls launched, switching players.
	public void NextRound()
	{
		print("LocalNext");
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
		if (isCurrentPlayerA != GameManager.Instance.isPlayerA)
		{
			infos[3].GetComponent<RoundInfo>().ShowRoundInfo();
			foreach(Button b in GameManager.Instance.uILaunch.ballSelectButtons)
			{
				b.GetComponent<Image>().color = colorUncheck;
			}
			//GetServerMsg(); //For local use only.
			return;
		}

		infos[2].GetComponent<RoundInfo>().ShowRoundInfo();
		foreach (Button b in GameManager.Instance.uILaunch.ballSelectButtons)
		{
			b.GetComponent<Image>().color = colorCheck;
		}
		GameManager.Instance.uILaunch.ready = true;
		//GameManager.Instance.uILaunch.SwitchButtonInteract(true);
	}

	public void CheckBallList()
	{
		print("Check.");
		bool allDie = true;
		foreach (Ball b in ballListA)
		{
			if (b.gameObject.activeSelf)
			{
				allDie = false;
				break;
			}
		}
		if (allDie)
		{
			GameManager.Instance.Win(false);
			return;
		}

		allDie = true;
		foreach (Ball b in ballListB)
		{
			if (b.gameObject.activeSelf)
			{
				allDie = false;
				break;
			}
		}
		if (allDie)
		{
			GameManager.Instance.Win(true);
			return;
		}

		/*
		if (ballListA.Count == 0)
		{
			GameManager.Instance.Win(false);
		}
		else if (ballListB.Count == 0)
		{
			GameManager.Instance.Win(true);
		}
		*/
	}

	private void CheckBallMove()
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
				if (GameManager.Instance.isPlayerA == isCurrentPlayerA)
				{
					SendPosition();
				}
				//Inactive player validate the positions.
				//Do a little pause to compensate the lag.

				LoginRequist.ucl.rpcCall("play.round_over", null, null);
				//LoginRequist.ucl.rpcCall("play.all_balls_stop", null, null);
			}
		}
	}

	public void GetServerMsg(int id, int deg, int length)
	{
		print("ServerDeg: " + deg);
		print("ServerLen: " + length);
		if (GameManager.Instance.isPlayerA)
			print(id - 3);
		//float rad = UnityEngine.Random.value * Mathf.PI * 2;
		//float length = UnityEngine.Random.value;
		//current = ballListB[(int)(ballListB.Count * UnityEngine.Random.value)];
		current = GameManager.Instance.isPlayerA ? ballListB[id - 3] : ballListA[id];
		current.bl.Launch(deg, length);
		//firing = true;
	}

	public void SendPosition()
	{
		int[] x, y;
		x = new int[6];
		y = new int[6];
		x[0] = (int)(ballListA[0].transform.position.x * 10000);
		x[1] = (int)(ballListA[1].transform.position.x * 10000);
		x[2] = (int)(ballListA[2].transform.position.x * 10000);
		x[3] = (int)(ballListB[0].transform.position.x * 10000);
		x[4] = (int)(ballListB[1].transform.position.x * 10000);
		x[5] = (int)(ballListB[2].transform.position.x * 10000);

		y[0] = (int)(ballListA[0].transform.position.y * 10000);
		y[1] = (int)(ballListA[1].transform.position.y * 10000);
		y[2] = (int)(ballListA[2].transform.position.y * 10000);
		y[3] = (int)(ballListB[0].transform.position.y * 10000);
		y[4] = (int)(ballListB[1].transform.position.y * 10000);
		y[5] = (int)(ballListB[2].transform.position.y * 10000);

		VerifyList verifyList = new VerifyList(x, y);
		LoginRequist.ucl.rpcCall("location.get_location", JsonConvert.SerializeObject(verifyList), null);
	}

	public void ValidatePosition(int[] intX, int[] intY)
	{
		ballListA[0].transform.position = new Vector2((float)intX[0] / 10000, (float)intY[0] / 10000);
		ballListA[1].transform.position = new Vector2((float)intX[1] / 10000, (float)intY[1] / 10000);
		ballListA[2].transform.position = new Vector2((float)intX[2] / 10000, (float)intY[2] / 10000);
		ballListB[0].transform.position = new Vector2((float)intX[3] / 10000, (float)intY[3] / 10000);
		ballListB[1].transform.position = new Vector2((float)intX[4] / 10000, (float)intY[4] / 10000);
		ballListB[2].transform.position = new Vector2((float)intX[5] / 10000, (float)intY[5] / 10000);
		/*
		if (GameManager.Instance.isPlayerA)
		{
			ballListA[0].transform.position = new Vector2((float)intX[0] / 10000, (float)intY[0] / 10000);
			ballListA[1].transform.position = new Vector2((float)intX[1] / 10000, (float)intY[1] / 10000);
			ballListA[2].transform.position = new Vector2((float)intX[2] / 10000, (float)intY[2] / 10000);
			ballListB[0].transform.position = new Vector2((float)intX[3] / 10000, (float)intY[3] / 10000);
			ballListB[1].transform.position = new Vector2((float)intX[4] / 10000, (float)intY[4] / 10000);
			ballListB[2].transform.position = new Vector2((float)intX[5] / 10000, (float)intY[5] / 10000);
		}
		else
		{
			for (int i = 0; i < intY.Length; i++)
			{
				if (intY[i] > 0)
				{
					intY[i] = 10000 - intY[i];
				}
				else
				{
					intY[i] = -10000 - intY[i];
				}
			}
			ballListA[0].transform.position = new Vector2((float)-intX[0] / 10000, (float)intY[0] / 10000);
			ballListA[1].transform.position = new Vector2((float)-intX[1] / 10000, (float)intY[1] / 10000);
			ballListA[2].transform.position = new Vector2((float)-intX[2] / 10000, (float)intY[2] / 10000);
			ballListB[0].transform.position = new Vector2((float)-intX[3] / 10000, (float)intY[3] / 10000);
			ballListB[1].transform.position = new Vector2((float)-intX[4] / 10000, (float)intY[4] / 10000);
			ballListB[2].transform.position = new Vector2((float)-intX[5] / 10000, (float)intY[5] / 10000);
		}
		*/
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

public class VerifyList
{
	public VerifyList(int[] _intX, int[] _intY)
	{
		intX = _intX;
		intY = _intY;
	}

	public int[] intX;
	public int[] intY;
}

using BaseFramework.Network;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;
	public UILaunch uILaunch;
	public UIHUD uIHUD;
	public UIMenu uIMenu;

	public bool isPlayerA;
	public string playerAName;
	public string playerBName;

	private bool winner;	//A: true; B: false.

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	void Start()
	{
		//isPlayerA = true;	//Just for offline test.
		//Get If the player is player A.

		winner = false;
	}

	void Update()
	{
		if (Input.GetButtonDown("Cancel"))
		{
			Reset();
		}
	}

	public void Reset()
	{
		SceneManager.LoadScene(0);
	}

	public void StartMatchmaking()
	{
		uIMenu.menuPanel.gameObject.SetActive(false);
		LoginRequist.ucl.rpcCall("combat.start_match", null, null);
		//Matckmaking...
	}

	public void EnterMatch(int side)
	{
		//Done!
		//Receiving data...

		isPlayerA = side == 0;

		playerAName = uIMenu.inputField.text;
		playerBName = "Server";	//Required name from server.
		uIHUD.playerA.text = playerAName;
		uIHUD.playerB.text = playerBName;
		uIMenu.middlePanel.gameObject.SetActive(false);

		RoundManager.Instance.StartDeploy();
	}

	public void Win(bool isPlayerA)
	{
		transform.GetComponent<RoundManager>().gameStop = true;

		winner = isPlayerA;
		IsWin myWin = new IsWin(winner);
		LoginRequist.ucl.rpcCall("play.game_over", JsonConvert.SerializeObject(myWin), null);
	}

	public void ConfirmWin(bool result)
	{
		if(winner == result)
		{
			uIMenu.resultPanel.gameObject.SetActive(true);
			uIMenu.resultText.text = winner ? playerAName : playerBName + "Wins!";
		}
		else
		{
			Debug.LogError("Match result unmatched!!!");
		}
	}
}

public class IsWin
{
	public IsWin(bool iIsWin)
	{
		Win = iIsWin;
	}
	public bool Win;
}

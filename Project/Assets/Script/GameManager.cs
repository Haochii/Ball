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


	}



	void Update()
	{
		if (Input.GetButtonDown("Cancel"))
		{
			Reset();
		}
	}

	private void Reset()
	{
		SceneManager.LoadScene(0);
	}

	public void StartMatchmaking()
	{
		uIMenu.menuPanel.gameObject.SetActive(false);

		//Matckmaking...
		//Done!
		//Receiving data...

		playerAName = uIMenu.inputField.text;
		playerBName = "Server";
		uIHUD.playerA.text = playerAName;
		uIHUD.playerB.text = playerBName;
		uIMenu.middlePanel.gameObject.SetActive(false);

		RoundManager.Instance.deploying = true;
	}

	public void Win(bool isPlayerA)
	{
		transform.GetComponent<RoundManager>().gameStop = true;
		//transform.GetComponent<UIHUD>().result.gameObject.SetActive(true);
		//transform.GetComponent<UIHUD>().result.text = "Player " + id + "Wins!";
		Debug.Log("Player " + (isPlayerA ? "A" : "B") + " wins!");
	}
}

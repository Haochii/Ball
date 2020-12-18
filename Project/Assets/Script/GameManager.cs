using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;


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


		RoundManager.Instance.GameStart();
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

	public void Win(bool isPlayerA)
	{
		transform.GetComponent<RoundManager>().gameStop = true;
		//transform.GetComponent<UIHUD>().result.gameObject.SetActive(true);
		//transform.GetComponent<UIHUD>().result.text = "Player " + id + "Wins!";
		Debug.Log("Player " + (isPlayerA ? "A" : "B") + " wins!");
	}
}

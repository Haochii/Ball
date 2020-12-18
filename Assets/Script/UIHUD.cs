using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHUD : MonoBehaviour
{
	public Text countDown;
	public Text playerA;
	public Text playerB;
	public Image skillIcon;
	public Image skillBar;
	public Button[] ballIcon;
	public Button[] ballIconA;
	public Button[] ballIconB;
	public Transform healthHolder;
	public Transform[] healthBar;

	/*
	private void Awake()
	{
		for (int i = 0; i < healthHolder.childCount; i++)
		{
			healthBar[i] = healthHolder.GetChild(i);
		}
	}
	*/

	void Start()
	{
		playerA.text = GameManager.Instance.playerAName;
		playerB.text = GameManager.Instance.playerBName;
		/*
		for (int i = 0; i < ballIcon.Length; i++)
		{
			ballIcon[i].onClick.AddListener()
		}
		for (int i = 0; i < ballIconA.Length; i++)
		{

		}
		for (int i = 0; i < ballIconB.Length; i++)
		{

		}
		*/
	}

	void Update()
	{
		countDown.text = ((int)RoundManager.Instance.countDown).ToString();

	}

	public void Halt()
	{

	}
}

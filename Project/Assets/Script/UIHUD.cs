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
	public Button[] ballIconA;
	public Button[] ballIconB;
	public Transform healthHolder;
	public Transform[] healthBar;
	public float selectTime = 30f;
	public Button[] selectButtons;
	public Button deployButton;
	public Image[] spawnPoints;
	public Color checkColor;
	public Color uncheckColor;
	public GameObject footerDeploy;

	public Color isControlling;
	public Color notControlling;

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
		//RoundManager.Instance.nextRound.AddListener(ChangePlayerNameColor);

		foreach (Image image in spawnPoints)
		{
			image.color = uncheckColor;
		}

		for (int i = 0; i < ballIconA.Length; i++)
		{

		}
		for (int i = 0; i < ballIconB.Length; i++)
		{

		}
	}

	void Update()
	{
		countDown.text = ((int)RoundManager.Instance.countDown).ToString();

		if (RoundManager.Instance.isCurrentPlayerA)
		{
			playerA.color = isControlling;
			playerB.color = notControlling;
		}
		else
		{
			playerB.color = isControlling;
			playerA.color = notControlling;
		}
	}

	private void OnDestroy()
	{
		RoundManager.Instance.nextRound.RemoveListener(ChangePlayerNameColor);
	}

	public void ChangePlayerNameColor()
	{
		/*
		if (RoundManager.Instance.isCurrentPlayerA)
		{
			playerA.color = isControlling;
			playerB.color = notControlling;
		}
		else
		{
			playerB.color = isControlling;
			playerA.color = notControlling;
		}
		*/
	}

	public void Halt()
	{
		
	}

	public void Stop()
	{
		enabled = false;
	}
}

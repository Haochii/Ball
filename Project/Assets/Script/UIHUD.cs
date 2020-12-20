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
	public Image SkillBarMiniA;
	public Image SkillBarMiniB;
	public Transform healthHolder;
	public Transform[] healthBar;
	public Button[] selectButtons;
	public Button deployButton;
	public Image[] spawnPoints;
	public Color checkColor;
	public Color uncheckColor;
	public GameObject footerDeploy;

	public Color isControlling;
	public Color notControlling;

	void Start()
	{
		//RoundManager.Instance.nextRound.AddListener(ChangePlayerNameColor);
		foreach (Image image in spawnPoints)
		{
			image.color = uncheckColor;
		}

		skillBar.fillAmount = 0f;
		SkillBarMiniA.fillAmount = 0f;
		SkillBarMiniB.fillAmount = 0f;
	}

	void Update()
	{
		ChangePlayerNameColor();
		if (RoundManager.Instance.deploying)
		{
			playerA.color = isControlling;
			playerB.color = notControlling;
			countDown.text = ((int)RoundManager.Instance.deployCountdown).ToString();
		}
		else
		{
			countDown.text = ((int)RoundManager.Instance.countDown).ToString();
		}

	}

	private void OnDestroy()
	{
		//RoundManager.Instance.nextRound.RemoveListener(ChangePlayerNameColor);
	}

	public void MoveSpawnICon()
	{
		spawnPoints[0].transform.position = RoundManager.Instance.spawnPoints[3].position;
		spawnPoints[1].transform.position = RoundManager.Instance.spawnPoints[4].position;
		spawnPoints[2].transform.position = RoundManager.Instance.spawnPoints[5].position;
	}

	public void ChangePlayerNameColor()
	{
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

	public void ChargeSkillBar()
	{
		float sa = (float)RoundManager.Instance.skillPointA / RoundManager.Instance.maxSkillPoints;
		float sb = (float)RoundManager.Instance.skillPointB / RoundManager.Instance.maxSkillPoints;
		if (GameManager.Instance.isPlayerA)
		{
			skillBar.fillAmount = sa;
		}
		else
		{
			skillBar.fillAmount = sb;
		}
		SkillBarMiniA.fillAmount = sa;
		SkillBarMiniB.fillAmount = sb;
	}

	public void Halt()
	{

	}

	public void Stop()
	{
		enabled = false;
	}
}

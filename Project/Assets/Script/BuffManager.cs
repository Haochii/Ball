using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffList
{
	None = 3001,
	Heal = 3101,
	Buff = 3201,
	SkillPoints = 3301
}

public class BuffManager : MonoBehaviour
{
	public List<List<string>> buffLists;

	public void AddBuff(Ball ball, BuffList buff)
	{
		buffLists = CSVManager.Instance.BuffLists;

		switch (buff)
		{
			case BuffList.None:
				break;
			case BuffList.Heal:
				print(-1 * int.Parse(buffLists[2][2]));
				ball.Damage(-int.Parse(buffLists[2][2]));
				break;
			case BuffList.Buff:
				ball.Strengthen();
				break;
			case BuffList.SkillPoints:
				if (GameManager.Instance.isPlayerA)
				{
					RoundManager.Instance.skillPointA += int.Parse(buffLists[4][2]);
					if(RoundManager.Instance.skillPointA > RoundManager.Instance.maxSkillPoints)
					{
						RoundManager.Instance.skillPointA = RoundManager.Instance.maxSkillPoints;
					}
					GameManager.Instance.uIHUD.ChargeSkillBar();
				}
				else
				{
					RoundManager.Instance.skillPointB += int.Parse(buffLists[4][2]);
					if (RoundManager.Instance.skillPointB > RoundManager.Instance.maxSkillPoints)
					{
						RoundManager.Instance.skillPointB = RoundManager.Instance.maxSkillPoints;
					}
					GameManager.Instance.uIHUD.ChargeSkillBar();
				}
				break;
		}
	}
}

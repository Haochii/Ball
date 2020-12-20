using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
public enum SkillList
{
	//<算无遗策>需要碰撞触发 碰撞时结算
	SuanWuYiCe = 2101,
	//<护国恶来>碰撞时触发&结算
	HuGuoELai = 2102,
	//<血战长坂>碰撞时触发&结算
	XueZhanChangBan = 2201,
	//<火烧连营>碰撞时触发一阶段固定伤害&禁用此次碰撞的伤害&结算
	HuoShaoLianYing = 2301,
	//<火烧连营-弹射>回调触发
	HuoShaoLianYingContinue = 23011,
	//<悬壶济世>需要碰撞触发 碰撞时累加结算
	XuanHuJiShi = 2401,
	//<悬壶济世-结算>回合结束触发&结算
	XuanHuJiShiResult = 24011
}

public class SkillClass
{
	public SkillClass(string a = "", string b = "", string c = "")
	{
		numa = int.Parse(a);
		numb = int.Parse(b);
		numc = int.Parse(c);
	}
	public int numa;
	public int numb;
	public int numc;
}


public class SkillManager : MonoBehaviour
{
	public static SkillManager Instance;

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

	public List<List<string>> lists;

	private void Start()
	{
		lists = CSVManager.Instance.SkillLists;
	}

	public void UseSkill(SkillList skillID, int ia = 0, int ib = 0, int ic = 0, float ix = 0)
	{
		switch (skillID)
		{
			//<算无遗策>
			case SkillList.SuanWuYiCe:
				SuanWuYiCeFunc();
				break;
			//<护国恶来>
			case SkillList.HuGuoELai:
				//ia,ib传入&修改 基础伤害减少防御值 和 加成伤害减少防御值;
				//ia = 基础伤害减少防御值; ib = 加成伤害减少防御值;
				HuGuoELaiFunc(ia, ib);
				break;
			//<血战长坂>
			case SkillList.XueZhanChangBan:
				//ia 判定敌友
				//ib ic传入&修改 速度衰减值 修改防御值
				//ia = 敌友enum; ib = 速度衰减值; ic = 攻击力
				XueZhanChangBanFunc(ia, ib, ic,ix);
				break;
			//<火烧连营>
			case SkillList.HuoShaoLianYing:
				//ia 传入&修改 固定碰撞伤害值
				//ia = 固定碰撞伤害值
				HuoShaoLianYingFunc(ia);
				break;
			case SkillList.HuoShaoLianYingContinue:
				//ia 传入 被链接的球的id
				//ia = 被链接的球的id
				HuoShaoLianYingContinueFunc(ia);
				break;
			//<悬壶济世>
			case SkillList.XuanHuJiShi:
				//ia传入&修改 碰撞次数
				//ia = 碰撞次数
				XuanHuJiShiFunc(ia);
				break;
			case SkillList.XuanHuJiShiResult:
				//ia传入 碰撞次数
				//ib传入&修改 结算治疗量
				//ia = 碰撞次数 ib = 结算治疗量
				XuanHuJiShiResultFunc(ia, ib);
				break;
			default:
				break;
		}
	}

	public void SuanWuYiCeFunc()
	{
		SkillClass SuanWuYiCeClass = new SkillClass(lists[1][4], lists[1][5], lists[1][6]);
		int dmg = SuanWuYiCeClass.numa + SuanWuYiCeClass.numb * Mathf.Min(7, SuanWuYiCeClass.numc);
		int forCounts = 0;
		int index = (int)UnityEngine.Random.value * 3;
		while (forCounts < 3)
		{
			if (GameManager.Instance.isPlayerA)
			{
				Ball ballB = RoundManager.Instance.ballListB[index];
				if (ballB.gameObject.activeSelf)
				{
					ballB.GetComponent<Ball>().Damage(dmg);
					print("算无遗策 触发");
				}
				else
				{
					IndexAdd(index);
				}
			}
			else
			{
				Ball ballA = RoundManager.Instance.ballListA[index];
				if (ballA.gameObject.activeSelf)
				{
					ballA.GetComponent<Ball>().Damage(dmg);
					print("算无遗策 触发");
				}
				else
				{
					IndexAdd(index);
				}
			}
			forCounts++;
		}
	}

	public void HuGuoELaiFunc(int ia,int ib)
	{
		//ia,ib传入&修改 基础伤害减少防御值 和 加成伤害减少防御值;
		//ia = 基础伤害减少防御值; ib = 加成伤害减少防御值;
		SkillClass HuGuoELaiClass = new SkillClass(lists[2][4], lists[2][5]);
		ia -= HuGuoELaiClass.numa;
		ib -= HuGuoELaiClass.numb;
		print("护国恶来 触发");
	}


	public void XueZhanChangBanFunc(int ia, int ib, int ic,float ix)
	{
		//ia 判定敌友
		//ib ic传入&修改 速度衰减值 修改防御值
		//ia = 敌友enum; ib = 速度衰减值; ic = 攻击力
		SkillClass XueZhanChangBan = new SkillClass(lists[3][4], lists[3][5], lists[3][6]);
		ix -= XueZhanChangBan.numa / 10;
		switch (ia)
		{
			case (int)ColType.Friendly:
				ic += XueZhanChangBan.numb;
				break;
			case (int)ColType.Enemy:
				ic += XueZhanChangBan.numc;
				break;
			default:
				break;
		}
		print("血战长坂 触发");
	}

	public void HuoShaoLianYingFunc(int ia)
	{
		//ia 传入&修改 固定碰撞伤害值
		//ia = 固定碰撞伤害值
		SkillClass HuoShaoLianYing = new SkillClass(lists[4][4]);
		ia = HuoShaoLianYing.numa;
		print("火烧连营 触发");
	}

	public void HuoShaoLianYingContinueFunc(int ia)
	{
		//传入 ia 被撞球的id
		SkillClass HuoShaoLianYingContinue = new SkillClass(lists[4][4], lists[4][5], lists[4][6]);
		int conti_dmg = HuoShaoLianYingContinue.numb;
		if (RoundManager.Instance.isCurrentPlayerA)
		{
			Ball ballB = RoundManager.Instance.ballListB[ia];
			Collider2D[] balls;
			balls = Physics2D.OverlapCircleAll(ballB.transform.position, HuoShaoLianYingContinue.numc);
			foreach (Collider2D b in balls)
			{
				Ball ene = b.transform.GetComponent<Ball>();
				if (ene.isPlayerA != RoundManager.Instance.isCurrentPlayerA && ene.huoShaoStatus != HuoShaoLianYingStatus.HuoShaoLianYingTouched)
				{
					ene.GetComponent<Ball>().Damage(conti_dmg);
					ene.huoShaoStatus = HuoShaoLianYingStatus.HuoShaoLianYingTouched;
					SkillManager.Instance.UseSkill(SkillList.HuoShaoLianYingContinue, ene.id);
				}
			}
		}
		else
		{
			Ball ballA = RoundManager.Instance.ballListA[ia];
			Collider2D[] balls;
			balls = Physics2D.OverlapCircleAll(ballA.transform.position, HuoShaoLianYingContinue.numc);
			foreach (Collider2D b in balls)
			{
				Ball ene = b.transform.GetComponent<Ball>();
				if (ene.isPlayerA != RoundManager.Instance.isCurrentPlayerA && ene.huoShaoStatus != HuoShaoLianYingStatus.HuoShaoLianYingTouched)
				{
					ene.GetComponent<Ball>().Damage(conti_dmg);
					ene.huoShaoStatus = HuoShaoLianYingStatus.HuoShaoLianYingTouched;
					SkillManager.Instance.UseSkill(SkillList.HuoShaoLianYingContinue, ene.id);
				}
			}
		}
	}

	public void XuanHuJiShiFunc(int ia)
	{
		SkillClass XuanHuJiShi = new SkillClass(lists[5][4], lists[5][5]);
		ia += XuanHuJiShi.numb;
		print("悬壶济世 触发");
	}

	public void XuanHuJiShiResultFunc(int ia, int ib)
	{
		// ia传入 碰撞次数
		//ib传入&修改 结算治疗量
		//ia = 碰撞次数 ib = 结算治疗量
		SkillClass XuanHuJiShiResult = new SkillClass(lists[5][4], lists[5][5]);
		ib += XuanHuJiShiResult.numa + XuanHuJiShiResult.numb * ia;
		print("悬壶济世-结算 触发");
	}
	public void IndexAdd(int index)
	{
		index++;
		index = index > 2 ? 0 : index;
	}
}

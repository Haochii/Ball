using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ColType
{
	Enemy,
	Friendly,
	Wall,
}

public enum HuoShaoLianYingStatus
{
	HuoShaoLianYingUnTouch,
	HuoShaoLianYingFirst,
	HuoShaoLianYingFirstEnd,
	HuoShaoLianYingTouched,
}

public class Ball : MonoBehaviour
{
	[Header("Property")]
	public int id;
	public bool isPlayerA;
	public int health;
	public int maxHealth;
	public float radius;
	[Header("Movement")]
	public bool stopped = true;
	public float launchSpeed;
	public float speedReduce;
	[Header("Combat")]
	public int type;
	public bool[] status;   //For buffs.
	public float baseAttack;
	public float damageFormulaCoefficient;
	public int buffCount = 0;
	public int maxBuff = 5;
	public Transform healthBar;
	public Image healthFill;
	public HuoShaoLianYingStatus huoShaoStatus;

	[HideInInspector]
	public BallLaunch bl;

	private Rigidbody2D rb;
	private float lastSpeed, curSpeed;
	//碰撞次数初始化
	private int colCounts = 0;

	void Start()
	{
		stopped = true;
		rb = GetComponent<Rigidbody2D>();
		lastSpeed = curSpeed = 0f;
		bl = GetComponent<BallLaunch>();
		health = maxHealth;
		healthBar = GameManager.Instance.uIHUD.healthBar[id];
		healthFill = healthBar.GetChild(0).GetComponent<Image>();
		buffCount = 0;
		gameObject.SetActive(false);
	}

	private void Update()
	{
		if (GameManager.Instance.isPlayerA)
		{
			healthBar.position = transform.position - new Vector3(0f, 1.6f * radius, 0f);
			healthFill.fillAmount = (float)health / maxHealth;
		}
		else
		{
			healthBar.position = transform.position + new Vector3(0f, 1.6f * radius, 0f);
			healthFill.fillAmount = (float)health / maxHealth;
		}
	}

	void FixedUpdate()
	{
		lastSpeed = curSpeed;
		curSpeed = rb.velocity.magnitude;
		if (curSpeed < speedReduce * Time.deltaTime)
		{
			rb.velocity = Vector2.zero;
			stopped = true;
		}
		else
		{
			rb.velocity -= rb.velocity * speedReduce * Time.deltaTime;
			stopped = false;
		}
	}

	public void Damage(int damage)
	{
		health -= damage;
		if (health <= 0)
		{
			health = 0;
			/*
			if (isPlayerA)
			{
				RoundManager.Instance.ballListA.Remove(this);
			}
			else
			{
				RoundManager.Instance.ballListB.Remove(this);
			}
			*/
			gameObject.SetActive(false);
			healthBar.gameObject.SetActive(false);
			RoundManager.Instance.CheckBallList();
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		//常规碰撞增加技力
		if (GameManager.Instance.isPlayerA)
		{
			RoundManager.Instance.skillPointA += RoundManager.Instance.skillPointPerCollision;
			GameManager.Instance.uIHUD.ChargeSkillBar();
		}
		else
		{
			RoundManager.Instance.skillPointB += RoundManager.Instance.skillPointPerCollision;
			GameManager.Instance.uIHUD.ChargeSkillBar();
		}

		Transform col = collision.transform;
		if (col.CompareTag("Player"))
		{
			//伤害敌人额外增加技力
			//造成碰撞的球为一方，回合也为那一方，且被碰撞的球为另一方
			if (isPlayerA == RoundManager.Instance.isCurrentPlayerA && col.GetComponent<Ball>().isPlayerA != RoundManager.Instance.isCurrentPlayerA)
			{
				col.GetComponent<Ball>().Damage((int)(lastSpeed / launchSpeed * baseAttack));
				if (GameManager.Instance.isPlayerA)
				{
					RoundManager.Instance.skillPointA += RoundManager.Instance.skillPointPerDamage;
					GameManager.Instance.uIHUD.ChargeSkillBar();
				}
				else
				{
					RoundManager.Instance.skillPointB += RoundManager.Instance.skillPointPerDamage;
					GameManager.Instance.uIHUD.ChargeSkillBar();
				}
			}
		}
		else if (col.CompareTag("Wall"))
		{
			GameManager.Instance.GetComponent<BuffManager>().AddBuff(this, GetRandomBuff());
			col.GetComponent<WallTile>().Damage();
		}

		//For using skills.
		//SkillManager.Instance.UseSkill(skillID, a, b);
	}

	public BuffList GetRandomBuff()
	{
		List<List<string>> buffLists = CSVManager.Instance.BuffLists;
		int i;
		float f = Random.value;
		float chance = 0f;
		for (i = 0; i < buffLists.Count - 1; i++)
		{
			chance += float.Parse(buffLists[i + 1][3]);
			if (f <= chance)
			{
				break;
			}
		}
		switch (i)
		{
			case 0:
				return BuffList.None;
			case 1:
				return BuffList.Heal;
			case 2:
				return BuffList.Buff;
			case 3:
				return BuffList.SkillPoints;
			default:
				return 0;
		}
	}

	public void Strengthen()
	{
		if (buffCount >= maxBuff)
		{
			return;
		}
		buffCount++;
		baseAttack += baseAttack * RoundManager.Instance.attackBuff;
		launchSpeed += launchSpeed * RoundManager.Instance.speedBuff;
	}
}

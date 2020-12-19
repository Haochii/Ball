using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
	public bool[] status;	//For buffs.
	public float attackMultiplier;
	public Transform healthBar;
	public Image healthFill;

	[HideInInspector]
	public BallLaunch bl;

	private Rigidbody2D rb;
	private float lastSpeed, curSpeed;

	void Start()
	{
		stopped = true;
		rb = GetComponent<Rigidbody2D>();
		lastSpeed = curSpeed = 0f;
		bl = GetComponent<BallLaunch>();
		healthBar = GameManager.Instance.uIHUD.healthBar[id];
		healthFill = healthBar.GetChild(0).GetComponent<Image>();
		if(!GameManager.Instance.isPlayerA)
		{
			transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
		}
		gameObject.SetActive(false);
	}

	private void Update()
	{
		if(GameManager.Instance.isPlayerA)
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
		Transform col = collision.transform;
		if (col.CompareTag("Player"))
		{
			if (isPlayerA == RoundManager.Instance.isCurrentPlayerA && col.GetComponent<Ball>().isPlayerA != RoundManager.Instance.isCurrentPlayerA)
			{
				col.GetComponent<Ball>().Damage((int)(lastSpeed / launchSpeed * attackMultiplier));
			}
		}
		else if (col.CompareTag("Wall"))
		{
			col.GetComponent<WallTile>().Damage();
		}
	}
}

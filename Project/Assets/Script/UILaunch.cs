using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILaunch : MonoBehaviour
{
	public bool active; //Ready for selection.
	public bool ready;  //Ready for launch.
	public Transform arrow;
	public float waitTime;
	public float aimRingRadius;
	public float maxArrowScale;
	public Button[] ballSelectButtons;
	public bool buttonPressed;

	private Vector2 startPos;
	private Vector2 endPos;
	private float angle;
	private float length;
	private bool waiting;
	private bool waitComplete;
	private float waitTimer;    //检测按下持续时间的计时器
	private float switchButtonTimer;	//在极短时间内屏蔽更换按钮时的鼠标按下操作，否则将导致点击别的小球按钮也会判定为取消选中

	void Start()
	{
		arrow.gameObject.SetActive(false);
		ready = false;
		waiting = waitComplete = false;
		waitTimer = 0f;
		//SwitchBotton(false);
		/*
		for (int i = 0; i < ballSelectButtons.Length; i++)
		{
			ballSelectButtons[i].onClick.AddListener(() => { RoundManager.Instance.current = GameManager.Instance.isPlayerA ? RoundManager.Instance.ballListA[i] : RoundManager.Instance.ballListB[i]; });
			ballSelectButtons[i].onClick.AddListener(() => { print(nums[i]); });
		}
		*/
	}

	void Update()
	{
		if (active)
		{
			//直接点选小球选中，弃用。
			/*
			if (Input.GetButtonDown("Fire1"))
			{
				Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				//Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, 100f, LayerMask.GetMask("Player"));
				if (hit && hit.transform.GetComponent<Ball>().isPlayerA)
				{
					Ball ball = hit.transform.GetComponent<Ball>();
					RoundManager.Instance.current = ball;
					ready = true;
				}
				else
				{
					RoundManager.Instance.current = null;
					ready = false;
					waiting = waitComplete = false;
				}
			}
			*/
		}
		else
		{
			return;
		}

		if (ready)	//选中小球，可以拖动
		{
			//点击，冷却时间未到
			if (Input.GetButtonDown("Fire1") && !waitComplete)
			{
				waitTimer = 0f;
				switchButtonTimer = 0f;
				waiting = true; //等待按下足够长的时间
				SwitchButton(false);
			}
			//冷却时间已到
			if (Input.GetButton("Fire1") && waitComplete)
			{
				endPos = Input.mousePosition;
				angle = Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x);
				length = Vector2.Distance(endPos, startPos) < aimRingRadius ? Vector2.Distance(endPos, startPos) : aimRingRadius;
				RotateArrow(length);
			}
			//松开鼠标
			if (Input.GetButtonUp("Fire1"))
			{
				if (waitComplete)
				{
					float degree = angle * Mathf.Rad2Deg;
					int degreeInt = (int)(degree * 10000);

					int forceInt = (int)(length / aimRingRadius * 10000);

					print("LocalDeg: " + degreeInt);
					print("LocalForce: " + forceInt);
					RoundManager.Instance.current.bl.Launch(degreeInt, forceInt);
					/*
					HideArrow();
					SwitchBotton(false);
					active = ready = false;
					waitComplete = false;
					*/
					Halt();
					RoundManager.Instance.firing = true;
				}
				//选好小球的按钮在松开鼠标后，需要一段极短的时间屏蔽松开鼠标的操作，否则会直接触发else if.
				else if (waitTimer > 0.05f && !buttonPressed)
				{
					waitTimer = 0f;
					ready = false;
					waiting = false;
					RoundManager.Instance.GetBall(-1);
					SwitchButton(true);
				}
			}
		}
		
		if (waiting)
		{
			waitTimer += Time.deltaTime;
			if (waitTimer > waitTime)
			{
				waitComplete = true;
				waiting = false;
				ShowArrow();
			}
		}
		//按下按钮，重置冷却与等待
		if (buttonPressed)
		{
			switchButtonTimer += Time.deltaTime;
			if(switchButtonTimer > 0.05f)
			{
				buttonPressed = false;
			}
		}
	}

	private void ShowArrow()
	{
		arrow.position = RoundManager.Instance.current.transform.position;
		arrow.localScale = Vector3.zero;
		startPos = Input.mousePosition;
		arrow.gameObject.SetActive(true);
	}

	private void HideArrow()
	{
		arrow.gameObject.SetActive(false);
	}

	private void RotateArrow(float length)
	{
		arrow.rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg + 180f);
		arrow.localScale = new Vector3(length / aimRingRadius * maxArrowScale, length / aimRingRadius * maxArrowScale);
	}

	public void SwitchButton(bool b)
	{
		foreach (Button button in ballSelectButtons)
		{
			button.interactable = b;
		}
	}

	public void Halt()
	{
		HideArrow();
		SwitchButton(false);
		active = false;
		ready = false;
		waiting = waitComplete = false;
		waitTimer = 0f;
	}
}

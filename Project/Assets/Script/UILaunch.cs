using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UILaunch : MonoBehaviour
{
	//public bool active; //Ready for selection.
	public bool ready;  //Ready for launch.
	public Transform arrow;
	public float waitTime;
	public float aimRingRadius;
	public float maxArrowScale;
	public Button[] ballSelectButtons;
	public Image ballLocIcon;
	public bool buttonPressed;
	public Color normalColor;
	public Color selectedColor;

	private Vector2 startPos;
	private Vector2 endPos;
	private float angle;
	private float length;
	private GameObject ui;
	private bool waiting;
	private bool waitComplete;
	private float waitTimer;    //检测按下持续时间的计时器
	private float switchButtonTimer;    //在极短时间内屏蔽更换按钮时的鼠标按下操作，否则将导致点击别的小球按钮也会判定为取消选中

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

	//void Update()
	//{
	//	if (active)
	//	{
	//		//直接点选小球选中，弃用。
	//		/*
	//		if (Input.GetButtonDown("Fire1"))
	//		{
	//			Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
	//			//Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
	//			RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, 100f, LayerMask.GetMask("Player"));
	//			if (hit && hit.transform.GetComponent<Ball>().isPlayerA)
	//			{
	//				Ball ball = hit.transform.GetComponent<Ball>();
	//				RoundManager.Instance.current = ball;
	//				ready = true;
	//			}
	//			else
	//			{
	//				RoundManager.Instance.current = null;
	//				ready = false;
	//				waiting = waitComplete = false;
	//			}
	//		}
	//		*/
	//	}
	//	else
	//	{
	//		return;
	//	}

	//	if (ready)  //选中小球，可以拖动
	//	{
	//		//点击，每次点击重置冷却
	//		/*
	//		if (Input.GetButtonDown("Fire1"))
	//		{
	//			waitTimer = 0f;
	//			switchButtonTimer = 0f;
	//			waiting = true; //等待按下足够长的时间
	//							//SwitchButton(false);
	//		}
	//		*/
	//		if(Input.GetButtonDown("Fire1"))
	//		{
	//			Debug.Log(GetCurrentUI().name);
	//		}

	//		//一直按着鼠标，冷却时间已到
	//		if (Input.GetButton("Fire1") && waitComplete)
	//		{
	//			endPos = Input.mousePosition;
	//			angle = Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x);
	//			length = Vector2.Distance(endPos, startPos) < aimRingRadius ? Vector2.Distance(endPos, startPos) : aimRingRadius;
	//			RotateArrow(length);
	//		}
	//		//松开鼠标
	//		if (Input.GetButtonUp("Fire1"))
	//		{
	//			//发射
	//			if (waitComplete)
	//			{
	//				float degree = angle * Mathf.Rad2Deg;
	//				int degreeInt = (int)(degree * 10000);

	//				int forceInt = (int)(length / aimRingRadius * 10000);
	//				if (!GameManager.Instance.isPlayerA)
	//				{
	//					degreeInt += 180 * 10000;
	//				}
	//				print("LocalDeg: " + degreeInt);
	//				print("LocalForce: " + forceInt);
	//				RoundManager.Instance.current.bl.Launch(degreeInt, forceInt);
	//				/*
	//				HideArrow();
	//				SwitchBotton(false);
	//				active = ready = false;
	//				waitComplete = false;
	//				*/
	//				Halt();
	//				RoundManager.Instance.firing = true;
	//			}
	//			//未到冷却时间就松开，同时判断按按钮的极小操作时间已过，重置小球选择与按钮状态
	//			else if (switchButtonTimer > 0.05f && waitTimer < waitTime)
	//			{
	//				waitTimer = 0f;
	//				ready = false;
	//				waiting = false;
	//				RoundManager.Instance.GetBall(-1);
	//				ChangeBallButton(-1);
	//				SwitchButtonInteract(true);
	//			}
	//			//切换了小球，重置选择状态（由按钮事件完成）
	//			else if(switchButtonTimer <= 0.05f)
	//			{
	//				//ChangeBallButton();
	//			}
	//		}
	//	}
	//	//等到冷却完成，停止计时准备发射
	//	if (waiting)
	//	{
	//		waitTimer += Time.deltaTime;
	//		if (waitTimer > waitTime)
	//		{
	//			waitComplete = true;
	//			waiting = false;
	//			ShowArrow();
	//		}
	//	}

	//	if (buttonPressed)
	//	{
	//		switchButtonTimer += Time.deltaTime;
	//		if (switchButtonTimer > 0.05f)
	//		{
	//			buttonPressed = false;
	//		}
	//	}
	//}

	private void Update()
	{
		if (!ready)
		{
			return;
		}

		if (Input.GetButtonDown("Fire1"))
		{
			ui = GetCurrentUI();
			waitTimer = 0f;
			waiting = true;
			waitComplete = false;
			if (ui.CompareTag("BallSelectButton"))
			{
				int id = ui.transform.GetSiblingIndex();
				RoundManager.Instance.GetBall(id);
			}
			else if (ui.CompareTag("SkillButton"))
			{
				//Activate the skill.
			}
			else
			{
				ui = null;
			}
		}
		if (Input.GetButton("Fire1") && waitComplete)
		{
			endPos = Input.mousePosition;
			angle = Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x);
			length = Vector2.Distance(endPos, startPos) < aimRingRadius ? Vector2.Distance(endPos, startPos) : aimRingRadius;
			RotateArrow(length);
		}
		if (Input.GetButtonUp("Fire1"))
		{
			if (waitComplete)
			{
				float degree = angle * Mathf.Rad2Deg;
				int degreeInt = (int)(degree * 10000);

				int forceInt = (int)(length / aimRingRadius * 10000);
				if (!GameManager.Instance.isPlayerA)
				{
					degreeInt += 180 * 10000;
				}
				print("LocalDeg: " + degreeInt);
				print("LocalForce: " + forceInt);
				RoundManager.Instance.current.bl.Launch(degreeInt, forceInt);
				RoundManager.Instance.firing = true;
				Halt();
			}
			else
			{
				waitTimer = 0f;
				waiting = false;
				if(ui == null)
				{
					RoundManager.Instance.GetBall(-1);
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
		float fireAngle = GameManager.Instance.isPlayerA ? angle : angle + Mathf.PI;
		arrow.rotation = Quaternion.Euler(0f, 0f, fireAngle * Mathf.Rad2Deg + 180f);
		arrow.localScale = new Vector3(length / aimRingRadius * maxArrowScale, length / aimRingRadius * maxArrowScale);
	}

	public void SwitchButtonInteract(bool b)
	{
		foreach (Button button in ballSelectButtons)
		{
			button.interactable = b;
		}
	}

	public void ChangeBallButton(int num)
	{
		foreach (Button button in ballSelectButtons)
		{
			button.image.color = normalColor;
		}
		if (num < 0)
		{
			return;
		}
		ballSelectButtons[num].image.color = selectedColor;
		waitTimer = 0f;
		waiting = false;
	}

	public void Halt()
	{
		HideArrow();
		//SwitchButtonInteract(false);
		//active = false;
		ready = false;
		waiting = waitComplete = false;
		waitTimer = 0f;
		switchButtonTimer = 0f;
		ChangeBallButton(-1);
		ballLocIcon.gameObject.SetActive(false);
	}

	public GameObject GetCurrentUI()
	{
		PointerEventData eventData = new PointerEventData(EventSystem.current);
		eventData.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		List<RaycastResult> raycastResults = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, raycastResults);
		if (raycastResults.Count > 0)
		{
			return raycastResults[0].gameObject;
		}
		else
		{
			return null;
		}
	}
}

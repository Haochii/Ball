using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILaunch : MonoBehaviour
{
	public bool active; //Ready for selection.
	public bool ready;  //Ready for launch.
	public Transform arrow;
	public float waitTime;
	public float aimRingRadius;
	public float maxArrowScale;

	private Vector2 startPos;
	private Vector2 endPos;
	private float angle;
	private float length;
	private bool waiting;
	private bool waitComplete;
	private float timer;

	void Start()
	{
		arrow.gameObject.SetActive(false);
		ready = false;
		waiting = waitComplete = false;
		timer = 0f;
	}

	void Update()
	{
		if (active)
		{
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
		}
		else
		{
			return;
		}

		if (ready)
		{
			if (Input.GetButtonDown("Fire1") && !waitComplete)
			{
				timer = 0f;
				waiting = true;
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
					RoundManager.Instance.current.bl.Launch(angle, length / aimRingRadius);
					HideArrow();
					active = ready = false;
					waitComplete = false;
					RoundManager.Instance.firing = true;
				}
				else
				{
					waiting = false;
				}
			}
		}

		if (waiting)
		{
			timer += Time.deltaTime;
			if (timer > waitTime)
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
		arrow.rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg + 180f);
		arrow.localScale = new Vector3(length / aimRingRadius * maxArrowScale, length / aimRingRadius * maxArrowScale);
	}

	public void Halt()
	{
		HideArrow();
		active = false;
		ready = false;
		waiting = waitComplete = false;
		timer = 0f;
	}
}

using UnityEngine;

public enum WallType
{
	Empty,
	Unbreakable,
	Breakable,
	Respawnable
}

public class WallTile : MonoBehaviour
{
	public int x;
	public int y;
	public bool breakable;
	public int life;
	public int maxLife;
	public bool respawnable;
	public int respawnLeft;
	public int respawnRounds;
	public Sprite []wall;//0unbreakable,1breakable1
	private SpriteRenderer sr;

	private void Start()
	{
		sr = GetComponent<SpriteRenderer>();
		if(breakable)
		{
			life = maxLife;
			sr.sprite = wall[maxLife];
		}
		if(respawnable)
		{
			RoundManager.Instance.nextRound.AddListener(Restore);
			sr.sprite = wall[life];
		}
	}

	private void OnDestroy()
	{
		if (respawnable)
		{
			RoundManager.Instance.nextRound.RemoveAllListeners();
		}
	}

	public void Damage()
	{
		if (breakable)
		{
			life--;
			if (life == 0)
			{
				sr.sprite = wall[life];
				Break();
			}
			if(life<0)
            {
				Debug.LogError("Wall's life cannot be negative!");
            }
			sr.sprite = wall[life];
		}
	}

	public void Break()
	{
		if(respawnable)
		{
			respawnLeft = respawnRounds;
		}
		gameObject.GetComponent<Collider2D>().enabled = false;
		if(!respawnable)
			gameObject.SetActive(false);
		//particle
	}

	public void Restore()
	{
		if(gameObject.GetComponent<Collider2D>().enabled)
		{
			return;
		}

		respawnLeft--;
		if(respawnLeft <= 0)
		{
			life = maxLife;
			gameObject.GetComponent<Collider2D>().enabled = true;
			sr.sprite = wall[maxLife];
			//particle
		}
	}
}
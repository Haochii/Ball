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

	private void Start()
	{
		if(breakable)
		{
			life = maxLife;
		}
		if(respawnable)
		{
			RoundManager.Instance.nextRound.AddListener(Restore);
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
			if (life <= 0)
			{
				Break();
			}
		}
	}

	public void Break()
	{
		if(respawnable)
		{
			respawnLeft = respawnRounds;
		}
		gameObject.SetActive(false);
		//particle
	}

	public void Restore()
	{
		if(gameObject.activeSelf)
		{
			return;
		}

		respawnLeft--;
		if(respawnLeft <= 0)
		{
			life = maxLife;
			gameObject.SetActive(true);
			//particle
		}
	}
}

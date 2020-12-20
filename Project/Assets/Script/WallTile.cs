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
	private int life;
	public int maxLife;
	public bool respawnable;
	public int respawnLeft;
	public int respawnRounds;
	public Sprite[] walls;
	private SpriteRenderer sr;

	private void Start()
	{
		sr = GetComponent<SpriteRenderer>();
		if(breakable)
		{
			life = maxLife;
			sr.sprite = walls[maxLife];
		}
		if(respawnable)
		{
			RoundManager.Instance.nextRound.AddListener(Restore);
			sr.sprite = walls[life];
		}
	}

    private void Update()
    {
        if(!GameManager.Instance.isPlayerA)
        {
			sr.flipY = true;
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
				Break();
			}
			if(life < 0)
            {
				Debug.LogError("wall's life cannot be negative!");
            }
			sr.sprite = walls[life];
		}
	}

	public void Break()
	{
		if(respawnable)
		{
			respawnLeft = respawnRounds;
			sr.sprite = walls[0];
			gameObject.GetComponent<Collider2D>().enabled = false;
		}
        else
        {
			gameObject.SetActive(false);
		}
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
			sr.sprite = walls[maxLife];
			//particle
		}
	}
}

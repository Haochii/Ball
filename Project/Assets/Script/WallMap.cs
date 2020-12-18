using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallMap : MonoBehaviour
{
	public Vector3 cellSize;
	public Vector3 origin;
	public Vector3 offset;
	public Transform tileHolder;
	public GameObject Tile;

	private int type;
	private int life;

	public void PaintTile(int[,] cord)
	{
		for (int i = cord.GetLength(1); i > 0; i--)
		{
			for (int j = 0; j < cord.GetLength(0); j++)
			{
				type = cord[i, j] / 10;
				life = cord[i, j] % 10;
				SetTile(new Vector3Int(j, cord.GetLength(1) - i - 1, 0), type, life);
			}
		}
	}

	public void SetTile(Vector3Int cord, int type, int life)
	{
		if(type <= 0)
		{
			return;
		}

		switch (type)
		{
			case (int)WallType.Unbreakable:
				GameObject t = Instantiate(Tile, cord + offset, Quaternion.identity, tileHolder);
				break;
				
		}
	}
}

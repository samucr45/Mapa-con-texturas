using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
	
    //Prefab del tile
	public Transform tilePrefab;

    //Prefab de los edificios
	public Transform [] buildingPrefab;

    //Tamaño del mapa
	public Vector2 mapSize;
	
    //Tamaño del tile
	[Range(0,1)]
	public float outlinePercent;

    //Cantidad de obstaculos generados
	[Range(0,1)]
	public float obstaclePercent;

	List<Coord> allTileCoords;
	Queue<Coord> shuffledTileCoords;

	public int seed = 10;
	Coord mapCentre;
	
	void Start()
    {
		GenerateMap ();
	}
	
	public void GenerateMap()
    {

		allTileCoords = new List<Coord> ();
		for (int x = 0; x < mapSize.x; x ++) {
			for (int y = 0; y < mapSize.y; y ++) {
				allTileCoords.Add(new Coord(x,y));
			}
		}
		shuffledTileCoords = new Queue<Coord> (Utility.ShuffleArray (allTileCoords.ToArray (), seed));
		mapCentre = new Coord ((int)mapSize.x / 2, (int)mapSize.y / 2);

		string holderName = "Generated Map";
		if (transform.Find (holderName)) {
			DestroyImmediate (transform.Find (holderName).gameObject);
		}
		
		Transform mapHolder = new GameObject (holderName).transform;
		mapHolder.parent = transform;
		
		for (int x = 0; x < mapSize.x; x ++)
        {
			for (int y = 0; y < mapSize.y; y ++)
            {
				Vector3 tilePosition = CoordToPosition(x,y);
				Transform newTile = Instantiate (tilePrefab, tilePosition, Quaternion.Euler (-90,0,0)) as Transform;
				newTile.localScale = Vector3.one * (1 - outlinePercent);
				newTile.parent = mapHolder;
			}
		}

		bool[,] obstacleMap = new bool[(int)mapSize.x,(int)mapSize.y];

		int obstacleCount = (int)(mapSize.x * mapSize.y * obstaclePercent);
		int currentObstacleCount = 0;

		for (int i =0; i < obstacleCount; i ++)
        {
			Coord randomCoord = GetRandomCoord();
			obstacleMap[randomCoord.x,randomCoord.y] = true;
			currentObstacleCount ++;

			if (randomCoord != mapCentre && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
            {
				Vector3 obstaclePosition = CoordToPosition(randomCoord.x,randomCoord.y);
                int randomBuilding = Random.Range(0, buildingPrefab.Length);
				Transform newBuilding = Instantiate(buildingPrefab[randomBuilding], obstaclePosition, Quaternion.Euler(-90,0,0)) as Transform;
                newBuilding.localScale = Vector3.one * (1 - outlinePercent);
                newBuilding.parent = mapHolder;
			}
			else
            {
				obstacleMap[randomCoord.x,randomCoord.y] = false;
				currentObstacleCount --;
			}
		}
		
	}

	bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
		bool[,] mapFlags = new bool[obstacleMap.GetLength(0),obstacleMap.GetLength(1)];
		Queue<Coord> queue = new Queue<Coord> ();
		queue.Enqueue (mapCentre);
		mapFlags [mapCentre.x, mapCentre.y] = true;

		int accessibleTileCount = 1;

		while (queue.Count > 0)
        {
			Coord tile = queue.Dequeue();

			for (int x = -1; x <= 1; x ++) {
				for (int y = -1; y <= 1; y ++) {
					int neighbourX = tile.x + x;
					int neighbourY = tile.y + y;
					if (x == 0 || y == 0) {
						if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1)) {
							if (!mapFlags[neighbourX,neighbourY] && !obstacleMap[neighbourX,neighbourY]) {
								mapFlags[neighbourX,neighbourY] = true;
								queue.Enqueue(new Coord(neighbourX,neighbourY));
								accessibleTileCount ++;
							}
						}
					}
				}
			}
		}

		int targetAccessibleTileCount = (int)(mapSize.x * mapSize.y - currentObstacleCount);
		return targetAccessibleTileCount == accessibleTileCount;
	}

	Vector3 CoordToPosition(int x, int y) {
		return new Vector3 (-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y);
	}

	public Coord GetRandomCoord()
    {
		Coord randomCoord = shuffledTileCoords.Dequeue ();
		shuffledTileCoords.Enqueue (randomCoord);
		return randomCoord;
	}

	public struct Coord
    {
		public int x;
		public int y;

		public Coord(int _x, int _y) {
			x = _x;
			y = _y;
		}

		public static bool operator ==(Coord c1, Coord c2) {
			return c1.x == c2.x && c1.y == c2.y;
		}

		public static bool operator !=(Coord c1, Coord c2) {
			return !(c1 == c2);
		}

	}
}
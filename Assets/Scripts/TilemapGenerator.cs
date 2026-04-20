using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TilemapGenerator : MonoBehaviour
{
    [Header("Tilemap References")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public Tilemap objectsTilemap;

    [Header("Tiles")]
    public TileBase floorTile;
    public TileBase wallTile;
    public TileBase[] decorativeTiles;
    public TileBase doorTile;
    public TileBase chestTile;

    [Header("Map Settings")]
    public int mapWidth = 40;
    public int mapHeight = 30;
    public int minRoomSize = 5;
    public int maxRoomSize = 10;
    public int roomAttempts = 20;

    [Header("Decoration")]
    [Range(0f, 1f)]
    public float decorationChance = 0.1f;
    public int chestCount = 2;

    private bool[,] map;
    private List<Room> rooms = new List<Room>();

    private class Room
    {
        public int x, y, width, height;
        public Vector2Int Center => new Vector2Int(x + width / 2, y + height / 2);

        public Room(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public bool Intersects(Room other)
        {
            return x < other.x + other.width + 1 &&
                   x + width + 1 > other.x &&
                   y < other.y + other.height + 1 &&
                   y + height + 1 > other.y;
        }
    }

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        if (floorTilemap == null || wallTilemap == null)
        {
            Debug.LogError("Tilemaps not assigned!");
            return;
        }

        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        if (objectsTilemap != null)
            objectsTilemap.ClearAllTiles();

        map = new bool[mapWidth, mapHeight];
        rooms.Clear();

        GenerateRooms();
        ConnectRooms();
        PlaceWalls();
        PlaceDecorations();
        PlaceChests();

        Debug.Log($"Generated dungeon with {rooms.Count} rooms");
    }

    void GenerateRooms()
    {
        for (int i = 0; i < roomAttempts; i++)
        {
            int width = Random.Range(minRoomSize, maxRoomSize + 1);
            int height = Random.Range(minRoomSize, maxRoomSize + 1);
            int x = Random.Range(1, mapWidth - width - 1);
            int y = Random.Range(1, mapHeight - height - 1);

            Room newRoom = new Room(x, y, width, height);

            bool canPlace = true;
            foreach (Room room in rooms)
            {
                if (newRoom.Intersects(room))
                {
                    canPlace = false;
                    break;
                }
            }

            if (canPlace)
            {
                rooms.Add(newRoom);
                CarveRoom(newRoom);
            }
        }
    }

    void CarveRoom(Room room)
    {
        for (int x = room.x; x < room.x + room.width; x++)
        {
            for (int y = room.y; y < room.y + room.height; y++)
            {
                map[x, y] = true;
                floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
            }
        }
    }

    void ConnectRooms()
    {
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            Vector2Int start = rooms[i].Center;
            Vector2Int end = rooms[i + 1].Center;

            if (Random.value > 0.5f)
            {
                CarveHorizontalCorridor(start.x, end.x, start.y);
                CarveVerticalCorridor(start.y, end.y, end.x);
            }
            else
            {
                CarveVerticalCorridor(start.y, end.y, start.x);
                CarveHorizontalCorridor(start.x, end.x, end.y);
            }
        }
    }

    void CarveHorizontalCorridor(int x1, int x2, int y)
    {
        int start = Mathf.Min(x1, x2);
        int end = Mathf.Max(x1, x2);

        for (int x = start; x <= end; x++)
        {
            if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
            {
                map[x, y] = true;
                floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
            }
        }
    }

    void CarveVerticalCorridor(int y1, int y2, int x)
    {
        int start = Mathf.Min(y1, y2);
        int end = Mathf.Max(y1, y2);

        for (int y = start; y <= end; y++)
        {
            if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
            {
                map[x, y] = true;
                floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
            }
        }
    }

    void PlaceWalls()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (!map[x, y])
                {
                    if (HasAdjacentFloor(x, y))
                    {
                        wallTilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                    }
                }
            }
        }
    }

    bool HasAdjacentFloor(int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;

                if (nx >= 0 && nx < mapWidth && ny >= 0 && ny < mapHeight)
                {
                    if (map[nx, ny])
                        return true;
                }
            }
        }
        return false;
    }

    void PlaceDecorations()
    {
        if (decorativeTiles == null || decorativeTiles.Length == 0)
            return;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (map[x, y] && Random.value < decorationChance)
                {
                    TileBase decorTile = decorativeTiles[Random.Range(0, decorativeTiles.Length)];
                    if (objectsTilemap != null)
                        objectsTilemap.SetTile(new Vector3Int(x, y, 0), decorTile);
                }
            }
        }
    }

    void PlaceChests()
    {
        if (chestTile == null || objectsTilemap == null || rooms.Count == 0)
            return;

        int placed = 0;
        int attempts = 0;
        int maxAttempts = chestCount * 10;

        while (placed < chestCount && attempts < maxAttempts)
        {
            attempts++;
            Room room = rooms[Random.Range(0, rooms.Count)];
            int x = Random.Range(room.x + 1, room.x + room.width - 1);
            int y = Random.Range(room.y + 1, room.y + room.height - 1);

            Vector3Int pos = new Vector3Int(x, y, 0);
            if (objectsTilemap.GetTile(pos) == null)
            {
                objectsTilemap.SetTile(pos, chestTile);
                placed++;
            }
        }
    }

    [ContextMenu("Regenerate Map")]
    public void RegenerateMap()
    {
        GenerateMap();
    }
}

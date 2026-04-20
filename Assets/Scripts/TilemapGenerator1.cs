using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGenerator : MonoBehaviour
{
    [Header("Tilemap References")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;

    [Header("Tiles")]
    public TileBase floorTile;
    public TileBase wallTile;

    [Header("Room Settings")]
    public int roomWidth = 20;
    public int roomHeight = 15;

    void Start()
    {
        GenerateRoom();
    }

    public void GenerateRoom()
    {
        if (floorTilemap == null || wallTilemap == null)
        {
            Debug.LogError("Tilemaps not assigned!");
            return;
        }

        // Clear existing tiles
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        // Generate floor
        for (int x = 0; x < roomWidth; x++)
        {
            for (int y = 0; y < roomHeight; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);

                // Place walls on borders
                if (x == 0 || x == roomWidth - 1 || y == 0 || y == roomHeight - 1)
                {
                    if (wallTile != null)
                        wallTilemap.SetTile(tilePosition, wallTile);
                }
                else
                {
                    // Place floor tiles
                    if (floorTile != null)
                        floorTilemap.SetTile(tilePosition, floorTile);
                }
            }
        }

        Debug.Log($"Generated room: {roomWidth}x{roomHeight}");
    }

    // Call this from Unity Editor or Inspector
    [ContextMenu("Regenerate Room")]
    public void RegenerateRoom()
    {
        GenerateRoom();
    }
}

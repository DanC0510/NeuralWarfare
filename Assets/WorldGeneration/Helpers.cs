using UnityEngine;

public static class Helpers
{
    public static int mod(int x, int m) {
        return (x%m + m)%m;
    }
    
    public static Tile GetTileAtWorldPos(Vector2 worldPos)
    {
        int width = WorldGenerator.Instance.ActualWidth;
        int height = WorldGenerator.Instance.ActualHeight;
        float hexSize = WorldGenerator.Instance.HexSize;
        Tile[] tiles = WorldGenerator.Instance.Tiles;
        float hexWidth = Mathf.Sqrt(3f) * hexSize;
        float hexHeight = 2f * hexSize;

        // Estimate row first (without offset correction)
        int row = Mathf.RoundToInt(worldPos.y / (hexHeight * 0.75f));

        // Correct x for odd row offset
        float xCorrected = worldPos.x - (Helpers.mod(row, 2) == 1 ? hexWidth * 0.5f : 0f);
        int col = Mathf.RoundToInt(xCorrected / hexWidth);

        if (col < 0 || col >= width || row < 0 || row >= height) { return null; }

        int index = row * width + col;
        return tiles[index];
    }
    
    public static Vector3 TileToWorldPos(Vector2 tilePos)
    {
        float hexSize = WorldGenerator.Instance.HexSize;
        float hexWidth = Mathf.Sqrt(3f) * hexSize;
        float hexHeight = 2f * hexSize;

        int col = (int)tilePos.x;
        int row = (int)tilePos.y;

        float x = col * hexWidth;
        float z = row * hexHeight * 0.75f;

        if (row % 2 == 1)
            x += hexWidth * 0.5f;

        return new Vector3(x, 0.5f, z);
    }
    
    public static Tile GetTileAtTilePos(Vector2 tilePos)
    {
        int width = WorldGenerator.Instance.ActualWidth;
        int height = WorldGenerator.Instance.ActualHeight;
        Tile[] tiles = WorldGenerator.Instance.Tiles;

        int col = (int)tilePos.x;
        int row = (int)tilePos.y;

        if (col < 0 || col >= width || row < 0 || row >= height) { return null; }

        int index = row * width + col;
        return tiles[index];
    }
}

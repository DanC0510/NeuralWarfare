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
}

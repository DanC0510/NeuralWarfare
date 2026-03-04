using UnityEngine;

public class PathfindingGraph
{
    private Tile[] tiles;
    private int width;
    private int height;

    public PathfindingGraph(Tile[] tiles, int width, int height)
    {
        this.tiles = tiles;
        this.width = width;
        this.height = height;
    }

    public void BuildGraph()
    {
        foreach (Tile tile in tiles)
        {
            for (int i = 0; i < 6; i++)
            {
                tile.NeighbourTiles.Add(GetNeighbour(tile, i));
            }
        }
    }
    public Tile GetNeighbour(Tile currentTile, int direction)
    {
        Vector2 neighbourPos = Vector2.zero;
        int col = (int)currentTile.Position.x;
        int row = (int)currentTile.Position.y;
        
        bool isOddRow = row % 2 == 1;
        
        if (direction == 0) // NE
        {
            neighbourPos = new Vector2(col + (isOddRow ? 1 : 0), row + 1);
        }
        else if (direction == 1) // E
        {
            neighbourPos = new Vector2(col + 1, row);
        }
        else if (direction == 2) // SE
        {
            neighbourPos = new Vector2(col + (isOddRow ? 1 : 0), row - 1);
        }
        else if (direction == 3) // SW
        {
            neighbourPos = new Vector2(col + (isOddRow ? 0 : -1), row - 1);
        }
        else if (direction == 4) // W
        {
            neighbourPos = new Vector2(col - 1, row);
        }
        else if (direction == 5) // NW
        {
            neighbourPos = new Vector2(col + (isOddRow ? 0 : -1), row + 1);
        }
        
        if (neighbourPos.x < 0 || neighbourPos.x >= width || neighbourPos.y < 0 || neighbourPos.y >= height)
        {
            return null;
        }
        
        int index = (int)neighbourPos.y * width + (int)neighbourPos.x;
        if (index >= 0 && index < tiles.Length)
        {
            return tiles[index];
        }

        return null;
    }
}

using System.Collections.Generic;
using UnityEngine;

public class PathfindingGraph : MonoBehaviour
{
    private static PathfindingGraph _instance;
    public static PathfindingGraph Instance => _instance;
    
    private List<Tile> movementPath;
    private Tile[] tiles;
    private int width;
    private int height;
    
    private void Awake()
    {
        _instance = this;
    }

    public void Init(Tile[] tiles, int width, int height)
    {
        this.tiles = tiles;
        this.width = width;
        this.height = height;
    }

    public void BuildGraph()
    {
        foreach (Tile tile in tiles)
        {
            if (tile == null) { continue; } //stops errors at edges of map
            for (int direction = 0; direction < 6; direction++)
            {
                Tile neighbour = GetNeighbour(tile, direction);
                tile.NeighbourTiles[direction] = neighbour;
            }
        }
    }
    public Tile GetNeighbour(Tile currentTile, int direction)
    {
        Vector2 neighbourPos = Vector2.zero;
        int col = (int)currentTile.Position.x;
        int row = (int)currentTile.Position.y;
        
        bool isOddRow = Helpers.mod(row,2) == 1;
        
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

    public List<Tile> GetPath(Vector2 startPosition, Vector2 targetPosition)
    {
        return FindPath(Helpers.GetTileAtWorldPos(startPosition), 
            Helpers.GetTileAtWorldPos(targetPosition));
    }
    
    private List<Tile> FindPath(Tile start, Tile target)
    {
        if(start == null || target == null) { return null; }
        
        Dictionary<Tile, float> gCost = new Dictionary<Tile, float>();
        Dictionary<Tile, float> fCost = new Dictionary<Tile, float>();
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
        
        List<Tile> openSet = new List<Tile>();
        HashSet<Tile> closedSet = new HashSet<Tile>();

        gCost[start] = 0;
        fCost[start] = Heuristic(start, target);
        openSet.Add(start);

        while (openSet.Count > 0)
        {
            Tile current = GetLowestFCost(openSet, fCost);

            if (current == target) { return ReconstructPath(cameFrom, current); }

            openSet.Remove(current);
            closedSet.Add(current);
            
            foreach(Tile neighbour in current.NeighbourTiles)
            {
                if (neighbour == null || closedSet.Contains(neighbour)) { continue; }
                if (neighbour.IsOccupied && neighbour != target) { continue; }

                float tentativeG = gCost[current] + GetMoveCost(neighbour);

                if (!gCost.ContainsKey(neighbour) || tentativeG < gCost[neighbour])
                {
                    cameFrom[neighbour] = current;
                    gCost[neighbour] = tentativeG;
                    fCost[neighbour] = tentativeG + Heuristic(neighbour, target);

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }
        
        return null;
    }
    
    private float GetMoveCost(Tile tile)
    { //higher cost means slower movement, 0 means free movement 
        return tile.Biome switch
        {
            Tile.TileBiome.Deep_Water => 9999f, //add conditional logic for when deep sea travel is unlocked
            Tile.TileBiome.Water => 9999f, 
            Tile.TileBiome.Snow => 1.5f,
            Tile.TileBiome.River => 2.25f,
            Tile.TileBiome.Grass => 1f,
            Tile.TileBiome.Sand => 1.25f,
            _ => 0f
        };
    }

    private float Heuristic(Tile a, Tile b)
    {
        return Vector2.Distance(a.Position, b.Position);
    }

    private Tile GetLowestFCost(List<Tile> openSet, Dictionary<Tile, float> fCost)
    {
        Tile lowest = openSet[0];
        foreach (Tile tile in openSet)
        {
            if (fCost.ContainsKey(tile) && fCost[tile] < fCost[lowest])
            {
                lowest = tile;
            }
        }
        return lowest;
    }

    private List<Tile> ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile current)
    {
        List<Tile> path = new List<Tile>();
        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Add(current);
        path.Reverse();
        return path;
    }
}

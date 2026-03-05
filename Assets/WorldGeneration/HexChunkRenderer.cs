using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexChunkRenderer : MonoBehaviour
{
    [Header("Chunk Settings")]
    public static int chunkSize = 16;
    public float hexSize = 0.5f;
    [SerializeField] private int chunkQ;
    [SerializeField] private int chunkR;
    [SerializeField] private Mesh mesh;
    private List<Vector3> vertices;
    private List<int> triangles;
    private List<Vector2> uvs;
    private Tile[] tiles; //making this serializefield slows down the inspector a lot
    private int mapWidth;
    private int mapHeight;
    
    [Header("Atlas")]
    [SerializeField] private int atlasColumns = 4;
    [SerializeField] private int atlasRows = 4;
    [SerializeField, Range(0f, 0.5f)] private float uvInnerMargin = 0.08f; // shrink inside the cell
    [SerializeField] private int degOffset;
    
    [Header("Debug")]
    [SerializeField] private bool showBiomeColours;

    public void Init(Tile[] tiles, int mapWidth, int mapHeight, int chunkQ, int chunkR)
    {
        this.tiles = tiles;
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;
        this.chunkQ = chunkQ;
        this.chunkR = chunkR;
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        gameObject.transform.position = OffsetToWorld(chunkQ * chunkSize, chunkR * chunkSize, hexSize);
        BuildMesh();
    }
    
    public void Refresh()
    {
        BuildMesh();
    }

    void BuildMesh()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();

        int baseCol = chunkQ * chunkSize;
        int baseRow = chunkR * chunkSize;

        for (int col = 0; col < chunkSize; col++)
        {
            for (int row = 0; row < chunkSize; row++)
            {
                int worldCol = baseCol + col;
                int worldRow = baseRow + row;
                
                if (worldCol >= mapWidth || worldRow >= mapHeight)
                    continue;
                
                int index = worldRow * mapWidth + worldCol;
                if (index < 0 || index >= tiles.Length)
                    continue;
                
                AddHex(tiles[index], col, row);
            }
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;

    }


    void AddHex(Tile tile, int col, int row)
    {
        Vector3 center = OffsetToWorld(col, row, hexSize);

        int vertStart = vertices.Count;
        vertices.Add(center);

        for (int i = 0; i < 6; i++)
        {
            vertices.Add(center + HexCorner(i, hexSize));
        }

        for (int i = 0; i < 6; i++)
        {
            triangles.Add(vertStart);
            triangles.Add(vertStart + 1 + ((i + 1) % 6));
            triangles.Add(vertStart + 1 + i);
        }

        if (showBiomeColours)
        {
            Vector2 uv = GetBiomeUV(tile.Biome);
            for (int i = 0; i < 7; i++)
                uvs.Add(uv);
        }
        else
        {
            Vector2[] hexUVs = GetHexUVs(tile);
            for (int i = 0; i < 7; i++)
                uvs.Add(hexUVs[i]);
        }

        degOffset = 30;
    }
    

    static Vector3 HexCorner(int i, float size)
    {
        float angle = Mathf.Deg2Rad * (60 * i + 30);
        return new Vector3(size * Mathf.Cos(angle), 0f, size * Mathf.Sin(angle));
    }

    static Vector3 OffsetToWorld(int col, int row, float size)
    {
        
        float hexWidth = Mathf.Sqrt(3f) * size;
        float hexHeight = 2f * size;
        
        float x = col * hexWidth;
        float z = row * hexHeight * 0.75f;
        
        // Shift odd rows to the right by half hex width
        if (row % 2 == 1)
        {
            x += hexWidth * 0.5f;
        }
        
        return new Vector3(x, 0f, z);
    }

    Vector2 GetBiomeUV(Tile.TileBiome biome)
    {
        int index = (int)biome;

        int x = index % atlasColumns;
        int y = index / atlasColumns;

        float cellWidth = 1f / atlasColumns;
        float cellHeight = 1f / atlasRows;

        float u = (x + 0.5f) * cellWidth;
        float v = 1f - (y + 0.5f) * cellHeight;

        return new Vector2(u, v);
    }
    
    static Vector2[] RotateHexUVs(Vector2[] uv, float degs)
    {
        if (uv == null || uv.Length != 7) return uv;
        if (Mathf.Approximately(degs, 0f)) return uv;

        Vector2[] rotated = new Vector2[7];
        rotated[0] = uv[0];

        float angle = Mathf.Deg2Rad * degs;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        for (int i = 1; i <= 6; i++)
        {
            Vector2 p = uv[i] - uv[0];
            float x = p.x * cos - p.y * sin;
            float y = p.x * sin + p.y * cos;
            rotated[i] = new Vector2(x, y) + uv[0];
        }
        return rotated;
    }
    
    static Vector2[] FlipHexUVsX(Vector2[] uv)
    {
        if (uv == null || uv.Length != 7) return uv;
        
        Vector2[] flipped = new Vector2[7];
        flipped[0] = uv[0]; // Center stays the same
        
        for (int i = 1; i <= 6; i++)
        {
            Vector2 p = uv[i] - uv[0];
            flipped[i] = new Vector2(-p.x, p.y) + uv[0];
        }
        
        return flipped;
    }
    
    static Vector2[] FlipHexUVsY(Vector2[] uv)
    {
        if (uv == null || uv.Length != 7) return uv;
        
        Vector2[] flipped = new Vector2[7];
        flipped[0] = uv[0]; // Center stays the same
        
        for (int i = 1; i <= 6; i++)
        {
            Vector2 p = uv[i] - uv[0];
            flipped[i] = new Vector2(p.x, -p.y) + uv[0];
        }
        
        return flipped;
    }
    
    static Vector2[] FlipHexUVsXY(Vector2[] uv)
    {
        // Flip both X and Y axes
        return FlipHexUVsY(FlipHexUVsX(uv));
    }
    
    List<Tile> river = new List<Tile>();
    (int, int) CalculateAtlasIndex(Tile tile)
    {
        int index = (int)tile.Biome;
        int rotation = 30;
        
        // Find this tile in its river chain
        Tile startTile = tile.IsStartingRiver ? tile : tile.RiverStart;
        
        if (startTile == null || startTile.RiverTiles == null)
            return (index, rotation);
            
        var river = new List<Tile>(startTile.RiverTiles);
        
        // Don't render rivers that are too short
        if (river.Count < 7)
        {
            return (3, rotation);
        }
        
        int tileIndex = river.IndexOf(tile);
        
        if (tileIndex == -1)
            return (index, rotation);
            
        // River start
        if (tileIndex == 0)
        {
            index = 8;
            if (river.Count > 1)
            {
                int dir = GetDirection(tile, river[1]);
                rotation = Helpers.mod(dir + 3, 6) * 60 + 30;
            }
            return (index, rotation);
        }
        
        // Get previous and next tiles
        Tile prev = river[tileIndex - 1];
        Tile next = (tileIndex < river.Count - 1) ? river[tileIndex + 1] : null;
        
        int prevDir = GetDirection(tile, prev);
        
        // River end (no next tile)
        if (next == null)
        {
            index = 5;
            rotation = prevDir * 60 + 30;
            return (index, rotation);
        }
        
        int nextDir = GetDirection(tile, next);
        
        // Calculate and apply the turn angle
        int dirDiff = Helpers.mod(nextDir - prevDir, 6);
        
        switch (dirDiff)
        {
            case 0: // Same direction (shouldn't happen)
                index = 5;
                rotation = nextDir * 60 + 30;
                break;
                
            case 3: // 180° - straight through
                index = 5;
                rotation = nextDir * 60 + 30;
                break;
                
            case 1: // 60° clockwise turn
                index = 6;
                rotation = Helpers.mod(nextDir - 0, 6) * 60 + 30;
                break;
                
            case 5: // 60° counter-clockwise turn
                index = 6;
                rotation = Helpers.mod(nextDir - 5, 6) * 60 + 30;
                break;
                
            case 2: // 120° clockwise turn
                index = 7;
                rotation = Helpers.mod(nextDir - 0, 6) * 60 + 30;
                break;
                
            case 4: // 120° counter-clockwise turn
                index = 7;
                rotation = Helpers.mod(nextDir - 4, 6) * 60 + 30;
                break;
        }
        
        return (index, rotation);
    }
    
    // Get the direction from 'from' tile to 'to' tile (0-5, where 0 is NE)
    int GetDirection(Tile from, Tile to)
    {
        int fromCol = (int)from.Position.x;
        int fromRow = (int)from.Position.y;
        int toCol = (int)to.Position.x;
        int toRow = (int)to.Position.y;
        
        int dCol = toCol - fromCol;
        int dRow = toRow - fromRow;
        
        bool isOddRow = fromRow % 2 == 1;
        
        // Direction 0: NE
        if (dRow == 1 && dCol == (isOddRow ? 1 : 0)) return 0;
        // Direction 1: E
        if (dRow == 0 && dCol == 1) return 1;
        // Direction 2: SE
        if (dRow == -1 && dCol == (isOddRow ? 1 : 0)) return 2;
        // Direction 3: SW
        if (dRow == -1 && dCol == (isOddRow ? 0 : -1)) return 3;
        // Direction 4: W
        if (dRow == 0 && dCol == -1) return 4;
        // Direction 5: NW
        if (dRow == 1 && dCol == (isOddRow ? 0 : -1)) return 5;
        
        return 0;
    }

    Vector2[] GetHexUVs(Tile tile)
    {
        int index = (int)tile.Biome;

        int rotation = 30; // Default rotation
        bool flipX = false;
        bool flipY = false;
        
        if (tile.Biome == Tile.TileBiome.River)
        {
            // Skip rendering if river is too short or invalid
            if (tile.RiverStart == null || tile.RiverStart.RiverTiles == null || tile.RiverStart.RiverTiles.Count < 2)
            {
                // Render as grass instead
                index = 3;
                tile.NotRiver();
            }
            else if (!tile.IsStartingRiver && tile.RiverStart != null && !tile.RiverStart.IsStartingRiver)
            {
                // River start is not valid, render as grass
                index = 3;
            }
            else
            {
                var result = CalculateAtlasIndex(tile);
                index = result.Item1;
                rotation = result.Item2;
                
                if (index == 6 || index == 7) // 60° or 120° bends
                {
                    flipX = true;
                    flipY = true;
                }
                else if (index == 8) // River start tile
                {
                    flipX = false;
                    flipY = false;
                }
            }
        }
        
        int cellX = index % atlasColumns;
        int cellY = index / atlasColumns;

        float cellW = 1f / atlasColumns;
        float cellH = 1f / atlasRows;

        const float eps = 0.0005f;

        float uCenter = (cellX + 0.5f) * cellW;
        float vCenter = 1f - (cellY + 0.5f) * cellH;

        float rU = (cellW * 0.5f) - (cellW * uvInnerMargin) - eps;
        float rV = (cellH * 0.5f) - (cellH * uvInnerMargin) - eps;

        Vector2[] uv = new Vector2[7];
        uv[0] = new Vector2(uCenter, vCenter);

        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.Deg2Rad * (60 * i + 30);
            float u = uCenter + Mathf.Cos(angle) * rU;
            float v = vCenter + Mathf.Sin(angle) * rV;
            uv[1 + i] = new Vector2(u, v);
        }

        // Apply flips before rotation
        if (flipX && flipY)
        {
            uv = FlipHexUVsXY(uv);
        }
        else if (flipX)
        {
            uv = FlipHexUVsX(uv);
        }
        else if (flipY)
        {
            uv = FlipHexUVsY(uv);
        }
        
        return RotateHexUVs(uv, rotation);
    }

}

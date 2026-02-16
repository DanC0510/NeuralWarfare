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
    [SerializeField] private Tile[] tiles;
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
            Vector2[] hexUVs = GetHexUVs(tile.Biome);
            for (int i = 0; i < 7; i++)
                uvs.Add(hexUVs[i]);
        }
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
    
    Vector2[] GetHexUVs(Tile.TileBiome biome)
    {
        int index = (int)biome;
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

        // Rotate first: apply degOffset once here
        return RotateHexUVs(uv, degOffset);
    }
    
    static Vector3 HexCornerRotated(int i, float size, float degs)
    {
        float angle = Mathf.Deg2Rad * (60 * i + 30 + degs);
        return new Vector3(size * Mathf.Cos(angle), 0f, size * Mathf.Sin(angle));
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

}

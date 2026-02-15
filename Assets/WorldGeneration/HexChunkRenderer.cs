using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexChunkRenderer : MonoBehaviour
{
    public static int chunkSize = 16;

    public float hexSize = 0.5f;
    public int chunkQ;
    public int chunkR;

    private Mesh mesh;
    private List<Vector3> vertices;
    private List<int> triangles;
    private List<Vector2> uvs;

    private Tile[] tiles;
    private int mapWidth;
    private int mapHeight;
    
    [Header("Atlas")]
    public int atlasColumns = 4;
    public int atlasRows = 4;
    
    [Header("Rivers")]
    public bool renderRivers = true;
    public Material riverMaterial;
    [Range(0.01f, 0.5f)] public float riverWidth;
    public float riverYOffset = 0.05f;
    private GameObject riverObject;
    private Mesh riverMesh;
    private MeshFilter riverMeshFilter;
    private MeshRenderer riverMeshRenderer;
    private readonly List<Vector3> riverVertices = new();
    private readonly List<int> riverTriangles = new();
    private readonly List<Vector2> riverUvs = new();
    public bool riversDoubleSided = true;

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
        EnsureRiverRenderer();
        BuildMesh();
    }

    void BuildMesh()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();
        riverVertices.Clear();
        riverTriangles.Clear();
        riverUvs.Clear();

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
                
                int index = worldCol * mapHeight + worldRow;
                if (index < 0 || index >= tiles.Length)
                    continue;
                
                AddHex(tiles[index], col, row);
                if (renderRivers)
                {
                    AddRivers(tiles[index], col, row);
                }
            }
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        
        if (renderRivers && riverMesh != null)
        {
            riverMesh.Clear();
            riverMesh.SetVertices(riverVertices);
            riverMesh.SetTriangles(riverTriangles, 0);
            riverMesh.SetUVs(0, riverUvs);
            riverMesh.RecalculateNormals();
        }
    }
    
    void EnsureRiverRenderer()
    {
        if (!renderRivers)
            return;

        if (riverObject == null)
        {
            riverObject = new GameObject("Rivers");
            riverObject.transform.SetParent(transform, false);
            riverObject.transform.localPosition = Vector3.zero;
            riverObject.transform.localRotation = Quaternion.identity;
            riverObject.transform.localScale = Vector3.one;

            riverMeshFilter = riverObject.AddComponent<MeshFilter>();
            riverMeshRenderer = riverObject.AddComponent<MeshRenderer>();
        }

        if (riverMesh == null)
        {
            riverMesh = new Mesh { name = "RiverMesh" };
            riverMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        riverMeshFilter.sharedMesh = riverMesh;

        // Ensure we actually have something to render with
        if (riverMaterial != null)
        {
            riverMeshRenderer.sharedMaterial = riverMaterial;
        }
        else
        {
            // Fallback visible material (Built-in/URP usually has "Sprites/Default")
            var fallback = new Material(Shader.Find("Universal Render Piperline/Lit"));
            fallback.color = Color.blue;
            riverMeshRenderer.sharedMaterial = fallback;
        }
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

        Vector2 uv = GetBiomeUV(tile.Biome);
        for (int i = 0; i < 7; i++)
            uvs.Add(uv);
    }
    
    void AddRivers(Tile tile, int col, int row)
    {
        bool[] rivers = tile.GetRiverEdges();
        if (rivers == null || rivers.Length < 6)
            return;

        Vector3 center = OffsetToWorld(col, row, hexSize);
        float halfWidth = riverWidth * 0.5f;

        for (int edge = 0; edge < 6; edge++)
        {
            if (!rivers[edge])
                continue;

            Vector3 c0 = center + HexCorner(edge, hexSize);
            Vector3 c1 = center + HexCorner((edge + 1) % 6, hexSize);

            Vector3 dir = c1 - c0;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.000001f)
                continue;

            dir.Normalize();
            Vector3 perp = new Vector3(-dir.z, 0f, dir.x);

            Vector3 v0 = c0 + perp * halfWidth; v0.y += riverYOffset;
            Vector3 v1 = c0 - perp * halfWidth; v1.y += riverYOffset;
            Vector3 v2 = c1 - perp * halfWidth; v2.y += riverYOffset;
            Vector3 v3 = c1 + perp * halfWidth; v3.y += riverYOffset;

            int start = riverVertices.Count;
            riverVertices.Add(v0);
            riverVertices.Add(v1);
            riverVertices.Add(v2);
            riverVertices.Add(v3);

            // Front face (one winding)
            riverTriangles.Add(start + 0);
            riverTriangles.Add(start + 1);
            riverTriangles.Add(start + 2);
            riverTriangles.Add(start + 0);
            riverTriangles.Add(start + 2);
            riverTriangles.Add(start + 3);

            // Back face (reverse winding) to bypass culling while debugging
            if (riversDoubleSided)
            {
                riverTriangles.Add(start + 2);
                riverTriangles.Add(start + 1);
                riverTriangles.Add(start + 0);
                riverTriangles.Add(start + 3);
                riverTriangles.Add(start + 2);
                riverTriangles.Add(start + 0);
            }

            riverUvs.Add(new Vector2(0f, 1f));
            riverUvs.Add(new Vector2(0f, 0f));
            riverUvs.Add(new Vector2(1f, 0f));
            riverUvs.Add(new Vector2(1f, 1f));
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

}

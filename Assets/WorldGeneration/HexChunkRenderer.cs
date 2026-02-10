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
    
    [Header("Atlas")]
    public int atlasColumns = 4;
    public int atlasRows = 4;

    public void Init(Tile[] tiles, int mapWidth, int chunkQ, int chunkR)
    {
        this.tiles = tiles;
        this.mapWidth = mapWidth;
        this.chunkQ = chunkQ;
        this.chunkR = chunkR;

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        GetComponent<MeshFilter>().mesh = mesh;

        BuildMesh();
    }

    void BuildMesh()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();

        int baseQ = chunkQ * chunkSize;
        int baseR = chunkR * chunkSize;

        for (int dq = 0; dq < chunkSize; dq++)
        {
            for (int dr = 0; dr < chunkSize; dr++)
            {
                int q = baseQ + dq;
                int r = baseR + dr;

                int index = q + r * mapWidth;
                if (index < 0 || index >= tiles.Length)
                    continue;

                int localQ = dq;
                int localR = dr;
                AddHex(tiles[index], localQ, localR);
            }
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
    }

    void AddHex(Tile tile, int q, int r)
    {
        gameObject.transform.position = HexToWorld(chunkQ*chunkSize, chunkR*chunkSize, hexSize);

        int localQ = q - chunkQ * chunkSize;
        int localR = r - chunkR * chunkSize;
        Vector3 center = HexToWorld(localQ, localR, hexSize);

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

    static Vector3 HexCorner(int i, float size)
    {
        float angle = Mathf.Deg2Rad * (60 * i);
        return new Vector3(
            size * Mathf.Cos(angle),
            0f,
            size * Mathf.Sin(angle)
        );
    }

    static Vector3 HexToWorld(int q, int r, float size)
    {
        float x = size * 1.5f * q;
        float z = size * Mathf.Sqrt(3f) * (r + q * 0.5f);
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

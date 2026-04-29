using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CityBorderMeshRenderer : MonoBehaviour
{
    [SerializeField] private City city;
    [SerializeField] private float borderWidth = 0.08f;
    [SerializeField] private float yOffset = 0.06f;

    private Mesh _mesh;

    // Direction -> hex corner indices (pointy-top, same convention as your world code)
    // 0 NE, 1 E, 2 SE, 3 SW, 4 W, 5 NW
    private static readonly (int a, int b)[] EdgeCorners =
    {
        (0, 1), // NE
        (5, 0), // E
        (4, 5), // SE
        (3, 4), // SW
        (2, 3), // W
        (1, 2), // NW
    };

    public void Rebuild()
    {
        if (city == null) { city = GetComponent<City>(); }
        if (city == null || city.TilesOwned == null || city.TilesOwned.Count == 0) { return; }

        if (_mesh == null)
        {
            _mesh = new Mesh { name = "CityBorderMesh" };
            GetComponent<MeshFilter>().sharedMesh = _mesh;
        }

        var owned = city.TilesOwned; // HashSet<Tile>
        float hexSize = WorldGenerator.Instance.HexSize;

        var vertices = new List<Vector3>(owned.Count * 12);
        var triangles = new List<int>(owned.Count * 18);
        var uvs = new List<Vector2>(owned.Count * 12);

        foreach (Tile tile in owned)
        {
            if (tile == null) { continue; }

            Vector3 centerWorld = Helpers.TileToWorldPos(tile.Position) + Vector3.up * yOffset;

            for (int dir = 0; dir < 6; dir++)
            {
                Tile neighbour = tile.NeighbourTiles[dir];
                if (neighbour != null && owned.Contains(neighbour))
                {
                    // Internal edge (not on perimeter)
                    continue;
                }

                var (ca, cb) = EdgeCorners[dir];
                Vector3 edgeAWorld = centerWorld + HexCorner(ca, hexSize);
                Vector3 edgeBWorld = centerWorld + HexCorner(cb, hexSize);

                Vector3 edgeDir = (edgeBWorld - edgeAWorld).normalized;
                Vector3 normal = new Vector3(-edgeDir.z, 0f, edgeDir.x); // horizontal perpendicular
                Vector3 halfW = normal * (borderWidth * 0.5f);

                // Quad strip for this border segment (world -> local)
                Vector3 v0 = transform.InverseTransformPoint(edgeAWorld - halfW);
                Vector3 v1 = transform.InverseTransformPoint(edgeAWorld + halfW);
                Vector3 v2 = transform.InverseTransformPoint(edgeBWorld + halfW);
                Vector3 v3 = transform.InverseTransformPoint(edgeBWorld - halfW);

                int start = vertices.Count;
                vertices.Add(v0);
                vertices.Add(v1);
                vertices.Add(v2);
                vertices.Add(v3);

                triangles.Add(start + 0);
                triangles.Add(start + 1);
                triangles.Add(start + 2);
                triangles.Add(start + 0);
                triangles.Add(start + 2);
                triangles.Add(start + 3);

                uvs.Add(new Vector2(0f, 0f));
                uvs.Add(new Vector2(0f, 1f));
                uvs.Add(new Vector2(1f, 1f));
                uvs.Add(new Vector2(1f, 0f));
            }
        }

        _mesh.Clear();
        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(triangles, 0);
        _mesh.SetUVs(0, uvs);
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
    }

    private static Vector3 HexCorner(int i, float size)
    {
        float angle = Mathf.Deg2Rad * (60f * i + 30f);
        return new Vector3(size * Mathf.Cos(angle), 0f, size * Mathf.Sin(angle));
    }
}

using UnityEngine;

[System.Serializable]
public class Tile
{
    [SerializeField] private bool hasRiverAdjecent;
    private bool[] hasRiverOnEdge = new bool[6];
    [SerializeField] private Vector2 position;
    [SerializeField] private TileBiome biome;
    public enum TileBiome
    {
        Water = 0,
        Deep_Water = 1,
        Sand = 2,
        Grass = 3,
        Snow = 4,
    };
    
    public Vector2 Position { get { return position; } }
    public TileBiome Biome { get { return biome; } }
    
    public Tile(Vector2 pos, TileBiome bio)
    {
        position = pos;
        biome = bio;
    }
    public bool HasRiverOnEdge(int edgeIndex) { return hasRiverOnEdge[edgeIndex]; }
    public void SetRiverOnEdge(int edgeIndex, bool hasRiver) { hasRiverOnEdge[edgeIndex] = hasRiver; hasRiverAdjecent = hasRiver; }
}

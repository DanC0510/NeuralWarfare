using UnityEngine;

[System.Serializable]
public class Tile
{
    private bool[] hasRiverOnEdge = new bool[6];
    [SerializeField] private Vector2 position;
    [SerializeField] private TileBiome biome;
    public enum TileBiome
    {
        Water = 0,
        Deep_Water = 1,
        Grass = 2,
        Snow = 3,
        Sand = 4,
    };
    
    public Vector2 Position { get { return position; } }
    public TileBiome Biome { get { return biome; } }
    
    public Tile(Vector2 pos, TileBiome bio)
    {
        position = pos;
        biome = bio;
    }
    public bool HasRiverOnEdge(int edgeIndex) { return hasRiverOnEdge[edgeIndex]; }
    public void SetRiverOnEdge(int edgeIndex, bool hasRiver) { hasRiverOnEdge[edgeIndex] = hasRiver; }
}

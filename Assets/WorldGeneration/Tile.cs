using UnityEngine;

[System.Serializable]
public class Tile
{
    [SerializeField] private bool hasRiverAdjecent;
    [SerializeField] private Vector2 position;
    [SerializeField] private TileBiome biome;
    public enum TileBiome
    {
        Water = 0,
        Deep_Water = 1,
        Sand = 2,
        Grass = 3,
        Snow = 4,
        River = 5
    };
    
    public Vector2 Position { get { return position; } }
    public TileBiome Biome { get { return biome; } set { biome = value; } }
    
    public Tile(Vector2 pos, TileBiome bio)
    {
        position = pos;
        biome = bio;
    }
    
    public bool HasRiver() { return hasRiverAdjecent; }
    
}

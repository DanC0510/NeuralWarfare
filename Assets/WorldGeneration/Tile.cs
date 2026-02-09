using UnityEngine;

public class Tile
{
    private int[,,] position;
    private TileBiome biome;
    public enum TileBiome
    {
        Grass,
        Sand,
        Snow,
        Water,
        Deep_Water,
    };
    
    public Tile(int[,,] pos, TileBiome bio)
    {
        position = pos;
        biome = bio;
    }
}

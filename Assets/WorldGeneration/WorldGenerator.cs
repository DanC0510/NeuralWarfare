using System.IO;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldGenerator : MonoBehaviour
{
    [Header("Texture Details")]
    [SerializeField] private float scaleFactor;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int seed;
    [SerializeField] private Vector2 offset;
    [SerializeField] private float scale;
    [SerializeField] private int octaves;
    [SerializeField] private int lacunarity;
    [SerializeField] private float persistence;
    
    [Header("Textures")]
    [SerializeField] private RenderTexture noiseRT;
    [SerializeField] private RenderTexture colorRT;

    [Header("Colour Deatils")]
    [SerializeField] private TerrainType[] regions;

    [Header("Tile Details")] 
    [SerializeField] private Tile[,] tiles;
    
    void Start()
    {
        GenerateTexture(Mathf.FloorToInt(width * scaleFactor), Mathf.FloorToInt(height * scaleFactor));
    }

    
    private void GenerateTexture(int width, int height)
    {
        //Generate the noise map
        Color[] pix = new Color[width * height];
        Texture2D noiseTexture = new Texture2D(width, height);
        if(seed == 0){ seed = Random.Range(int.MinValue, int.MaxValue); }
        if(offset == Vector2.zero) { offset = new Vector2(Random.Range(-100000.0f, 100000.0f), Random.Range(-100000.0f, 100000.0f)); }
        float[,] noiseMap = Noise.CalculateNoiseMap(width, height, seed, scale, octaves, persistence, lacunarity, offset);
        
        // Convert the noise map to a texture
        for (int y = 0; y < noiseTexture.height; y++)
        {
            for (int x = 0; x < noiseTexture.width; x++)
            {
                float currentHeight = noiseMap[x, y];
                pix[y * noiseTexture.width + x] = Color.Lerp(Color.black, Color.white, currentHeight);
            }
        }
        noiseTexture.SetPixels(pix);
        noiseTexture.Apply();
        //Convert the noise map to a colour map + texture
        Texture2D colourTexture = new Texture2D(width, height);
        Color[] colourMap = new Color[width * height];
        tiles = new Tile[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colourMap[y * width + x] = regions[i].color;
                        tiles[x,y] = new Tile( new int[x , y], regions[i].biome);
                        break;
                    }
                }
            }
        }
        colourTexture.SetPixels(colourMap);
        colourTexture.Apply();
        
        SaveWorld();
        Graphics.Blit(noiseTexture, noiseRT);
        Graphics.Blit(colourTexture, colorRT);
    }

    public void SaveWorld()
    {
        string path = Application.persistentDataPath + "/world.dat";
        if(Application.isEditor) { path = "Assets/WorldGeneration/world.dat"; }

        if(!File.Exists(path)) { File.Create(path); }
        var data = new WorldData { tiles = tiles };

        string json = JsonUtility.ToJson(data);
        Debug.Log(json);
        File.WriteAllText(path, json);
    }

    public void LoadWorld()
    {
        string path = Application.persistentDataPath + "/world.dat";
        if(Application.isEditor) { path = "Assets/WorldGeneration/world.dat"; }
        
        StreamReader reader = new StreamReader(path);
        string json = reader.ReadToEnd();
        tiles = JsonUtility.FromJson<Tile[,]>(json);
        reader.Close();
    }
}

public class WorldData
{
    public Tile[,] tiles;
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
    public Tile.TileBiome biome;
}

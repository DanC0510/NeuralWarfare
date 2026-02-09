using System.IO;
using UnityEngine;

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
    [SerializeField] private Tile[] tiles;
    
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
        tiles = new Tile[width * height];
        int tileIndex = 0;
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
                        tiles[tileIndex] = new Tile( new Vector2(x , y), regions[i].biome);
                        tileIndex++;
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
        if (Application.isEditor)
        {
            path = "Assets/WorldGeneration/world.dat";
        }

        if (!File.Exists(path))
        {
            File.Create(path);
        }

        TileArrayWrapper tileArray = new TileArrayWrapper
        {
            tileArr = tiles
        };
        string json = JsonUtility.ToJson(tileArray); 
        File.WriteAllText(path, json);
    }

    public void LoadWorld()
    {
        string path = Application.persistentDataPath + "/world.dat";
        if (Application.isEditor)
        {
            path = "Assets/WorldGeneration/world.dat";
        }
        if(!File.Exists(path)) { Debug.LogError("No world data found at " + path); return; }
        string json = File.ReadAllText(path);
        tiles = JsonUtility.FromJson<Tile[]>(json);
    }
}

[System.Serializable]
public class TileArrayWrapper
{
    public Tile[] tileArr;
}


[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
    public Tile.TileBiome biome;
}

using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [Header("Texture Details")]
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float scale;
    [SerializeField] private int octaves;
    [SerializeField] private int lacunarity;
    [SerializeField] private float persistence;
    private Texture2D noiseTexture;
    private Renderer textureRenderer;
    private Color[] pix;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        textureRenderer = GetComponent<Renderer>();
        noiseTexture = new Texture2D(width, height);
        pix = new Color[noiseTexture.width * noiseTexture.height];
        textureRenderer.material.mainTexture = noiseTexture;
    }

    // Update is called once per frame
    void Update()
    {
        float[,] noiseMap = Noise.CalculateNoiseMap(width, height, scale, octaves, persistence, lacunarity);
        
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
    }
}

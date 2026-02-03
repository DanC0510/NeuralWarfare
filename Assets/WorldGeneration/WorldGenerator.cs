using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [Header("Texture Details")]
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float xOrigin;
    [SerializeField] private float yOrigin;
    [SerializeField] private float scale;
    [SerializeField] private Texture2D noiseTexture;
    [SerializeField] private Color[] pix;
    [SerializeField] private Renderer textureRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        textureRenderer = GetComponent<Renderer>();
        noiseTexture = new Texture2D(width, height);
        pix = new Color[noiseTexture.width * noiseTexture.height];
        textureRenderer.material.mainTexture = noiseTexture;
    }

    void CalculateNoise()
    {
        for (float y = 0.0f; y < noiseTexture.height; y++)
        {
            for (float x = 0.0f; x < noiseTexture.width; x++)
            {
                float xCoord = xOrigin + (x / noiseTexture.width) * scale;
                float yCoord = yOrigin + (y / noiseTexture.height) * scale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                pix[(int)(y * noiseTexture.width + x)] = new Color(sample, sample, sample);
            }
        }
        noiseTexture.SetPixels(pix);
        noiseTexture.Apply();
    }

    // Update is called once per frame
    void Update()
    {
        CalculateNoise();
    }
}

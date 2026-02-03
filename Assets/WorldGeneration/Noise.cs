using UnityEngine;

public static class Noise
{
    public static float[,] CalculateNoiseMap(int width, int height, float scale, int octaves, float persistence, float lacunarity)
    {

        
        float[,] noiseMap = new float[width, height];
        
        if(scale <= 0)
        {
            scale = 0.0001f;
        }
        
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float frequency = 1;
                float amplitude = 1;
                float noiseHeight = 0;
                
                for (int i = 0; i < octaves; i++)
                {
                    float xCoord = x / scale * frequency;
                    float yCoord = y / scale * frequency;
                    
                    float sample = Mathf.PerlinNoise(xCoord, yCoord) * 2 - 1;
                    noiseHeight += sample * amplitude;
                    
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                
                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }
        return noiseMap;
    }
}

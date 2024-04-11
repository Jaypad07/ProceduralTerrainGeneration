using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureBuilder
{
    
    // Builds a texture based on the given noise map
    public static Texture2D BuildTexture(float[,] noiseMap)
    {
        // Create color array for the pixels
        Color[] pixels = new Color[noiseMap.Length];

        // Calculate the length of the texture
        int pixelLength = noiseMap.GetLength(0);

        // Loop through each pixel
        for (int x = 0; x < pixelLength; x++)
        {
            for (int z = 0; z < pixelLength; z++)
            {
                // Next index in the 'pixels' array
                int index = (x * pixelLength) + z;
                
                pixels[index] = Color.Lerp(Color.black, Color.white, noiseMap[x, z]);
            }
        }

        Texture2D texture = new Texture2D(pixelLength, pixelLength);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.SetPixels(pixels);
        texture.Apply();

        return texture;
    }
}

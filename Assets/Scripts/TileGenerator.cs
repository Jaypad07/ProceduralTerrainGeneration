using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TerrainVisualization
{
    Height,
    Heat,
    Moisture,
    Biome
}

public class TileGenerator : MonoBehaviour
{
    [Header("Parameters")]
    public int noiseSampleSize;
    public float scale;
    public float maxHeight = 1.0f;
    public int textureResolution = 1;
    public TerrainVisualization visualizationType;

    [HideInInspector]
    public Vector2 offset;

    [Header("Terrain Types")]
    public TerrainType[] heightTerrainTypes;
    public TerrainType[] heatTerrainTypes;
    public TerrainType[] moistureTerrainTypes;

    [Header("Waves")]
    public Wave[] waves;
    public Wave[] heatWaves;
    public Wave[] moistureWaves;

    [Header("Curves")]
    public AnimationCurve heightCurve;
    
    private MeshRenderer tileMeshRenderer;
    private MeshFilter tileMeshFilter;
    private MeshCollider tileMeshCollider;

    private MeshGenerator _meshGenerator;
    private MapGenerator _mapGenerator;

    private TerrainData[,] datamap;

    private void Start()
    {
        // Get the tile components
        tileMeshRenderer = GetComponent<MeshRenderer>();
        tileMeshFilter = GetComponent<MeshFilter>();
        tileMeshCollider = GetComponent<MeshCollider>();

        _meshGenerator = GetComponent<MeshGenerator>();
        _mapGenerator = FindObjectOfType<MapGenerator>();
        
        GenerateTile();
    }

    void GenerateTile()
    {
        // Generate a new height map
        float[,] heightMap = NoiseGenerator.GenerateNoiseMap(noiseSampleSize, scale, waves, offset);

        // Generate a hd height map to apply as a texture
        float[,] hdHeightMap = NoiseGenerator.GenerateNoiseMap(noiseSampleSize - 1, scale, waves, offset, textureResolution);

        Vector3[] verts = tileMeshFilter.mesh.vertices;

        for (int x = 0; x < noiseSampleSize; x++)
        {
            for (int z = 0; z < noiseSampleSize; z++)
            {
                int index = (x * noiseSampleSize) + z;

                verts[index].y = heightCurve.Evaluate(heightMap[x, z]) * maxHeight;
            }
        }

        tileMeshFilter.mesh.vertices = verts;
        tileMeshFilter.mesh.RecalculateBounds();
        tileMeshFilter.mesh.RecalculateNormals();

        // Update the mesh collider
        tileMeshCollider.sharedMesh = tileMeshFilter.mesh;
        
        // Create the height map texture
        Texture2D heightMapTexture = TextureBuilder.BuildTexture(hdHeightMap, heightTerrainTypes);

        float[,] heatMap = GenerateHeatMap(heightMap);
        float[,] moistureMap = GenerateMoistureMap(heightMap);

        TerrainType[,] heatTerrainTypeMap = TextureBuilder.CreateTerrainTypeMap(heatMap, heatTerrainTypes);
        TerrainType[,] moistureTerrainTypeMap = TextureBuilder.CreateTerrainTypeMap(moistureMap, moistureTerrainTypes);

        switch (visualizationType)
        {
            case TerrainVisualization.Height:
                tileMeshRenderer.material.mainTexture = TextureBuilder.BuildTexture(hdHeightMap, heightTerrainTypes);
                break;
            case TerrainVisualization.Heat:
                tileMeshRenderer.material.mainTexture = TextureBuilder.BuildTexture(heatMap, heatTerrainTypes);
                break;
            case TerrainVisualization.Moisture:
                tileMeshRenderer.material.mainTexture = TextureBuilder.BuildTexture(moistureMap, moistureTerrainTypes);
                break;
            case TerrainVisualization.Biome:
                tileMeshRenderer.material.mainTexture = BiomeBuilder.instance.BuildTexture(heatTerrainTypeMap, moistureTerrainTypeMap);
                break;
        }
        
        CreateDataMap(heatTerrainTypeMap, moistureTerrainTypeMap);
        TreeSpawner.instance.Spawn(datamap);
    }

    void CreateDataMap(TerrainType[,] heatTerrainTypeMap, TerrainType[,] moistureTerrainTypeMap)
    {
        datamap = new TerrainData[noiseSampleSize, noiseSampleSize];
        Vector3[] verts = tileMeshFilter.mesh.vertices;

        for (int x = 0; x < noiseSampleSize; x++)
        {
            for (int z = 0; z < noiseSampleSize; z++)
            {
                TerrainData data = new TerrainData();
                data.position = transform.position + verts[(x * noiseSampleSize) + z];
                data.heatTerrainType = heatTerrainTypeMap[x, z];
                data.moistureTerrainType = moistureTerrainTypeMap[x, z];
                data.biome = BiomeBuilder.instance.GetBiome(data.heatTerrainType, data.moistureTerrainType);

                datamap[x, z] = data;
            }
        }
    }

    // Generates a new heat map
    float[,] GenerateHeatMap(float[,] heightMap)
    {
        float[,] uniformHeatMap = NoiseGenerator.GenerateUniformNoiseMap(noiseSampleSize, transform.position.z * (noiseSampleSize / _meshGenerator.xSize), (noiseSampleSize / 2 * _mapGenerator.numX) + 1);
        float[,] randomHeatMap = NoiseGenerator.GenerateNoiseMap(noiseSampleSize, scale, heatWaves, offset);

        float[,] heatMap = new float[noiseSampleSize, noiseSampleSize];

        for (int x = 0; x < noiseSampleSize; x++)
        {
            for (int z = 0; z < noiseSampleSize; z++)
            {
                heatMap[x, z] = randomHeatMap[x, z] * uniformHeatMap[x, z];
                heatMap[x, z] += 0.5f * heightMap[x, z];

                heatMap[x, z] = Mathf.Clamp(heatMap[x, z], 0.0f, 0.99f);
            }
        }

        return heatMap;
    }

    float[,] GenerateMoistureMap(float[,] heightmap)
    {
        float[,] moistureMap = NoiseGenerator.GenerateNoiseMap(noiseSampleSize, scale, moistureWaves, offset);

        for (int x = 0; x < noiseSampleSize; x++)
        {
            for (int z = 0; z < noiseSampleSize; z++)
            {
                moistureMap[x, z] -= 0.1f * heightmap[x, z];
            }
        }

        return moistureMap;
    }
}

[System.Serializable]
public class TerrainType
{
    public int index;
    [Range(0.0f, 1.0f)]
    public float threshold;
    public Gradient ColorGradient;
}

public class TerrainData
{
    public Vector3 position;
    public TerrainType heatTerrainType;
    public TerrainType moistureTerrainType;
    public Biome biome;
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public TileGenerator tilePrefab;
    public int numX = 2;
    public int numZ = 2;

    private void Start()
    {
        GenerateTiles();
    }

    void GenerateTiles()
    {
        
        // Get the size of the tile.
        float tileSize = tilePrefab.GetComponent<MeshGenerator>().xSize;

        for (int x = 0; x < numX; x++)
        {
            for (int z = 0; z < numZ; z++)
            {
                GameObject tileObj = Instantiate(tilePrefab.gameObject, transform);
                tileObj.transform.position = new Vector3((x - ((float)numX / 2)) * tileSize, 0, (z - ((float)numZ / 2)) * tileSize);

                // Set the tiles offset
                float offsetRate = (tilePrefab.noiseSampleSize - 1) / tilePrefab.scale;
                tileObj.GetComponent<TileGenerator>().offset = new Vector2(x * offsetRate, z * offsetRate);
            }
        }
    }
}

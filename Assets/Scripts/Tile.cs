using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject HexTileDirt;
    private int width = 10;
    private int height = 25;
    //private float x_offset = 0.73f;
    void Start()
    {
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject obj = Instantiate(HexTileDirt);
            }
        }
    }
}
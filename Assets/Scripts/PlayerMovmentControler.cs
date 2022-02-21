using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovmentControler : MonoBehaviour
{
    private TileManager manager;
    private float x_offset = 1.5f;
    private float y_offset = 0.435f;
    
    void Start()
    {
        Debug.Log(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int gridPos = manager.map.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            //manager.ShowTilesFromList (manager.GetNeigbours (new Vector3Int (gridPos.x,gridPos.y, 0)));
            //manager.ShowRing (gridPos, 2);
            // if (manager.isWalkable(Camera.main.ScreenToWorldPoint(Input.mousePosition)))
            // {
            //     transform.position = manager.getPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            //     Debug.Log(transform.position);
            // }
        }
        
    }

    void Awake()
    {
        manager = FindObjectOfType<TileManager>();
    }
}

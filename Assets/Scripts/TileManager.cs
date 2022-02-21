using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Linq;
using Random = UnityEngine.Random;

public class TileManager : MonoBehaviour
{
    public Tilemap map;
    private int width = 10;
    private int height = 25;
    private float x_offset = 1.5f;
    private float y_offset = 0.435f;
    
    [SerializeField] private List<TileBase> _tiles;
    [SerializeField] private List<TileData> tileDatas;
    private Dictionary<TileBase, TileData> dataFromTiles;

    private int[,,] _neighbours = {{{1, 0}, {0, 1}, {-1, 1}, {-1, 0}, {-1, -1}, {0, -1}}, {{1, 0}, {1, 1}, {0, 1}, {-1, 0}, {0, -1}, {1, -1}}};
    private List<Vector3Int> _cubeDirections = new List<Vector3Int> () {
    new Vector3Int (0,-1,1), new Vector3Int (1, -1, 0), new Vector3Int (1, 0, -1),
    new Vector3Int(0,1,-1),new Vector3Int (-1,1,0), new Vector3Int (-1,0,1)};

    void Start() {
        int[] dist = {25, 50}; // up to 50
        GenerateMap(5, dist); // up to 10
        //GenerateRoom (200,dir);
        //GenerateRoomRandomWalk (new Vector3Int (0,0,0), 100,20, true);
    }

    void OnDestroy() {
        map.ClearAllTiles ();
    }
    
    private void Awake()
    {
        dataFromTiles = new Dictionary<TileBase, TileData>();
        foreach (var tileData in tileDatas)
        {
            foreach (var tile in tileData.tiles)
            {
                dataFromTiles.Add(tile, tileData);
            }
        }
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("clicked");
            Vector2 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPos = map.WorldToCell(mPos);
            TileBase tile = map.GetTile(gridPos);
            bool walkable = dataFromTiles[tile].walkable;
            Debug.Log("Clicked at: " + gridPos + "; walkable: " + walkable);
        }
    }

    void GenerateMap(int numOfRooms, int[] distanceBetweenRooms) {
        List<Vector3Int> roomSeeds = new List<Vector3Int> ();
        roomSeeds.Add (new Vector3Int (0, 0, 0));
        for (int i = 1; i < numOfRooms; i ++)
        {
            int lim = roomSeeds.Count;
            int rad = UnityEngine.Random.Range (distanceBetweenRooms[0], distanceBetweenRooms[1]);
            // var ring = GenerateRing (roomSeeds[UnityEngine.Random.Range (0,roomSeeds.Count)], rad);
            // int minDist; 
            // Vector3Int newSeed;
            // newSeed = ring[UnityEngine.Random.Range (0,ring.Count)];
            // do
            // {
            //     minDist = int.MaxValue;
            //     newSeed = ring[UnityEngine.Random.Range (0,ring.Count)];
            //     foreach (var tile in roomSeeds)
            //     {
            //         minDist = Math.Min (minDist, getDistance (tile, newSeed));
            //     }
            // } while (minDist < distanceBetweenRooms[0]);
            var newSeed = GetRandomTileFromRing (roomSeeds[UnityEngine.Random.Range (0, roomSeeds.Count)], rad);
            roomSeeds.Add (newSeed);
            //ring.Clear ();
        }

        foreach (var tile in roomSeeds)
        {
            map.SetTile (tile, _tiles[2]);
            int[] dir = {0,0,0,0,0,0};
            for (int i = 0; i < 6; i++)
            {
                dir[i] = UnityEngine.Random.Range (10, 100);
            }
            GenerateRoom (UnityEngine.Random.Range (50,150), dir, tile);//up to 200
        }
        //GenerateBoundries ();
    }
    

    HashSet <Vector3Int> RandomWalk(Vector3Int start, int len) {
        HashSet<Vector3Int> path = new HashSet<Vector3Int>();
        var prevPos = start;
        path.Add (start);
        for (int i = 0; i < len; i ++)
        {
            var newPos = GetGivenNeighbour (prevPos, Random.Range (0, 6));
            path.Add (newPos);
            prevPos = newPos;
        }
        return path;
    }

    void GenerateRoomRandomWalk(Vector3Int start, int iteration, int length, bool startRandomly) {
        HashSet<Vector3Int> floor = new HashSet<Vector3Int>();
        var currentPos = start;
        for (int i = 0; i < iteration; i ++)
        {
            floor.UnionWith (RandomWalk(currentPos, length));
            if (startRandomly)
            {
                currentPos = floor.ElementAt (Random.Range (0, floor.Count));
            }
        }

        foreach (var pos in floor)
        {
            map.SetTile (pos, _tiles[0]);
        }
            
    }
    void GenerateRoom(int size, int[] dir, Vector3Int seed) {
        List<Vector3Int> tileList = new List<Vector3Int> ();
        //map.SetTile (new Vector3Int (0, 0, 0), _tiles[0]);
        tileList.Add (seed);
        int actId = 0;
        while (tileList.Count < size)
        {
            int ct = tileList.Count;
            for (int i = actId; i < ct; i ++)
            {
                //List<Vector3Int> nei = GetNeigbours (tileList[i]);
                for (int j = 0; j < 6; j ++)
                {
                    //TileBase tile = map.GetTile (nei[j]);
                    var pos = GetGivenNeighbour (tileList[i], j);
                    TileBase tile = map.GetTile (pos);
                    if (tile == null)
                    {
                        int a = UnityEngine.Random.Range (0, 100);
                        if (a < dir[j])
                        {
                            int proc = UnityEngine.Random.Range (0, 100);
                            int choice = 0;
                            if (proc < 10)
                            {
                                choice = 1;
                            }
                            map.SetTile (new Vector3Int (pos.x, pos.y, 0), _tiles[choice]);
                            tileList.Add (new Vector3Int (pos.x, pos.y, 0));
                        }
                    }
                }
            }

            actId = ct;
        }
        tileList.Clear ();
    }

    private void GenerateBoundries() {
        List<Vector3Int> boundries = new List<Vector3Int> ();
        foreach (var position in map.cellBounds.allPositionsWithin)
        {
            List<Vector3Int> nei = GetNeigbours (position);
            foreach (Vector3Int entry in nei)
            {
                TileBase tile = map.GetTile (entry);
                if (tile == null)
                {
                    boundries.Add (entry);
                }
            }
        }

        foreach (Vector3Int entry in boundries)
        {
            map.SetTile (entry, _tiles[1]);
        }
        boundries.Clear ();
    }

    void setInfo(TileBase obj, int x, int y)
    {
        //obj.transform.parent = transform;
        obj.name = x.ToString() + " " + y.ToString();
    }

   

    public bool isWalkable(Vector2 pos)
    {
        Vector3Int gridPos = map.WorldToCell(pos);
        return isWalkable (gridPos);
    }

    public bool isWalkable(Vector3Int gridPos) {
        TileBase tile = map.GetTile(gridPos);
        if (tile == null)
        {
            return false;
        }
        else
        {
            return dataFromTiles[tile].walkable;
        }
    }
    
    public Vector3 ToCube(Vector3Int gridPos)
    {
        float q = gridPos.y;
        float r = gridPos.x - (gridPos.y - (Mathf.Abs(gridPos.y) %2))/2f;
        return new Vector3(q, r, -q-r);
    }

    Vector3Int ToOffset(Vector3 cube) {
        var row = (int) cube.x;
        var col = (int) (cube.y + (cube.x - (Mathf.Abs(cube.x) %2))/2f);
        return new Vector3Int (col, row, 0);
    }

    public Vector3 ToPix(Vector3Int gridPos)
    {
        float size = x_offset/3f;
        float x = size*3/2*gridPos.y;
        float y = size * Mathf.Sqrt(3) * (gridPos.x + 0.5f * (Mathf.Abs(gridPos.y) %2));
        return new Vector3(x, y, 0);
    }
    public Vector3 getPosition(Vector2 pos)
    {
        Vector3Int gridPos = map.WorldToCell(pos);
        TileBase tile = map.GetTile(gridPos);
        if (tile == null)
        {
            return new Vector3(0,0,0);
        }
        else
        {
            return ToPix(gridPos);
        }
    }
    
    public Vector3Int getPositionGrid(Vector3 pos)
    {
        Vector2 p = new Vector2(pos.x, pos.y);
        return map.WorldToCell(pos);
    }

    Vector3 subtractCube(Vector3 A, Vector3 B)
    {
        return new Vector3(A.x - B.x, A.y - B.y, A.z - B.z);
    }

    Vector3 CubeScale(Vector3 A, float factor) {
        return new Vector3 (A.x * factor, A.y * factor, A.z * factor);
    }

    Vector3 CubeAdd(Vector3 A, Vector3 B) {
        return new Vector3(A.x + B.x, A.y + B.y, A.z + B.z);
    }


    Vector3Int CubeDirection(int direction) {
        return _cubeDirections[direction];
    }
    
    

    List<Vector3Int> GenerateRing(Vector3Int center, int radius) {
        var res = new List <Vector3Int> ();
        var tile = ToOffset(CubeAdd (ToCube (center), CubeScale (CubeDirection (5), radius)));
        for (int i = 0; i < 6; i ++)
        {
            for (int j = 0; j < radius; j ++)
            {
                res.Add (tile);
                var nei = GetGivenNeighbour (tile, i);
                tile = nei;
            }
        }
        return res;
    }

    Vector3Int GetRandomTileFromRing(Vector3Int center, int radius) {
        int steps = 6 * radius;
        int choice = Random.Range (0, steps);
        var tile = ToOffset(CubeAdd (ToCube (center), CubeScale (CubeDirection (5), radius)));
        for (int i = 0; i < 6; i ++)
        {
            for (int j = 0; j < radius; j ++)
            {
                var nei = GetGivenNeighbour (tile, i);
                tile = nei;
                choice --;
                if (choice == 0)
                {
                    return tile;
                }
            }
        }

        return center;
    }

    public void ShowTilesFromList(List<Vector3Int> list) {
        foreach (var tile in list)
        {
            map.SetTile (tile, _tiles[2]);
        }
    }

    public void ShowRing(Vector3Int center, int radius) {
        var ring = GenerateRing (center, radius);
        foreach (var tile in ring)
        {
            map.SetTile (tile, _tiles[2]);
        }
    }
    
    public int getDistance(Vector3Int A, Vector3Int B)
    {
        Vector3 diff = subtractCube(ToCube(A), ToCube(B));
        return (int) Mathf.Max(Mathf.Abs(diff.x), Mathf.Max(Mathf.Abs(diff.y), Mathf.Abs(diff.z)));
    }

    public List<Vector3Int> GetNeigbours(Vector3Int pos) {
        TileBase tile = map.GetTile(pos);
        List<Vector3Int> res = new List<Vector3Int> (); 
        if (tile != null)
        {
            int parity = Math.Abs(pos.y) % 2;
            for (int i = 0; i < 6; i++)
            {
                res.Add (new Vector3Int (pos.x + _neighbours[parity, i, 0], pos.y + _neighbours[parity, i, 1], 0));
            }
        }

        return res;
    }

    public List<Vector3Int> GetAllNeighbours(Vector3Int pos) {
        List<Vector3Int> res = new List<Vector3Int> (); 
        int parity = Math.Abs(pos.y) % 2;
        for (int i = 0; i < 6; i++)
        {
            res.Add (new Vector3Int (pos.x + _neighbours[parity, i, 0], pos.y + _neighbours[parity, i, 1], 0));
        }

        return res;
    }

    Vector3Int GetGivenNeighbour(Vector3Int pos, int num) {
        int parity = Math.Abs(pos.y) % 2;
        return new Vector3Int (pos.x + _neighbours[parity, num, 0], pos.y + _neighbours[parity, num, 1], 0);
    }
    

    public List<Vector3Int> CalculateRoute(Vector3Int startPoint, Vector3Int goal) {
        float distance = getDistance (startPoint, goal);
        var toSearch = new List < Tuple <Vector3Int, List <float> > >();
        toSearch.Add (new Tuple<Vector3Int, List<float>> (startPoint, new List<float> {0f, distance, distance})); // g,h,f
		var porcessed = new List <Vector3Int> ();
        var connections = new Dictionary<Vector3Int, Vector3Int> ();
        
        while (toSearch.Count > 0)
        {
            var current = toSearch[0];
            int size = toSearch.Count;
            for (int i = 0; i < size; i++)
            {
                var vertex = toSearch[0];
                if (vertex.Item2[2] < current.Item2[2] || vertex.Item2[2] == current.Item2[2] && vertex.Item2[1] < current.Item2[1])
                {
                    current = vertex;
                }

                porcessed.Add (current.Item1);
                toSearch.Remove (current);

                if (current.Item1 == goal)
                {
                    var currentPathTile = current.Item1;
                    var path = new List<Vector3Int> ();
                    while (currentPathTile != startPoint)
                    {
                        path.Add (currentPathTile);
                        currentPathTile = connections[currentPathTile];
                    }
                    return path;
                }

                var nei = GetNeigbours (current.Item1);
                foreach (var n in nei)
                {
                    if (isWalkable (n) && !porcessed.Contains (n))
                    {
                        bool isIn = false;
                        foreach (var v in toSearch)
                        {
                            isIn = isIn || (v.Item1 == n);
                        }

                        var data = new Tuple<Vector3Int, List<float>> (n, new List<float>() );
                        data.Item2.Add (current.Item2[0]+1);
                        data.Item2.Add (getDistance (n, goal));
                        data.Item2.Add (data.Item2[0]+data.Item2[1]);
                        
                        float costToN = current.Item2[0] + getDistance (current.Item1, n);
                        if (!isIn || costToN < data.Item2[0])
                        {
                            data.Item2[0] = costToN;
                            if (connections.ContainsKey (n))
                            {
                                connections[n] = current.Item1;
                            }
                            else
                            {
                                connections.Add (n, current.Item1);
                            }
                            if (!isIn)
                            {
                                toSearch.Add (data);
                            }
                        }
                        
                    }
                }
            }
        }

        return null;
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellularAutomata : MonoBehaviour
{   
    [Header("Default settings")]
    [SerializeField]
    private int fps;
    [SerializeField, Range(0.1f, 2f)]
    private float simulationSpeed;

    [Header("Cell settings")]
    [SerializeField]
    private GameObject cellPrefab;
    [SerializeField]
    private float cellSize;

    //TODO possibly add more mats
    [Header("Cell materials")]
    [SerializeField]
    private Material materialFlamable;  //green-brown
    [SerializeField]
    private Material materialInflamable;    //grey/white
    [SerializeField]
    private Material materialBurningSlightly;   //orange
    [SerializeField]
    private Material materialBurningNormal; //orange-red
    [SerializeField]
    private Material materialBurningExtremely;  //deep red


    [Header("Map settings")]
    [SerializeField]
    private int width;
    [SerializeField]
    private int height;
    [SerializeField]
    private bool randomlyGenerated;
    [SerializeField, Range(0, 100)]
    private float fillPercent;
    [Tooltip("For no seed enter 'noSeed' or nothing")]
    [SerializeField]
    private string seedString;

    private int[,] map;
    private Cell[,] allCells;
    private int seedInt;
    private const int defaultMapValue = 0;
    private float timer;

    private void Awake()
    {
        map = GenerateMap(width, height, randomlyGenerated, fillPercent);
    }
    // Start is called before the first frame update
    void Start()
    {
        allCells = SpawnMap(map, height, width, cellSize);
    }

    // Update is called once per frame
    void Update()
    {
        map = OneStepInGame(map);
    }

    private void FixedUpdate()
    {
        //Start / stop simulation
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (timer <= 0)
            {
                map = OneStepInGame(map);
                timer += (1 / (float)fps) * simulationSpeed;
            }
        }
        if(timer > 0)
        {
            timer -= Time.fixedDeltaTime;
        }
    }

    private int GenerateSeedStringToInt(string _seedString)
    {
        if (string.IsNullOrEmpty(_seedString) || _seedString == "noSeed" || string.IsNullOrWhiteSpace(_seedString))
        {
            return (int)System.DateTime.Now.Ticks;
        } else return _seedString.GetHashCode();
        
    }

    private int[,] GenerateMap(int _width, int _height, bool _randomlyGenerated, float _fillPercent)
    {
        seedInt = GenerateSeedStringToInt(seedString);
        System.Random rdm = new System.Random(seedInt);
        int[,] tempMap = new int[_width, _height];

        for (int i = 0; i < _height; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                //randomly set cells to 1 or 0 (active/inactive)
                if (_randomlyGenerated)
                {
                    tempMap[j, i] = (float)rdm.NextDouble() * 100f <= fillPercent ? 1 : 0;
                }
                //sets cells to default value only for debugging purposes
                else tempMap[j, i] = defaultMapValue;
            }
        }
        return tempMap;
    }

    private Cell[,] SpawnMap(int [,] map, int _mapHeight, int _mapWidth, float _size)
    {
        Cell[,] spawnedCells = new Cell[_mapWidth, _mapHeight];
        for (int i = 0; i < _mapHeight; i++)
        {
            for (int j = 0; j < _mapWidth; j++)
            {
                GameObject cellGO = Instantiate(cellPrefab);
                cellGO.name = $"Cell[{j}|{i}]";
                cellGO.transform.localPosition = new Vector3(j, 0, i) * _size;
                spawnedCells[j, i] = cellGO.GetComponent<Cell>();
                spawnedCells[j, i].AssignCell(this, materialInflamable, materialFlamable, materialBurningSlightly, materialBurningNormal, materialBurningExtremely, j, i, OnCellChanged, map[j, i] == 1);
            }
        }
        return spawnedCells;
    }

    private int[,] OneStepInGame(int[,] _map)
    {
        int newWidth = _map.GetLength(0);
        int newHeight = _map.GetLength(1);
        int[,] newMap = new int[newWidth, newHeight];

        for (int i = 0; i < newHeight; i++)
        {
            for (int j = 0; j < newWidth; j++)
            {
                //get all active neighbours | max 8 active neighbours
                int activeNeighbours = GetActiveNeighbours(newMap, newHeight, newWidth, j, i);

                //Rules here
                //Cell active
                if (_map[j, i] == 1)
                {
                    //Rule 1: if less than 3 neighbours -> inactive
                    if (activeNeighbours <= 2)
                    {
                        newMap[j, i] = 0;
                    }
                    //Rule 2: if more than 8 neighbours -> stay active
                    else if (activeNeighbours >= 9)
                    {
                        newMap[j, i] = 1;
                    }
                    //Rule 3: if >2 and <9 -> randomly set status
                    else
                    {
                        newMap[j,i] = RandomlyReturn1or0();
                    }
                //Cell inactive
                }else if (_map[j,i] == 0)
                {
                    //Rule 4: if less than 7 neighbours -> stay inactive
                    if (activeNeighbours <=6)
                    {
                        newMap[j, i] = 0;
                    }
                    //Rule 5: if more than 8 neighbours -> active
                    else if (activeNeighbours >= 9)
                    {
                        newMap[j, i] = 1;
                    }
                    //Rule 6: if >6 and <9 -> randomly set status
                    else
                    {
                        newMap[j, i] = RandomlyReturn1or0();
                    }
                }else
                //shouldn't be reached anyways
                {

                }

            }
        }
        UpdateMap(newMap, newHeight, newWidth);
        return newMap;
    }

    private int GetActiveNeighbours(int[,] _map, int _newMapHeight, int _newMapWidth, int _posOnMapX, int _posOnMapY)
    {
        int neighbourCount = 0;

        for (int i = _posOnMapY - 1; i <= _posOnMapY + 1; i++)
        {
            for (int j = _posOnMapX - 1; j <= _posOnMapX + 1; j++)
            {
                //origin -> not a neighbour
                if(i == _posOnMapY && j == _posOnMapX)
                {
                    continue;
                }
                // check if on map
                if(i >= 0 && i < _newMapHeight && j >= 0 && j < _newMapWidth)
                {
                    //alive
                    if (_map[j,i] == 1)
                    {
                        neighbourCount++;
                    }
                }
            }
        }
        return neighbourCount;
    }

    private void UpdateMap(int[,] _map, int _mapHeight, int _mapWidth)
    {
        for (int i = 0; i < _mapHeight; i++)
        {
            for (int j = 0; j < _mapWidth; j++)
            {
                //if 1 -> cell active
                allCells[j, i].UpdateCellStatus(_map[j, i] == 1);
            }
        }
    }

    private void OnCellChanged(Vector2Int _position, bool _active)
    {
        map[_position.x, _position.y] = _active ? 1 : 0;
        UpdateMap(map, map.GetLength(0), map.GetLength(1));
    }

    private int RandomlyReturn1or0()
    {
        seedInt = GenerateSeedStringToInt(seedString);
        System.Random rdm = new System.Random(seedInt);
        int rdmInt = rdm.Next(0, 1);
        return rdmInt;
    }
}

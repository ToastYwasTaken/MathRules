using System.Collections.Generic;
using TMPro;
using UnityEngine;
/******************************************************************************
 * Project: MathRules
 * File: CellularAutomata.cs
 * Version: 1.01
 * Autor:  Franz Mörike (FM);
 * 
 * 
 * These coded instructions, statements, and computer programs contain
 * proprietary information of the author and are protected by Federal
 * copyright law. They may not be disclosed to third parties or copied
 * or duplicated in any form, in whole or in part, without the prior
 * written consent of the author.
 * 
 * NOTE:
 * 
 * The basic coding structure and algorithms are from Unterricht_GenerischeWelten!
 * That code-base was used, improved and adjusted to my usage.
 * 
 * The States are internally defined as [0] inflamable [1] flamable [2] burning
 * 
 * ChangeLog
 * ----------------------------
 *  11.11.2021  created
 *  13.11.2021  added comments
 *  14.11.2021  added public enum, additional code, added Text-functionality
 *  
 *****************************************************************************/
public enum EInternalStates
{
    E_INVALID = -1,
    E_INFLAMABLE = 0,
    E_FLAMABLE = 1,
    E_BURNING = 2,
    E_BURNT = 3
}
public class CellularAutomata : MonoBehaviour
{
    [Header("Default settings")]
    [SerializeField]
    private int fps;
    [SerializeField, Range(0.1f, 2f)]
    private float simulationSpeed;
    [SerializeField]
    private bool displayStates;
    [SerializeField]
    private int fontSize;
    [SerializeField]
    private Color textColor;
    [Tooltip("zoom in / zoom out | Range: 0f - 50f")]
    [SerializeField, Range(0f, 50f)]
    private float cameraZoom;

    [Header("Cell settings")]
    [SerializeField]
    private GameObject cellPrefab;
    [SerializeField]
    private float cellSize;
    [SerializeField]
    private GameObject textPrefab;

    //TODO possibly add more mats
    [Header("Cell materials")]
    [SerializeField]
    private Material materialFlamable;  //green-brown
    [SerializeField]
    private Material materialInflamable;    //grey/white
    [SerializeField]
    private Material materialBurning;
    [SerializeField]
    private Material materialBurnt;
    [SerializeField]
    private Material materialInvalid;

    [Header("Map settings")]
    [SerializeField]
    private int width = 64; //default value
    [SerializeField]
    private int height = 40; //default value
    [Tooltip("Don't use perlin and randomly at the same time")]
    [SerializeField]
    private bool randomlyGenerated;
    [SerializeField]
    [Tooltip("Don't use perlin and randomly at the same time")]
    private bool usePerlinNoise;
    [SerializeField]
    [Range(0.1f, 1f)]
    [Tooltip("1 means no offset")]
    private float perlinOffset;    
    [SerializeField, Range(0, 100)]
    private float fillPercent;
    [Tooltip("For no seed enter 'noSeed' or nothing")]
    [SerializeField]
    private string seedString;
    [Tooltip("For no seed enter 'noSeed' or nothing")]
    [SerializeField]
    private string seedStringForCells;
    [SerializeField]
    private int switchToBurningAfterXsteps = 10;
    [SerializeField]
    private int switchToBurntAfterXsteps = 10;

    private int[,] map;
    private Cell[,] allCells;
    private int seedInt;
    private const int defaultMapValue = 0;
    private float timer;
    private Camera cameraRef;
    private const float defaultCamDistance = 25f;
    private float currentCamDistance;
    private List<GameObject> allTextGOs = new List<GameObject>();
    private int stepsPassedToIgnite;
    private int stepsPassedToBurnOut;

    private int inflamableNeighbours;    
    private int flamableNeighbours;
    private int burningNeighbours;
    private int burntNeighbours;


    private void Awake()
    {
        cameraRef = FindObjectOfType<Camera>();
        cameraRef.transform.position = new Vector3(0f, defaultCamDistance, 0f);
        currentCamDistance = defaultCamDistance;
        map = GenerateMap(width, height, randomlyGenerated, usePerlinNoise, fillPercent);
    }
    // Start is called before the first frame update
    void Start()
    {
        allCells = SpawnMap(map, width, height, cellSize);
    }

    // Update is called once per frame
    void Update()
    {
        //Only step into camera zoom when needed
        if (cameraZoom != currentCamDistance)
        {
            CameraZoom();
            currentCamDistance = cameraZoom;
        }
    }

    private void FixedUpdate()
    {
        //Start / stop simulation
        if (Input.GetKey(KeyCode.Space))
        {
            if (timer <= 0)
            {
                map = ApplyRules(map);
                timer += (1 / (float)fps) * simulationSpeed;
            }
        }
        if (timer > 0)
        {
            timer -= Time.fixedDeltaTime;
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            map = ApplyRules(map);
        }
    }

    private int GenerateSeedStringToInt(string _seedString)
    {
        if (string.IsNullOrEmpty(_seedString) || _seedString == "noSeed" || string.IsNullOrWhiteSpace(_seedString))
        {
            return (int)System.DateTime.Now.Ticks;
        }
        else return _seedString.GetHashCode();

    }

    /// <summary>
    /// Generate the map before spawning it.
    /// </summary>
    /// <param name="_width">desired width of the map</param>
    /// <param name="_height">desired height of the map</param>
    /// <param name="_randomlyGenerated">should the map be randomly generated?</param>
    /// <param name="_fillPercent">how much percent of the map should be 'active'</param>
    /// <returns>new map</returns>
    private int[,] GenerateMap(int _width, int _height, bool _randomlyGenerated, bool _usePerlinNoise, float _fillPercent)
    {
        seedInt = GenerateSeedStringToInt(seedString);
        System.Random rdm = new System.Random(seedInt);
        int[,] tempMap = new int[_width, _height];

        for (int i = 0; i < _height; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                //randomly set cells to 1 or 0 (active/inactive in this case flamable / inflamable)
                if (_randomlyGenerated)
                {
                    usePerlinNoise = false;
                    tempMap[j, i] = (float)rdm.NextDouble() * 100f <= fillPercent ? (int)EInternalStates.E_FLAMABLE : (int)EInternalStates.E_INFLAMABLE;
                }else if (_usePerlinNoise)
                {
                    randomlyGenerated = false;
                    float currentPerlinNoise = Mathf.PerlinNoise(perlinOffset * j, perlinOffset * i);
                    //set cells to inflamable
                    if (currentPerlinNoise > 0.5f)
                    {
                        tempMap[j, i] = (int)EInternalStates.E_INFLAMABLE;
                    }
                    //set cells to flamable
                    else tempMap[j, i] = (int)EInternalStates.E_FLAMABLE;

                }
                //sets cells to default value only for debugging purposes
                else tempMap[j, i] = defaultMapValue;
            }
        }
        return tempMap;
    }

    /// <summary>
    /// Spawning the game map consisting of cubes
    /// </summary>
    /// <param name="map">the map generated previously in GenerateMap()</param>
    /// <param name="_mapHeight">the map's desired height</param>
    /// <param name="_mapWidth">the map's desired width</param>
    /// <param name="_size">the tile size</param>
    /// <returns></returns>
    private Cell[,] SpawnMap(int[,] map, int _mapWidth, int _mapHeight, float _size)
    {
        Cell[,] spawnedCells = new Cell[_mapWidth, _mapHeight];
        //Pass a seed to cell
        int seedIntForCells = GenerateSeedStringToInt(seedStringForCells);
        //make map centered at (0,0,0)
        Vector3 spawnOffset = new Vector3(_mapWidth, 0, _mapHeight) * -0.5f;
        //offset text above cells
        Vector3 textOffset = new Vector3(0, 0.7f, 0);
        //map spawning
        if (displayStates)
        {
            for (int i = 0; i < _mapHeight; i++)
            {
                for (int j = 0; j < _mapWidth; j++)
                {
                    //Spawn cell
                    GameObject cellGO = Instantiate(cellPrefab);
                    cellGO.name = $"Cell[{j}|{i}]";
                    cellGO.transform.localPosition = spawnOffset + new Vector3(j, 0, i) * _size;
                    //text preparations
                    GameObject textGO = Instantiate(textPrefab);
                    TextMeshPro textMesh = textPrefab.transform.GetComponent<TextMeshPro>();
                    textGO.name = cellGO.name + " text";
                    textMesh.fontSize = (int)fontSize;
                    textMesh.color = textColor;
                    textGO.transform.localPosition = textOffset + cellGO.transform.localPosition;
                    textGO.transform.Rotate(90f, 0, 0, Space.Self);
                    //Assign cells
                    spawnedCells[j, i] = cellGO.GetComponent<Cell>();
                    spawnedCells[j, i].AssignCell(this, materialInvalid, materialInflamable, materialFlamable, materialBurning, materialBurnt, seedIntForCells, j, i, OnCellChanged, map[j, i] == (int)EInternalStates.E_INFLAMABLE, map[j, i] == (int)EInternalStates.E_FLAMABLE, map[j, i] == (int)EInternalStates.E_BURNING, map[j, i] == (int)EInternalStates.E_BURNT);
                    //Assign / spawn text
                    Debug.Log($"Cell[{j}|{i}] : {spawnedCells[j, i].state}");
                    textMesh.text = ((int)(spawnedCells[j, i].state)).ToString();
                    //save text GOs
                    allTextGOs.Add(textGO);
                }
            }
        }
        else
        {
            for (int i = 0; i < _mapHeight; i++)
            {
                for (int j = 0; j < _mapWidth; j++)
                {
                    //Spawn cell
                    GameObject cellGO = Instantiate(cellPrefab);
                    cellGO.name = $"Cell[{j}|{i}]";
                    cellGO.transform.localPosition = spawnOffset + new Vector3(j, 0, i) * _size;

                    //Assign cells
                    spawnedCells[j, i] = cellGO.GetComponent<Cell>();
                    spawnedCells[j, i].AssignCell(this, materialInvalid, materialInflamable, materialFlamable, materialBurning, materialBurnt, seedIntForCells, j, i, OnCellChanged, map[j, i] == (int)EInternalStates.E_INFLAMABLE, map[j, i] == (int)EInternalStates.E_FLAMABLE, map[j, i] == (int)EInternalStates.E_BURNING, map[j, i] == (int)EInternalStates.E_BURNT);
                }
            }
        }
        return spawnedCells;
    }

    /// <summary>
    /// Applies the rules for the algorithm
    /// </summary>
    /// <param name="_map">current map</param>
    /// <returns>updated map</returns>
    private int[,] ApplyRules(int[,] _map)
    {
        int newWidth = _map.GetLength(0);
        int newHeight = _map.GetLength(1);
        int[,] newMap = new int[newWidth, newHeight];

        for (int i = 0; i < newHeight; i++)
        {
            for (int j = 0; j < newWidth; j++)
            {
                //Get burning neighbours 
                burningNeighbours = ReturnAllBurningNeighbours(_map, j, i);
                burntNeighbours = ReturnAllBurntNeighbours(_map, j, i);
                Debug.Log($"steps passed to ignite: {stepsPassedToIgnite} steps passed to burn out: {stepsPassedToBurnOut}");
                //Rule 1: if any neighbour is burning & this is flamable -> set this on fire after x steps
                if (burningNeighbours >= 1 && _map[j, i] == (int)EInternalStates.E_FLAMABLE)
                {
                    //Switch cell to burning
                    if (stepsPassedToIgnite > switchToBurningAfterXsteps)
                    {
                        newMap[j, i] = (int)EInternalStates.E_BURNING;
                        stepsPassedToIgnite = 0;
                    }
                    //Keep previous state
                    else
                    {
                        newMap[j, i] = (int)EInternalStates.E_FLAMABLE;
                        stepsPassedToIgnite++;
                    }
                }
                //Rule 2: if more than 3 neighbours burning & this is flamable -> set this on fire
                else if (burningNeighbours > 3 && _map[j, i] == (int)EInternalStates.E_FLAMABLE)
                {
                    newMap[j, i] = (int)EInternalStates.E_BURNING;
                }
                //Rule3: if more than 2 neighbours are burning & this is burning -> switch to burnt after x steps
                else if (burningNeighbours >=3 && _map[j, i] == (int)EInternalStates.E_BURNING)
                {
                    if (stepsPassedToBurnOut > switchToBurntAfterXsteps)
                    {
                        newMap[j, i] = (int)EInternalStates.E_BURNT;
                        stepsPassedToBurnOut = 0;
                    }
                    //Keep previous state
                    else
                    {
                        newMap[j, i] = (int)EInternalStates.E_BURNING;
                        stepsPassedToBurnOut++;
                    }
                }
                 //Rule4: if more than 1 neighbour are already burnt & this is burning -> switch to burnt
                else if (burntNeighbours >= 2 && _map[j, i] == (int)EInternalStates.E_BURNING)
                {
                    newMap[j, i] = (int)EInternalStates.E_BURNT;
                }
                else 
                //Keep previous state
                    newMap[j, i] = _map[j, i];
                #region oldrules
                //    GetAllCurrentNeighbours(_map, j, i);
                //    if (flamableNeighbours >= 5)
                //    {
                //        if (_map[j, i] == (int)EInternalStates.E_BURNING)
                //        {
                //            Debug.Log("Cell burning");
                //            //switch to burnt after burning for x steps
                //            if (stepsPassed >= switchStatusAfterSteps)
                //            {
                //                Debug.Log("Setting cell burnt");
                //                newMap[j, i] = (int)EInternalStates.E_BURNT;
                //            }
                //            else
                //            {
                //                Debug.Log("Cell stays burning");
                //                newMap[j, i] = (int)EInternalStates.E_BURNING;
                //            }
                //        }
                //        else if (_map[j, i] == (int)EInternalStates.E_FLAMABLE)
                //        {
                //            Debug.Log("Cell flamable");
                //            //randomly set fire
                //            if (RandomStatus(flamableNeighbours))
                //            {
                //                Debug.Log("Randomly set flamable cell on fire");
                //                newMap[j, i] = (int)EInternalStates.E_BURNING;
                //            }
                //            else
                //            {
                //                Debug.Log("Stay flamable");
                //                newMap[j, i] = (int)EInternalStates.E_FLAMABLE;
                //            }
                //            stepsPassed++;
                //        }
                //        else if (_map[j, i] == (int)EInternalStates.E_INFLAMABLE)
                //        {
                //            Debug.Log("Cell inflamable");
                //            if (RandomStatus(flamableNeighbours))
                //            {
                //                Debug.Log("Randomly set inflamable cell to flamable");
                //                newMap[j, i] = (int)EInternalStates.E_FLAMABLE;
                //            }
                //            else
                //            {
                //                Debug.Log("Stay inflamable");
                //                newMap[j, i] = (int)EInternalStates.E_INFLAMABLE;
                //            }
                //            stepsPassed++;
                //        }
                //        else if (_map[j, i] == (int)EInternalStates.E_BURNT)
                //        {
                //            Debug.Log("Stay burnt");
                //            newMap[j, i] = (int)EInternalStates.E_BURNT;
                //            stepsPassed++;
                //        }
                //        else
                //        {
                //            Debug.Log("Cell invalid");
                //            newMap[j, i] = (int)EInternalStates.E_INVALID;
                //            stepsPassed++;
                //        }
                //    }
                #endregion
            }
        }
        UpdateMap(newMap, newWidth, newHeight);
        return newMap;
    }

    /// <summary>
    /// Updates the count of flamable neighbours of a Cell at posX, poxY on _map
    /// </summary>
    /// <param name="_map">the map which neighbourcount is wished to be determinated</param>
    /// <param name="_posOnMapX"></param>
    /// <param name="_posOnMapY"></param>
    //private void GetAllCurrentNeighbours(int[,] _map, int _posOnMapX, int _posOnMapY)
    //{
    //    int mapWidth = _map.GetLength(0);
    //    int mapHeight = _map.GetLength(1);

    //    for (int i = _posOnMapY - 1; i <= _posOnMapY + 1; i++)
    //    {
    //        for (int j = _posOnMapX - 1; j <= _posOnMapX + 1; j++)
    //        {
    //            //origin -> not a neighbour
    //            if (i == _posOnMapY && j == _posOnMapX)
    //            {
    //                continue;
    //            }
    //            // check if position on map
    //            if (i >= 0 && i < mapHeight && j >= 0 && j < mapWidth)
    //            {
    //                //if flamable
    //                if (_map[j, i] == (int)EInternalStates.E_FLAMABLE)
    //                {
    //                    flamableNeighbours++;
    //                }else if(_map[j, i] == (int)EInternalStates.E_INFLAMABLE)
    //                {
    //                    inflamableNeighbours++;
    //                }else if(_map[j, i] == (int)EInternalStates.E_BURNING)
    //                {
    //                    burningNeighbours++;
    //                }else if(_map[j, i] == (int)EInternalStates.E_BURNT)
    //                {
    //                    burntNeighbours++;
    //                }
    //            }
    //        }
    //    }
    //    //Debug.Log($"inflamable neighbours: {inflamableNeighbours} flamable neighbours: {flamableNeighbours} burning neighbours: {burningNeighbours} burnt neighbours: {burntNeighbours}");
    //}

    /// <summary>
    /// Returns all currently burning neighbours
    /// </summary>
    /// <param name="_map"> current map </param>
    /// <param name="_posOnMapX">posX on map</param>
    /// <param name="_posOnMapY">posY on map</param>
    private int ReturnAllBurningNeighbours(int[,] _map, int _posOnMapX, int _posOnMapY)
    {
        int mapWidth = _map.GetLength(0);
        int mapHeight = _map.GetLength(1);
        int currentBurningNeighbours = 0;
        for (int i = _posOnMapY - 1; i <= _posOnMapY + 1; i++)
        {
            for (int j = _posOnMapX - 1; j <= _posOnMapX + 1; j++)
            {
                //origin -> not a neighbour
                if (i == _posOnMapY && j == _posOnMapX)
                {
                    continue;
                }
                // check if position on map
                if (i >= 0 && i < mapHeight && j >= 0 && j < mapWidth)
                {
                    //if burning
                    if (_map[j, i] == (int)EInternalStates.E_BURNING)
                    {
                        currentBurningNeighbours++;
                    }
                }
            }
        }
        Debug.Log("burning neighbours: " + currentBurningNeighbours);
        return currentBurningNeighbours;
    }

    /// <summary>
    /// Returns all currently burning neighbours
    /// </summary>
    /// <param name="_map"> current map </param>
    /// <param name="_posOnMapX">posX on map</param>
    /// <param name="_posOnMapY">posY on map</param>
    private int ReturnAllBurntNeighbours(int[,] _map, int _posOnMapX, int _posOnMapY)
    {
        int mapWidth = _map.GetLength(0);
        int mapHeight = _map.GetLength(1);
        int currentBurntNeighbours = 0;
        for (int i = _posOnMapY - 1; i <= _posOnMapY + 1; i++)
        {
            for (int j = _posOnMapX - 1; j <= _posOnMapX + 1; j++)
            {
                //origin -> not a neighbour
                if (i == _posOnMapY && j == _posOnMapX)
                {
                    continue;
                }
                // check if position on map
                if (i >= 0 && i < mapHeight && j >= 0 && j < mapWidth)
                {
                    //if burning
                    if (_map[j, i] == (int)EInternalStates.E_BURNT)
                    {
                        currentBurntNeighbours++;
                    }
                }
            }
        }
        Debug.Log("burnt neighbours: " + currentBurntNeighbours);
        return currentBurntNeighbours;
    }

    /// <summary>
    /// Updates the map after each step of the algorithm or when a cell's state changed
    /// </summary>
    /// <param name="_map">the map to be updated</param>
    /// <param name="_mapHeight">the map's height</param>
    /// <param name="_mapWidth">the map's width</param>
    private void UpdateMap(int[,] _map, int _mapWidth, int _mapHeight)
    {
        int textGOsCounter = 0;
        int textGOsMaxCounter = allTextGOs.Count;
        //Debug.Log($" intmap width: {_mapWidth} height: {_mapHeight}  |  width: {allCells.GetLength(0)} cell height: {allCells.GetLength(1)}");
        for (int i = 0; i < _mapHeight; i++)
        {
            for (int j = 0; j < _mapWidth; j++)
            {
                //Updates the state in the cells 
                allCells[j, i].UpdateCellStatus(_map[j, i] == (int)EInternalStates.E_INFLAMABLE, _map[j, i] == (int)EInternalStates.E_FLAMABLE, _map[j, i] == (int)EInternalStates.E_BURNING, _map[j, i] == (int)EInternalStates.E_BURNT);
                //Update cell displays
                if (displayStates && textGOsCounter <= textGOsMaxCounter)
                {
                    GameObject currentTextGO = allTextGOs[textGOsCounter];
                    TextMeshPro textMesh = currentTextGO.GetComponent<TextMeshPro>();
                    textMesh.text = ((int)(allCells[j, i].state)).ToString();
                    textGOsCounter++;
                }
            }
        }
    }

    /// <summary>
    /// Callback from cell to see if a cell's state was changed
    /// </summary>
    /// <param name="_position">position of cell on the 2d array</param>
    /// <param name="_flamable"></param>
    /// <param name="_flamable"></param>
    /// <param name="_burning"></param>
    private void OnCellChanged(Vector2Int _position, bool _inflamable, bool _flamable, bool _burning, bool _burnt)
    {
        if (_inflamable)
        {
            map[_position.x, _position.y] = (int)EInternalStates.E_INFLAMABLE;
        }
        if (_flamable)
        {
            map[_position.x, _position.y] = (int)EInternalStates.E_FLAMABLE;
        }
        else if (_burning)
        {
            map[_position.x, _position.y] = (int)EInternalStates.E_BURNING;
        }
        else if(_burnt) 
        {
            map[_position.x, _position.y] = (int)EInternalStates.E_BURNT;
        }
        UpdateMap(map, map.GetLength(0), map.GetLength(1));
    }

    /// <summary>
    /// Returns a random value between lower(inclusive) and upper(exclusive)
    /// </summary>
    /// <param name="_lowerIncl">Inclusive lower number</param>
    /// <param name="_higherExcl">Exclusive upper number</param>
    /// <returns>random number in range</returns>

    //private bool RandomStatus(int _neighbourCount)
    //{
    //    float probability = _neighbourCount / 7;
    //    if (probability >= 1)
    //    {
    //        return true;
    //    }
    //    else return false;
    //}

    private void CameraZoom()
    {
        cameraRef.transform.position = new Vector3(0f, cameraZoom, 0f);
    }
}

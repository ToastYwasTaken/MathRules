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
    [SerializeField]
    private bool randomlyGenerated;
    [SerializeField, Range(0, 100)]
    private float fillPercent;
    [Tooltip("For no seed enter 'noSeed' or nothing")]
    [SerializeField]
    private string seedString;
    [Tooltip("For no seed enter 'noSeed' or nothing")]
    [SerializeField]
    private string seedStringForCells;
    [SerializeField]
    private int switchStatusAfterSteps = 10;

    private int[,] map;
    private Cell[,] allCells;
    private int seedInt;
    private const int defaultMapValue = 0;
    private float timer;
    private Camera cameraRef;
    private const float defaultCamDistance = 25f;
    private float currentCamDistance;
    private int stepsPassed = 0;
    private List<GameObject> allTextGOs = new List<GameObject>();

    private void Awake()
    {
        cameraRef = FindObjectOfType<Camera>();
        cameraRef.transform.position = new Vector3(0f, defaultCamDistance, 0f);
        currentCamDistance = defaultCamDistance;
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
    private int[,] GenerateMap(int _width, int _height, bool _randomlyGenerated, float _fillPercent)
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
                    tempMap[j, i] = (float)rdm.NextDouble() * 100f <= fillPercent ? 1 : 0;
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
    private Cell[,] SpawnMap(int[,] map, int _mapHeight, int _mapWidth, float _size)
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
                //get all active neighbours | max 8 active neighbours
                int activeNeighbours = GetActiveNeighbours(_map, j, i);
                #region old rules
                ////Rules here
                ////Cell active
                //if (_map[j, i] == (int)EInternalStates.E_FLAMABLE)
                //{
                //    //Rule 1: if less than 2 neighbours -> inflamable
                //    if (activeNeighbours <= 1)
                //    {
                //        //here
                //        Debug.Log("cell flamable | activeNeighbors <=2 | state => 0");
                //        newMap[j, i] = (int)EInternalStates.E_INFLAMABLE;
                //    }
                //    //Rule 2: if more than 2 neighbours -> stay flamable
                //    else if (activeNeighbours >= 3)
                //    {
                //        Debug.Log("cell flamable | activeNeighbors >= 9 | state => 2");
                //        newMap[j, i] = (int)EInternalStates.E_BURNING;
                //    }
                //    //Rule 3: if between -> randomly set status
                //    else
                //    {
                //        //here
                //        //Debug.Log("cell flamable | activeNeighbors other | random 0, 1 or 2");
                //        //newMap[j, i] = RandomStatus(0, 3);
                //        Debug.Log("cell flamable | in else");
                //        _map[j, i] = (int)EInternalStates.E_FLAMABLE;
                //    }
                //    //Cell inactive
                //}
                //else if (_map[j, i] == (int)EInternalStates.E_INFLAMABLE)
                //{
                //    //Rule 4: if less than 3 neighbours -> stay inflamable
                //    if (activeNeighbours <= 2)
                //    {
                //        //Here
                //        Debug.Log("cell inflamable | activeNeighbors <=6 | state => 0");
                //        newMap[j, i] = (int)EInternalStates.E_INFLAMABLE;
                //    }
                //    //Rule 5: if more than 4 neighbours -> flamable
                //    else if (activeNeighbours >= 5)
                //    {
                //        Debug.Log("cell inflamable | activeNeighbors >=9 | state => 1");
                //        newMap[j, i] = (int)EInternalStates.E_FLAMABLE;
                //    }
                //    //Rule 6: if betweem -> randomly set status to flamable or inflamable
                //    else
                //    {
                //        //Debug.Log("cell inflamable | activeNeighbors other | state => random 0 or 1");
                //        //newMap[j, i] = RandomStatus(0, 2);
                //        Debug.Log("cell inflamable | in else");
                //        newMap[j, i] = (int)EInternalStates.E_INFLAMABLE;
                //    }
                //}
                #endregion

                if(activeNeighbours >= 1)
                {
                    if (newMap[j, i] == (int)EInternalStates.E_BURNING)
                    {
                        Debug.Log("Cell burning");
                        //switch to burnt after burning for x steps
                        if (stepsPassed >= switchStatusAfterSteps)
                        {
                            Debug.Log("Setting cell burnt");
                            newMap[j,i] = (int)EInternalStates.E_BURNT;
                        }
                        else
                        {
                            Debug.Log("Cell stays burning");
                        }
                    }
                    else if (newMap[j, i] == (int)EInternalStates.E_FLAMABLE)
                    {
                        Debug.Log("Cell flamable");
                        //randomly set fire
                        if (RandomStatus(activeNeighbours))
                        {
                            Debug.Log("Set flamable cell on fire");
                            newMap[j, i] = (int)EInternalStates.E_BURNING;
                        }
                        else
                        {
                            Debug.Log("Don't set flamable cell on fire");
                        }
                    }
                    else
                    {
                        Debug.Log("cell not burning or flamable");
                        stepsPassed++;
                    }
                }
            }
        }
        stepsPassed++;
        UpdateMap(newMap, newHeight, newWidth);
        return newMap;
    }

    /// <summary>
    /// Gets the neighbourcount of a Cell at posX, poxY on _map
    /// </summary>
    /// <param name="_map">the map which neighbourcount is wished to be determinated</param>
    /// <param name="_posOnMapX"></param>
    /// <param name="_posOnMapY"></param>
    /// <returns>count of active neighbours</returns>
    private int GetActiveNeighbours(int[,] _map, int _posOnMapX, int _posOnMapY)
    {
        int mapWidth = _map.GetLength(0);
        int mapHeight = _map.GetLength(1);
        int neighbourCount = 0;

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
                    //if flamable
                    if (_map[j, i] == (int)EInternalStates.E_FLAMABLE)
                    {
                        neighbourCount++;
                    }
                }
            }
        }
        //Debug.Log($"neighbours: {neighbourCount}");
        return neighbourCount;
    }

    /// <summary>
    /// Updates the map after each step of the algorithm or when a cell's state changed
    /// </summary>
    /// <param name="_map">the map to be updated</param>
    /// <param name="_mapHeight">the map's height</param>
    /// <param name="_mapWidth">the map's width</param>
    private void UpdateMap(int[,] _map, int _mapHeight, int _mapWidth)
    {
        int textGOsCounter = 0;
        int textGOsMaxCounter = allTextGOs.Count;
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
    //private int RandomStatus(int _lowerIncl, int _higherExcl)
    //{
    //    seedInt = GenerateSeedStringToInt(seedString);
    //    System.Random rdm = new System.Random(seedInt);
    //    int rdmInt = rdm.Next(_lowerIncl, _higherExcl);
    //    return rdmInt;
    //}

    private bool RandomStatus(int _neighbourCount)
    {
        float probability = _neighbourCount / 8;
        if (probability >= 1)
        {
            return true;
        }
        else return false;
    }

    private void CameraZoom()
    {
        cameraRef.transform.position = new Vector3(0f, cameraZoom, 0f);
    }
}

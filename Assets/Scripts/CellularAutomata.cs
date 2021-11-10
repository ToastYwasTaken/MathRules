using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellularAutomata : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private float cellSize;
    [SerializeField]
    private int fps;
    [SerializeField, Range(0.1f, 2f)]
    private float simulationSpeed;
    [Header("Prefabs")]
    [SerializeField]
    private GameObject cellFlamable;
    [SerializeField]
    private GameObject cellInflamable;
    [SerializeField]
    private GameObject cellBurning;
    //TODO possibly add more mats
    [Header("Materials")]
    [SerializeField]
    private Material materialBurningSlightly;   //orange
    [SerializeField]
    private Material materialBurningNormal; //orange-red
    [SerializeField]
    private Material materialBurningExtremely;  //deep red
    [SerializeField]
    private Material materialFlamable;  //green-brown
    [SerializeField]
    private Material materialInflamable;    //grey/white

    private Cell[,] allCells;
    [Header("Map generation")]
    private int[,] map;
    [SerializeField]
    private int width;
    [SerializeField]
    private int height;
    [SerializeField]
    private bool randomlyGenerated;
    [SerializeField, Range(0, 100)]
    private float fillPercent;
    [SerializeField]
    private string seedString;

    private int seedInt;
    private const int defaultMapValue = 0;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        map = GenerateMap(width, height, randomlyGenerated, fillPercent, defaultMapValue);
    }

    // Update is called once per frame
    void Update()
    {
        map = StepInGame(map);
    }

    private void FixedUpdate()
    {
        //Start / stop simulation
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (timer <= 0)
            {
                map = StepInGame(map);
                timer += (1 / (float)fps) * simulationSpeed;
            }
        }
    }

    private int GenerateSeedStringToInt(string _seedString)
    {
        seedInt = System.DateTime.Now();
    }

    private int[,] GenerateMap(int _width, int _height, bool _randomlyGenerated, float _fillPercent)
    {
        seedInt = GenerateSeedStringToInt(seedString).GetHashCode;
        System.Random rdm = new System.Random(seedInt);
        int[,] tempMap = new int[_width, _height];
        for (int i = 0; i < _height; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                if (_randomlyGenerated)
                {
                    tempMap[j, i] = rdm <= fillPercent ? 1 : 0;
                }
                //sets cells to default value only for debugging purposes
                else tempMap[j, i] = defaultMapValue;
            }
        }
        return tempMap;
    }

    private void StepInGame(int[,] _map)
    {

    }
}

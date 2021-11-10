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

    private float timer;
    private Cell[,] allCells;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

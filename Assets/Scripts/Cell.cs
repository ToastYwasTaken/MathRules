using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    private CellularAutomata cellularAutomata;
    [SerializeField]
    private Renderer rend;
    private Material materialInflamable;
    private Material materialFlamable;
    private Material materialBurningSlightly;
    private Material materialBurningNormal;
    private Material materialBurningExtremly;

    private Vector2Int idxInArray;

    private bool active;

    private void Awake()
    {
        rend = this.gameObject.GetComponent<Renderer>();
    }
    private void AssignCell(CellularAutomata _cellularAutomata, Material _materialInflamable, Material _materialFlamable, Material _materialBurningSlightly, Material _materialBurningNormal, Material _materialBurningExtremly, int idxX, int idxY)
    {
        cellularAutomata = _cellularAutomata;
        materialInflamable = _materialInflamable;
        materialFlamable = _materialFlamable;
        materialBurningSlightly = _materialBurningSlightly;
        materialBurningNormal = _materialBurningNormal;
        materialBurningExtremly = _materialBurningExtremly;
        idxInArray = new Vector2Int(idxX, idxY);
    }


}

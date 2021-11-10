using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Cell : MonoBehaviour
{
    private CellularAutomata cellularAutomata;
    [SerializeField]
    private Renderer rend;
    [Header("Flamable or inflamable")]
    private Material materialInflamable;
    private Material materialFlamable;
    [Header("Burning mats")]
    private Material materialBurningSlightly;
    private Material materialBurningNormal;
    private Material materialBurningExtremly;

    private Vector2Int idxInArray;

    //Callback for burning
    private Action<Vector2Int, bool> callBack;
    
    private bool burning;
    private bool flamable;

    private void Awake()
    {
        rend = this.gameObject.GetComponent<Renderer>();
    }

    private void AssignCell(CellularAutomata _cellularAutomata, Material _materialInflamable, Material _materialFlamable, Material _materialBurningSlightly, Material _materialBurningNormal, Material _materialBurningExtremly, int idxX, int idxY, Action<Vector2Int, bool> _callback, bool _flamable, bool _burning)
    {
        cellularAutomata = _cellularAutomata;
        materialInflamable = _materialInflamable;
        materialFlamable = _materialFlamable;
        materialBurningSlightly = _materialBurningSlightly;
        materialBurningNormal = _materialBurningNormal;
        materialBurningExtremly = _materialBurningExtremly;
        idxInArray = new Vector2Int(idxX, idxY);
        callBack = _callback;
        flamable = _flamable;
        burning = _burning;
        callBack = _callback;
        SetMaterial(active, burning);

    }

    //TODO: implement fire spreading
    private void SetMaterial(bool _flamable, bool _burning)
    {
        if (_flamable)
        {
            if (true)
            {

            } else if (true)
            {

            } else if (true)
            {

            } else return;
        } else if (_burning)
        {
            if (true)
            {

            }
            else if (true)
            {

            }
            else if (true)
            {

            }
            else return;
        } else return;
       
    }

    private void UpdateCellStatus(bool _burning)
    {
        burning = _burning;
    }

    private void OnMouseOver()
    {
        bool switched = false;
        //Set fire
        if (Input.GetKeyDown(KeyCode.Mouse0) && !burning && flamable)
        {
            burning = true;
            switched = true;
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1) && burning)
        {
            burning = false;
            switched = true;
        }
        if (switched)
        {
            callBack(idxInArray, burning);
        }
    }

}

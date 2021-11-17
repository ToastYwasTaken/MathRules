using System;
using UnityEngine;
/******************************************************************************
 * Project: MathRules
 * File: Cell.cs
 * Version: 1.01
 * Autor:  Franz M�rike (FM);
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
 * ChangeLog
 * ----------------------------
 *  11.11.2021  created
 *  13.11.2021  added comments
 *  14.11.2021  added code
 *  
 *****************************************************************************/
public class Cell : MonoBehaviour
{
    private CellularAutomata cellularAutomata;
    private Renderer rend;

    //Flamable or inflamable
    private Material materialInflamable;
    private Material materialFlamable;

    //Burning mats
    private Material materialBurning;
    private Material materialBurnt;

    private Vector2Int idxInArray;
    private int seedIntForCells;

    //Callback for [first bool] inflamable [second bool] flamable [third bool] burning [fourth bool] burnt
    private Action<Vector2Int, bool, bool, bool, bool> callback;
    private bool inflamable;
    private bool flamable;
    private bool burning;   
    private bool burnt;
    
    private EInternalStates state;

    private void Awake()
    {
        rend = this.gameObject.GetComponent<Renderer>();
    }

    /// <summary>
    /// Used to assign the cells from CellularAutomata
    /// </summary>
    /// <param name="_cellularAutomata">reference to the Cellular automata</param>
    /// <param name="_materialInflamable">material</param>
    /// <param name="_materialFlamable">material</param>
    /// <param name="_materialBurning">material</param>
    /// <param name="_materialBurnt">material</param>
    /// <param name="_seedIntForCells">seed used to spread fire</param>
    /// <param name="idxX">X coordinate of the cell in the 2D array of cells </param>
    /// <param name="idxY">Y coordinate of the cell in the 2D array of cells </param>
    /// <param name="_callback">Callback for igniting fire</param>
    /// <param name="_flamable">default: false</param>
    /// <param name="_burning">default: false</param>
    public void AssignCell(CellularAutomata _cellularAutomata, Material _materialInflamable, Material _materialFlamable, Material _materialBurning, Material _materialBurnt, int _seedIntForCells, int idxX, int idxY, Action<Vector2Int, bool, bool, bool, bool> _callback, bool inflamable = false, bool _flamable = false, bool _burning = false, bool _burnt = false)
    {
        cellularAutomata = _cellularAutomata;
        idxInArray = new Vector2Int(idxX, idxY);

        materialInflamable = _materialInflamable;
        materialFlamable = _materialFlamable;
        materialBurning = _materialBurning;
        materialBurnt = _materialBurnt;

        callback = _callback;
        flamable = _flamable;
        burning = _burning;

        seedIntForCells = _seedIntForCells;
        // Set flamable Material if cell is flamable -> else set inflamable material
        //Debug.Log($"inflamable: {inflamable} | flamable: {flamable} | burning: {burning} | burnt : {burnt}");
        SetMaterial(flamable ? materialFlamable : materialInflamable);
    }

    /// <summary>
    /// Set foundary material of cell
    /// </summary>
    /// <param name="_material"></param>
    private void SetMaterial(Material _material)
    {
        Debug.Log(_material);
        rend.material = _material;
    }

    //TODO: implement fire spreading here
    /// <summary>
    /// Change the material accordingly when fire is set
    /// </summary>
    /// <param name="_materialBurningSlightly">material</param>
    /// <param name="_materialBurningNormal">material</param>
    /// <param name="_materialBurningExtremely">material</param>
    private void SetRandomBurningMaterial(Material _materialBurningSlightly, Material _materialBurningNormal, Material _materialBurningExtremely)
    {
        System.Random rdm = new System.Random(seedIntForCells);
        //1 rdm int for each Material
        int rdmIntBurning = rdm.Next(0, 3);
        //burning slightly
        if (rdmIntBurning == (int)EInternalStates.E_INFLAMABLE)
        {
            rend.material = _materialBurningSlightly;
        }
        //burning normal
        else if (rdmIntBurning == (int)EInternalStates.E_FLAMABLE)
        {
            rend.material = _materialBurningNormal;
        }
        //burning extremely;
        else if (rdmIntBurning == (int)EInternalStates.E_BURNING)
        {
            rend.material = _materialBurningExtremely;
        }
        else return;
        Debug.Log(rend.material);
    }

    /// <summary>
    /// Updates the cells in case they switched states
    /// </summary>
    /// <param name="_flamable">state passed by Cellular automata</param>
    /// <param name="_burning">state passed by Cellular automata</param>
    public void UpdateCellStatus(bool _inflamable, bool _flamable, bool _burning, bool _burnt)
    {
        inflamable = _inflamable;
        flamable = _flamable;
        burning = _burning;
        burnt = _burnt;
        if (inflamable)
        {
            SetMaterial(materialInflamable);
        }else if (flamable)
        {
            SetMaterial(materialFlamable);
        }
        else if (burning)
        {
            SetMaterial(materialBurning);
        }
        else if (burnt)
        {
            SetMaterial(materialBurnt);
        }
    }

    private void OnMouseOver()
    {
        bool switched = false;
        //Set fire
        if (Input.GetMouseButton(0))
        {
            if (!burning && flamable)
            {
                Debug.Log("Setting fire");
                burning = true;
                switched = true;
            }else if (burning)
            {
                Debug.Log("Remove fire");
                burning = false;
                switched = true;
            }
        }

        if (switched)
        {
            callback(idxInArray, inflamable, flamable, burning, burnt);
        }
    }

    /// <summary>
    /// Getter for Cellular automata
    /// </summary>
    /// <returns>current state</returns>
    public EInternalStates GetCurrentState()
    {
        if (!flamable)
        {
            state = EInternalStates.E_INFLAMABLE;
        }
        else if (flamable && !burning)
        {
            state = EInternalStates.E_FLAMABLE;
        }
        else if (flamable && burning)
        {
            state = EInternalStates.E_BURNING;
        }
        else state = EInternalStates.E_INVALID;

        return state;
    }


}

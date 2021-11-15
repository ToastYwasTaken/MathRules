using System;
using UnityEngine;
/******************************************************************************
 * Project: MathRules
 * File: Cell.cs
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
    private Material materialBurningSlightly;
    private Material materialBurningNormal;
    private Material materialBurningExtremly;

    private Vector2Int idxInArray;
    private int seedIntForCells;

    //Callback for [first bool] flamable and [second bool] burning
    private Action<Vector2Int, bool, bool> callback;
    private bool flamable;
    private bool burning;
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
    /// <param name="_materialBurningSlightly">material</param>
    /// <param name="_materialBurningNormal">material</param>
    /// <param name="_materialBurningExtremly">material</param>
    /// <param name="_seedIntForCells">seed used to spread fire</param>
    /// <param name="idxX">X coordinate of the cell in the 2D array of cells </param>
    /// <param name="idxY">Y coordinate of the cell in the 2D array of cells </param>
    /// <param name="_callback">Callback for igniting fire</param>
    /// <param name="_flamable">default: false</param>
    /// <param name="_burning">default: false</param>
    public void AssignCell(CellularAutomata _cellularAutomata, Material _materialInflamable, Material _materialFlamable, Material _materialBurningSlightly, Material _materialBurningNormal, Material _materialBurningExtremly, int _seedIntForCells, int idxX, int idxY, Action<Vector2Int, bool, bool> _callback, bool _flamable = false, bool _burning = false)
    {
        cellularAutomata = _cellularAutomata;
        idxInArray = new Vector2Int(idxX, idxY);

        materialInflamable = _materialInflamable;
        materialFlamable = _materialFlamable;
        materialBurningSlightly = _materialBurningSlightly;
        materialBurningNormal = _materialBurningNormal;
        materialBurningExtremly = _materialBurningExtremly;

        callback = _callback;
        flamable = _flamable;
        burning = _burning;

        seedIntForCells = _seedIntForCells;
        // Set flamable Material if cell is flamable -> else set inflamable material
        //Debug.Log($"flamable: {flamable} burning: {burning}");
        SetFlamableMaterial(flamable ? materialFlamable : materialInflamable);
    }

    /// <summary>
    /// Set foundary material of cell
    /// </summary>
    /// <param name="_material"></param>
    private void SetFlamableMaterial(Material _material)
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
    public void UpdateCellStatus(bool _flamable, bool _burning)
    {
        flamable = _flamable;
        burning = _burning;
        if (flamable)
        {
            SetFlamableMaterial(materialFlamable);
        }
        else if (flamable && burning)
        {
            SetRandomBurningMaterial(materialBurningSlightly, materialBurningNormal, materialBurningExtremly);
        }
        else SetFlamableMaterial(materialInflamable);
    }

    private void OnMouseOver()
    {
        bool switched = false;
        //Set fire
        if (Input.GetKeyDown(KeyCode.Mouse0) )
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
            callback(idxInArray, flamable, burning);
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

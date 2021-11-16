using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/******************************************************************************
 * Project: KISimulation
 * File: PlayerManager.cs
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
 * ChangeLog
 * ----------------------------
 *  16.11.2021  created (imported from Project KISimulation)
 *              adjusted for usage here (changed to FP player controller)
 *  
 *****************************************************************************/
public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    float playerSpeed;
    [SerializeField]
    float slideSpeed;
    [SerializeField]
    private bool onIce;

    private CharacterController characterController;
    private float rotateX, rotateY;
    private const float gravityConstant = -9.81f;


    void Awake()
    {
        characterController = this.GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        RotatePlayer();
    }

    /// <summary>
    /// Player movement
    /// </summary>
    private void MovePlayer()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 fallDrag = Vector3.zero;
        Vector3 direction = transform.right * horizontal + transform.forward * vertical;
        //calculate slide direction to make player slide when on ice
        Vector3 slideDirection = transform.right * horizontal * slideSpeed + transform.forward * vertical * slideSpeed;
        Vector3 velocity;
        //calculates velocity depending on the surface standing on
        Debug.Log("onIce: " + onIce);
        if (onIce)
        {
            velocity = slideDirection * playerSpeed * Time.deltaTime;
        }
        else
        { 
            velocity = direction * playerSpeed * Time.deltaTime; 
        }
        characterController.Move(velocity);
        //calculate drag so the player doesn't fly
        fallDrag.y = gravityConstant * Time.deltaTime;
        characterController.Move(fallDrag);
    }

    /// <summary>
    /// Player rotation
    /// </summary>
    private void RotatePlayer()
    {
        rotateX += Input.GetAxis("Mouse X");
        rotateY += Input.GetAxis("Mouse Y");
        transform.localRotation = Quaternion.Euler(-rotateY, rotateX, 0);

    }

    /// <summary>
    /// Switches bool depending on colliding surface
    /// </summary>
    /// <param name="hit"></param>
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Snowy Plane"))
        {
            onIce = true;
        }
        else onIce = false;
    }


}

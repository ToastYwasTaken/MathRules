using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGameManager : MonoBehaviour
{
    private MyGameManager instance;
    // Start is called before the first frame update
    void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
    }

    private void GetInput()
    {
        //Start / stop simulation
        if (Input.GetKeyDown(KeyCode.Space))
        {

        }
        //Set fire
        else if (Input.GetKeyDown(KeyCode.Mouse0))
        {

        }
    }
}

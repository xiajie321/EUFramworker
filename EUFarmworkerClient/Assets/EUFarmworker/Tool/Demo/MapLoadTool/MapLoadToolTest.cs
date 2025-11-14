using System;
using System.Collections;
using System.Collections.Generic;
using EUFarmworker.Tool.DoubleTileTool.Script;
using EUFarmworker.Tool.DoubleTileTool.Script.Generate;
using EUFarmworker.Tool.MapLoadTool.Script;
using UnityEngine;

public class MapLoadToolTest : MonoBehaviour
{
    [SerializeField]
    GameObject LookGameObject;
    void Start()
    {
        Application.targetFrameRate = 120;
        DoubleTileTool.Init();
        MapLoadTool.Init();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            LookGameObject.transform.position += new Vector3(0, 2 * 10 * Time.deltaTime, 0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            LookGameObject.transform.position += new Vector3( -2 * 10 * Time.deltaTime, 0,0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            LookGameObject.transform.position += new Vector3(0, -2 * 10 * Time.deltaTime, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            LookGameObject.transform.position += new Vector3( 2 * 10 * Time.deltaTime, 0,0);
        }
    }

    private void FixedUpdate()
    {
        MapLoadTool.LookPosition(LookGameObject.transform.position);
    }
}

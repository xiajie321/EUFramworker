using System;
using System.Collections;
using System.Collections.Generic;
using EUFarmworker.Tool.DoubleTileTool.Script;
using EUFarmworker.Tool.DoubleTileTool.Script.Generate;
using EUFarmworker.Tool.MapLoadTool.Script;
using QFramework;
using UnityEngine;

public class MapLoadToolTest : MonoBehaviour
{
    [SerializeField]
    GameObject LookGameObject;
    void Start()
    {
        Application.targetFrameRate = -1;
        MapLoadTool.Init();
        ActionKit.Delay(3, ()=>MapLoadTool.Init()).Start(this);
        //DoubleTileTool.SetTiles(new Vector3Int[] {new(0,0,0),new(1,0,0)},new TileType[] { TileType.瓦片0 , TileType.瓦片0});
        //DoubleTileTool.SetTile(new Vector3(0,1,0),TileType.瓦片0);
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
        //Debug.Log(GC.GetTotalMemory(false)/(1024*1024));
    }
}

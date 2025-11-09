using System;
using System.Collections;
using System.Collections.Generic;
using EUFarmworker.Tool.DoubleTileTool.Script;
using EUFarmworker.Tool.DoubleTileTool.Script.Generate;
using UnityEngine;

public class DoubleTileToolTest : MonoBehaviour
{
    private void Start()
    {
        DoubleTileTool.Init();//初始化瓦片系统工具
        // 生成瓦片
        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                DoubleTileTool.SetTile( new Vector3(-50+i, -50+j, 0), (TileType)0);
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 ls = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            ls.z = 0;
            DoubleTileTool.SetTile(ls,(TileType)1);//设置瓦片
            //Debug.Log(DoubleTileTool.GetTile(ls));//获取当前位置的瓦片信息
        }

        if (Input.GetMouseButtonDown(1))
        {
            DoubleTileTool.ShowTagGrid(true);//显示标记
        }
        
        

    }
}

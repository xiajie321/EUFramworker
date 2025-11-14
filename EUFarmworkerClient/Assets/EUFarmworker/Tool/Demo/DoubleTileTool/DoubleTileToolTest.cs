using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
        //注册瓦片改变的事件
        DoubleTileTool.RegisterTileChangeEvent(v=>
        {
            Debug.Log($"执行{v.Position} {v.OldTileType} {v.NewTileType}");
        });
    }
    
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 ls = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            ls.z = 0;
            DoubleTileTool.LoadTile(ls);//加载对应位置的瓦片
            
            // DoubleTileTool.SetTile(ls,(TileType)1);//设置瓦片
            
            //Debug.Log(DoubleTileTool.GetTile(ls));//获取当前位置的瓦片信息
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 ls = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            ls.z = 0;
             DoubleTileTool.SetTile(ls,(TileType)1);//设置瓦片
            //DoubleTileTool.ShowTagGrid(true);//显示标记
        }

        if (Input.GetMouseButtonDown(2))
        {
            Vector3 ls = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            ls.z = 0;
            DoubleTileTool.UninstallTile(ls);//卸载对应位置的瓦片
        }
    }
}

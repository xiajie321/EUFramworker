using System;
using System.Collections;
using System.Collections.Generic;
using EUFarmworker.Tool.MapLoadTool.Script;
using EUFarmworker.Tool.MapLoadTool.Script.Data;
using UnityEngine;

public class MapLoadToolRunTime : MonoBehaviour
{
    [SerializeField]
    SOMapLoadViewConfig  config;
    public SOMapLoadViewConfig Config =>config;

    private void OnDestroy()
    {
        MapLoadTool.Dispose();
    }
}

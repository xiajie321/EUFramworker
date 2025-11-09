using System;
using EUFarmworker.Tool.DoubleTileTool.Script.Data;
using UnityEngine;

namespace EUFarmworker.Tool.DoubleTileTool.Script
{
    public class DoubleTileToolRunTimeMono:MonoBehaviour
    {
        [SerializeField]
        SODoubleTileViewConfig config;
        public SODoubleTileViewConfig Config =>config;
        
    }
}
using System;
using EUFarmworker.Tool.MapLoadTool.Script.Data.BlockLoadConfig;
using EUFarmworker.Tool.MapLoadTool.Script.Data.MapGenerateConfig;
using EUFarmworker.Tool.MapLoadTool.Script.Data.NoiseConfig;
using UnityEngine;

namespace EUFarmworker.Tool.MapLoadTool.Script.Data.MapLoadConfig
{
    [CreateAssetMenu(fileName = "MapLoadConfigData", menuName = "EUTool/MapLoad/MapLoadConfigData")]
    [Serializable]
    public class SOMapLoadScriptableObject:ScriptableObjectEditorBase
    {
        public SOBlockLoadConfigBase BlockLoadConfig;//区块加载
        public SOMapGenerateConfigBase  MapGenerateConfig;//地图生成
        public SONiseConfigBase SONiseConfig;//噪声
    }
}
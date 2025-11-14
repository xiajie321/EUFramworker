using System;
using System.Collections.Generic;
using System.Numerics;
using EUFarmworker.Tool.DoubleTileTool.Script.Generate;
using EUFarmworker.Tool.MapLoadTool.Script.Data.BlockLoadConfig;
using EUFarmworker.Tool.MapLoadTool.Script.Data.NoiseConfig;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace EUFarmworker.Tool.MapLoadTool.Script.Data.MapGenerateConfig
{
    [CreateAssetMenu(fileName = "DefineGenerateConfig", menuName = "EUTool/MapLoad/GenerateConfig/DefineGenerateConfig")]
    public class SODefineMapGenerateConfig:SOMapGenerateConfigBase
    {
        [SerializeField]
        private List<DefineMapGenerateConfigData> _defineMapGenerateConfigDatas =new();
        public override void SetMapSize(Vector2Int size)
        {
            _mapSize = size;
        }

        public override Vector2Int GetMapSize()
        {
            return _mapSize;
        }
        [NonSerialized]
        SONoiseConfigBase noiseConfig;
        public override void OnInit(SOBlockLoadConfigBase blockLoadConfig, SONoiseConfigBase noiseConfig)
        {
            this.noiseConfig = noiseConfig;
            blockLoadConfig.OnLoadBlockChangeEvent(LoadBlockChange);
            blockLoadConfig.OnUninstallBlockChangeEvent(UninstBlockChange);
            blockLoadConfig.Init();
        }

        private void LoadBlockChange(Vector3Int v)
        {
            if(_mapSize.x >=0 ||  _mapSize.y >= 0)
                if(v.x > _mapSize.x/2 || v.x < -_mapSize.x/2 || v.y > _mapSize.y/2 || v.y < -_mapSize.y/2) return;
            float ls = noiseConfig.OnUse(v);
            foreach (var i in _defineMapGenerateConfigDatas)
            {
                if (ls >= i.heigth)
                {
                    DoubleTileTool.Script.DoubleTileTool.SetTile(v,i.tileType);
                }
            }
        }

        private void UninstBlockChange(Vector3Int v)
        {
            if(_mapSize.x >=0 ||  _mapSize.y >= 0)
                if(v.x > _mapSize.x/2 || v.x < -_mapSize.x/2 || v.y > _mapSize.y/2 || v.y < -_mapSize.y/2) return;
            DoubleTileTool.Script.DoubleTileTool.UninstallTile(v);
        }
    }
    [Serializable]
    public class DefineMapGenerateConfigData
    {
        public float heigth;
        public TileType tileType;
    }
}
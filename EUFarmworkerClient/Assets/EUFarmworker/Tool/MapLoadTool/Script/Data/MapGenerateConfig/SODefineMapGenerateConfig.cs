using System;
using System.Collections.Generic;
using System.Numerics;
using EUFarmworker.Tool.DoubleTileTool.Script.Generate;
using EUFarmworker.Tool.MapLoadTool.Script.Data.BlockLoadConfig;
using EUFarmworker.Tool.MapLoadTool.Script.Data.NoiseConfig;
using Unity.Collections;
using UnityEngine;
using ZLinq;
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
        SONoiseConfigBase _noiseConfig;
        [NonSerialized]
        SOBlockLoadConfigBase _blockLoadConfig;
        public override void OnInit(SOBlockLoadConfigBase blockLoadConfig, SONoiseConfigBase noiseConfig)
        {
            _noiseConfig = noiseConfig;
            _blockLoadConfig = blockLoadConfig;
            blockLoadConfig.OnLoadBlockChangeEvent(LoadBlockChange);
            blockLoadConfig.OnUninstallBlockChangeEvent(UninstBlockChange);
            blockLoadConfig.Init();
        }

        private NativeHashMap<Vector3Int, TileType> _lsLoadTiles;
        private void LoadBlockChange(Vector3Int v)
        {
            if(_mapSize.x >=0 ||  _mapSize.y >= 0)
                if(v.x > _mapSize.x/2 || v.x < -_mapSize.x/2 || v.y > _mapSize.y/2 || v.y < -_mapSize.y/2) return;
            float ls;
            int lsSj = _blockLoadConfig.GetSingleBlockSize();
            int len = lsSj * lsSj;
            _lsLoadTiles = new NativeHashMap<Vector3Int, TileType>(len, Allocator.Temp);
            Vector3Int lspos = v;
            TileType lsType;
            for (int j = 0; j < lsSj; j++)
            {
                for (int k = 0; k < lsSj; k++)
                {
                    ls = _noiseConfig.OnUse(lspos);
                    foreach (var i in _defineMapGenerateConfigDatas)
                    {
                        if (ls <= i.heigth)
                        {
                            lspos = v + new Vector3Int(j, k, 0);
                            if(_lsLoadTiles.ContainsKey(lspos)) break;
                            //Debug.Log($"{ls} {i.tileType}");
                            lsType = i.tileType;
                            _lsLoadTiles.Add(lspos,lsType);
                            break;
                        }
                    }
                    lspos = v + new Vector3Int(j, k, 0);
                    if(_lsLoadTiles.ContainsKey(lspos)) continue;
                    //Debug.Log($"{ls} {i.tileType}");
                    lsType = 0;
                    _lsLoadTiles.Add(lspos,lsType);
                }
            }

            var lsKeys = _lsLoadTiles.GetKeyArray(Allocator.Temp);
            var lsValues = _lsLoadTiles.GetValueArray(Allocator.Temp);
            DoubleTileTool.Script.DoubleTileTool.SetTiles(lsKeys.AsValueEnumerable().ToArray(),lsValues.AsValueEnumerable().ToArray());
            if(lsKeys.IsCreated) lsKeys.Dispose();
            if(lsValues.IsCreated) lsValues.Dispose();
            if(_lsLoadTiles.IsCreated) _lsLoadTiles.Dispose();
        }

        private NativeArray<Vector3Int> _lsUninstallTiles;
        private void UninstBlockChange(Vector3Int v)
        {
            if(_mapSize.x >=0 ||  _mapSize.y >= 0)
                if(v.x > _mapSize.x/2 || v.x < -_mapSize.x/2 || v.y > _mapSize.y/2 || v.y < -_mapSize.y/2) return;
            int lsSj = _blockLoadConfig.GetSingleBlockSize();
            _lsUninstallTiles = new NativeArray<Vector3Int>(lsSj * lsSj, Allocator.Temp);
            int sum = 0;
            for (int i = 0; i < lsSj; i++)
            {
                for (int j = 0; j < lsSj; j++)
                {
                    _lsUninstallTiles[sum]  = v + new Vector3Int(i, j, 0);
                    sum++;
                }
            }
            DoubleTileTool.Script.DoubleTileTool.UninstallTiles(_lsUninstallTiles.AsValueEnumerable().ToArray());
            if(_lsUninstallTiles.IsCreated) _lsUninstallTiles.Dispose();
        }
    }
    [Serializable]
    public class DefineMapGenerateConfigData
    {
        public float heigth;
        public TileType tileType;
    }
}
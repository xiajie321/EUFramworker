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
            Dispose();//防止重新创建的时候容器没有被释放
            _noiseConfig = noiseConfig;
            _blockLoadConfig = blockLoadConfig;
            int lsSj = _blockLoadConfig.GetSingleBlockSize();
            int len = lsSj * lsSj;
            GcInit(len);
            NativeInit(len);
            blockLoadConfig.RegisterLoadBlockChangeEvent(LoadBlockChange);
            blockLoadConfig.RegisterUninstallBlockChangeEvent(UninstBlockChange);
            blockLoadConfig.Init();
        }

        private void NativeInit(int len)
        {
        }

        private void GcInit(int len)
        {
            _lsKeys = new Vector3Int[len];
            _lsValues = new TileType[len];
            _lsUninstallTiles = new Vector3Int[len];
        }
        public override void Dispose()
        {
            
        }
        
        private Vector3Int[] _lsKeys;
        private TileType[] _lsValues;
        private void LoadBlockChange(Vector3Int v)
        {
            if(_mapSize.x >=0 ||  _mapSize.y >= 0)
                if(v.x > _mapSize.x/2 || v.x < -_mapSize.x/2 || v.y > _mapSize.y/2 || v.y < -_mapSize.y/2) return;
            float ls;
            int lsSj = _blockLoadConfig.GetSingleBlockSize();
            Vector3Int lspos = v;
            TileType lsType = 0;
            int sum =0;
            for (int j = 0; j < lsSj; j++)
            {
                for (int k = 0; k < lsSj; k++)
                {
                    ls = _noiseConfig.OnUse(lspos);
                    for (int i =0;i<_defineMapGenerateConfigDatas.Count;i++)
                    {
                        if (ls <= _defineMapGenerateConfigDatas[i].heigth)
                        {
                            lspos = new Vector3Int(v.x+j,v.y+ k, v.z);
                            //Debug.Log($"{ls} {i.tileType}");
                            
                            lsType = _defineMapGenerateConfigDatas[i].tileType;
                            _lsKeys[sum] = lspos;
                            _lsValues[sum] = lsType;
                            sum++;
                            break;
                        }
                        else if (i>= _defineMapGenerateConfigDatas.Count - 1)
                        {
                            lspos = new Vector3Int(v.x+j, v.y+k, v.z);
                            //Debug.Log($"{ls} {i.tileType}");
                            lsType = 0;
                            _lsKeys[sum] = lspos;
                            _lsValues[sum] = lsType;
                            sum++;
                        }
                    }
    
                }
            }
            DoubleTileTool.Script.DoubleTileTool.SetTiles(_lsKeys,_lsValues);
        }

        private Vector3Int[] _lsUninstallTiles;
        private void UninstBlockChange(Vector3Int v)
        {
            if(_mapSize.x >=0 ||  _mapSize.y >= 0)
                if(v.x > _mapSize.x/2 || v.x < -_mapSize.x/2 || v.y > _mapSize.y/2 || v.y < -_mapSize.y/2) return;
            int lsSj = _blockLoadConfig.GetSingleBlockSize();
            int sum = 0;
            for (int i = 0; i < lsSj; i++)
            {
                for (int j = 0; j < lsSj; j++)
                {
                    _lsUninstallTiles[sum]  = new Vector3Int(v.x+i, v.y+j, v.z);
                    sum++;
                }
            }
            DoubleTileTool.Script.DoubleTileTool.UninstallTiles(_lsUninstallTiles);
        }
    }
    [Serializable]
    public class DefineMapGenerateConfigData
    {
        public float heigth;
        public TileType tileType;
    }
}
using System;
using EUFarmworker.Tool.MapLoadTool.Script.Data;
using EUFarmworker.Tool.MapLoadTool.Script.Data.MapGenerateConfig;
using EUFarmworker.Tool.MapLoadTool.Script.Data.NoiseConfig;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EUFarmworker.Tool.MapLoadTool.Script
{
    public static class MapLoadTool
    {
        private static SOMapLoadViewConfig _mapLoadViewConfig;
        private static GameObject root;
        /// <summary>
        /// 初始化地图加载(每次更换新的游戏场景都需要重新加载)
        /// </summary>
        /// <param name="soMapGenerateConfigBase">为空表示地图生成规则为地图默认</param>
        /// <param name="soNoiseConfigBase">为空表示噪声规则为地图默认</param>
        public static void Init(SOMapGenerateConfigBase soMapGenerateConfigBase = null,SONoiseConfigBase soNoiseConfigBase = null)
        {
            if (root)
            {
                Object.DestroyImmediate(root);
            }
            root = Object.Instantiate(Resources.Load<GameObject>("EUFarmworker/MapLoadTool/MapLoadTool"));
            _mapLoadViewConfig = root.GetComponent<MapLoadToolRunTime>().Config;
            if(soMapGenerateConfigBase) _mapLoadViewConfig.ConfigData.MapGenerateConfig = soMapGenerateConfigBase;
            if(soNoiseConfigBase) _mapLoadViewConfig.ConfigData.NoiseConfig = soNoiseConfigBase;
            _mapLoadViewConfig.ConfigData.MapGenerateConfig.OnInit(_mapLoadViewConfig.ConfigData.BlockLoadConfig, _mapLoadViewConfig.ConfigData.NoiseConfig);
        }
        
        /// <summary>
        /// 加载区块事件
        /// </summary>
        /// <param name="callback"></param>
        public static void OnLoadBlockChangeEvent(Action<Vector3Int> callback)
        {
            _mapLoadViewConfig.ConfigData.BlockLoadConfig.RegisterLoadBlockChangeEvent(callback);
        }
        
        /// <summary>
        /// 卸载区块事件
        /// </summary>
        /// <param name="callback"></param>
        public static void OnUninstallBlockChangeEvent(Action<Vector3Int> callback)
        {
            _mapLoadViewConfig.ConfigData.BlockLoadConfig.RegisterUninstallBlockChangeEvent(callback);
        }
        /// <summary>
        /// 视野位置,决定了区块加载。
        /// </summary>
        /// <param name="pos"></param>
        public static void LookPosition(Vector3 pos)
        {
            _mapLoadViewConfig.ConfigData?.BlockLoadConfig?.OnMovePosition(pos);
        }

        /// <summary>
        /// 设置区块显示范围
        /// </summary>
        /// <param name="size"></param>
        public static void SetLookBlockSize(Vector3Int size)
        {
            _mapLoadViewConfig.ConfigData.BlockLoadConfig?.SetLookBlockSize(size);
        }

        /// <summary>
        /// 单个区块的大小
        /// </summary>
        /// <param name="size"></param>
        public static void SetSingleBlockSize(int size)
        {
            _mapLoadViewConfig.ConfigData.BlockLoadConfig?.SetSingleBlockSize(size);
        }

        /// <summary>
        /// 获取当前种子信息
        /// </summary>
        /// <returns></returns>
        public static int GetSend()
        {
            return _mapLoadViewConfig.ConfigData.NoiseConfig.GetSend();
        }

        /// <summary>
        /// 设置种子
        /// </summary>
        /// <param name="value"></param>
        public static void SetSend(int value)
        {
            _mapLoadViewConfig.ConfigData.NoiseConfig?.SetSend(value);
        }

        /// <summary>
        /// 释放缓存
        /// </summary>
        public static void Release()
        {
        }
        public static void Dispose()
        {
            _mapLoadViewConfig.ConfigData.BlockLoadConfig.Dispose();
            _mapLoadViewConfig.ConfigData.NoiseConfig.Dispose();
            _mapLoadViewConfig.ConfigData.MapGenerateConfig.Dispose();
        }
    }
}
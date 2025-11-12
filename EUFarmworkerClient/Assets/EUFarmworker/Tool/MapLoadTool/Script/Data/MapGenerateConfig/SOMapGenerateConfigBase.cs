using EUFarmworker.Tool.MapLoadTool.Script.Data.BlockLoadConfig;
using EUFarmworker.Tool.MapLoadTool.Script.Data.NoiseConfig;
using UnityEngine;

namespace EUFarmworker.Tool.MapLoadTool.Script.Data.MapGenerateConfig
{
    /// <summary>
    /// 描述地图具体是怎么生成的(比如:箱庭?大世界?)
    /// </summary>
    public abstract class SOMapGenerateConfigBase : ScriptableObjectEditorBase
    {
        internal Vector2Int _mapSize;

        /// <summary>
        /// 设置地图大小
        /// </summary>
        /// <param name="size"></param>
        public abstract void SetMapSize(Vector2Int size);

        /// <summary>
        /// 获取地图大小
        /// </summary>
        /// <returns></returns>
        public abstract Vector2Int GetMapSize();

        /// <summary>
        /// 初始化
        /// </summary>
        public abstract void OnInit(SOBlockLoadConfigBase blockLoadConfig, SONoiseConfigBase noiseConfig);
    }
}
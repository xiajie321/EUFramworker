using System;
using UnityEngine;

namespace EUFarmworker.Tool.MapLoadTool.Script.Data.BlockLoadConfig
{
    /// <summary>
    /// 主要作用是描述区块生成算法
    /// </summary>
    public abstract class SOBlockLoadConfigBase:ScriptableObjectEditorBase
    {
        //推荐优化方式,使用队列去限制每帧触发时最多加载与卸载的次数。
        internal Action<Vector3Int> _OnLoadBlockChangeEvent;
        internal Action<Vector3Int> _OnUninstallBlockChangeEvent;

        public void OnLoadBlockChangeEvent(Action<Vector3Int> onLoadBlockChangeEvent)
        {
            _OnLoadBlockChangeEvent += onLoadBlockChangeEvent;
        }

        public void OnUninstallBlockChangeEvent(Action<Vector3Int> onUninstallBlockChangeEvent)
        {
            _OnUninstallBlockChangeEvent += onUninstallBlockChangeEvent;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="position">初始位置</param>
        public abstract void Init(Vector3 position =default);
        /// <summary>
        /// 单个区块的大小
        /// </summary>
        /// <param name="size"></param>
        public abstract void SetSingleBlockSize(int size);
        /// <summary>
        /// 设置显示区块的范围
        /// </summary>
        /// <param name="size"></param>
        public abstract void SetLookBlockSize(Vector3Int size);
        /// <summary>
        /// 正在移动时执行的方法
        /// </summary>
        /// <param name="position">当前的位置</param>
        public abstract void OnMovePosition(Vector3 position);

    }
}
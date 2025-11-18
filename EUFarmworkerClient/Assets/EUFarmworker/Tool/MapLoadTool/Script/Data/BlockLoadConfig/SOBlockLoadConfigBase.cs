using System;
using UnityEngine;

namespace EUFarmworker.Tool.MapLoadTool.Script.Data.BlockLoadConfig
{
    /// <summary>
    /// 主要作用是描述区块生成算法
    /// </summary>
    public abstract class SOBlockLoadConfigBase:ScriptableObjectEditorBase, IDisposable
    {
        //推荐优化方式,使用队列去限制每帧触发时最多加载与卸载的次数。
        internal Action<Vector3Int> _OnLoadBlockChangeEvent;
        internal Action<Vector3Int> _OnUninstallBlockChangeEvent;
        /// <summary>
        /// 注册区块加载改变事件;注意,要使用具体方法而非匿名方法否则无法注销
        /// </summary>
        /// <param name="onLoadBlockChangeEvent"></param>
        public void RegisterLoadBlockChangeEvent(Action<Vector3Int> onLoadBlockChangeEvent)
        {
            _OnLoadBlockChangeEvent += onLoadBlockChangeEvent;
        }
        /// <summary>
        /// 注册区块卸载改变事件;注意,要使用具体方法而非匿名方法否则无法注销
        /// </summary>
        /// <param name="onLoadBlockChangeEvent"></param>
        public void RegisterUninstallBlockChangeEvent(Action<Vector3Int> onUninstallBlockChangeEvent)
        {
            _OnUninstallBlockChangeEvent += onUninstallBlockChangeEvent;
        }
        /// <summary>
        /// 注销区块加载改变事件;注意,要使用具体方法而非匿名方法否则无法注销
        /// </summary>
        /// <param name="onLoadBlockChangeEvent"></param>
        public void UnRegisterLoadBlockChangeEvent(Action<Vector3Int> onLoadBlockChangeEvent)
        {
            _OnLoadBlockChangeEvent -= onLoadBlockChangeEvent;
        }
        /// <summary>
        /// 注销区块卸载改变事件;注意,要使用具体方法而非匿名方法否则无法注销
        /// </summary>
        /// <param name="onLoadBlockChangeEvent"></param>
        public void UnRegisterUninstallBlockChangeEvent(Action<Vector3Int> onUninstallBlockChangeEvent)
        {
            _OnUninstallBlockChangeEvent -= onUninstallBlockChangeEvent;
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
        /// 获取当前单个区块的大小
        /// </summary>
        /// <returns></returns>
        public abstract int GetSingleBlockSize();
        /// <summary>
        /// 设置显示区块的范围
        /// </summary>
        /// <param name="size"></param>
        public abstract void SetLookBlockSize(Vector3Int size);
        /// <summary>
        /// 区块显示范围的信息
        /// </summary>
        /// <returns></returns>
        public abstract Vector3Int GetLookBlockSize();
        /// <summary>
        /// 正在移动时执行的方法
        /// </summary>
        /// <param name="position">当前的位置</param>
        public abstract void OnMovePosition(Vector3 position);

        public abstract void Dispose();
    }
}
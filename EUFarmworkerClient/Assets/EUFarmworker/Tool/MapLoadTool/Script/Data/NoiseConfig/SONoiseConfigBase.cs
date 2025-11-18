using System;
using UnityEngine;

namespace EUFarmworker.Tool.MapLoadTool.Script.Data.NoiseConfig
{
    /// <summary>
    /// 主要作用是描述噪声生成算法
    /// </summary>
    public abstract class SONoiseConfigBase:ScriptableObjectEditorBase, IDisposable
    {
        internal int Send;
        /// <summary>
        /// 获取当前种子信息
        /// </summary>
        /// <returns></returns>
        public abstract int GetSend();
        /// <summary>
        /// 设置种子
        /// </summary>
        /// <param name="value"></param>
        public abstract void SetSend(int value);
        /// <summary>
        /// 生成噪声标量。
        /// </summary>
        /// <param name="position">提供生成位置的信息</param>
        /// <returns></returns>
        public abstract float OnUse(Vector3 position);
        public abstract void Dispose();
    }
}
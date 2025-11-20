using System;
using System.Collections.Generic;
using EUFarmworker.Tool.DamageNumTool.Script.Data;
using EUFarmworker.Tool.DamageNumTool.Script.Generate;
using Unity.Collections;
using UnityEngine;
using UnityEngine.VFX;

namespace EUFarmworker.Tool.DamageNumTool.Script
{
    public class DamageNumToolRunTimeMono : MonoBehaviour
    {
        [SerializeField] SODamageNumViewConfig config;
        public SODamageNumViewConfig Config => config;
        [SerializeField] private VisualEffect vfx;
        GraphicsBuffer _damageNumBuffer;
        GraphicsBuffer _colorBuffer;
        private NativeArray<Vector4> _damgeNums;
        private NativeQueue<Vector4> _queue;
        private void Start()
        {
            vfx.SetFloat("Bounds",config.ConfigData.bounds);
            vfx.SetFloat("Life",config.ConfigData.life);
            vfx.SetTexture("MainTexture",config.ConfigData.texture);
            vfx.SetAnimationCurve("Curve",config.ConfigData.alphaCurve);
            vfx.SetAnimationCurve("PosXCurve",Config.ConfigData.posXCurve);
            vfx.SetAnimationCurve("PosYCurve",Config.ConfigData.posYCurve);
            vfx.SetAnimationCurve("ScaleCurve",Config.ConfigData.scaleCurve);
            _damgeNums = new(config.ConfigData.numCount, Allocator.Persistent);
            _queue = new(Allocator.Persistent);
            _damageNumBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, config.ConfigData.numCount, 16);
            _colorBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, config.ConfigData.colors.Count, 16);
            _colorBuffer.SetData(config.ConfigData.colors);
            vfx.SetGraphicsBuffer("DamageData", _damageNumBuffer);
            vfx.SetGraphicsBuffer("ColorData", _colorBuffer);
        }
        private readonly int _countID = Shader.PropertyToID("Count");
        private int _count = 0;
        private void Update()
        {
            if (_queue.Count == 0) return;
            _count = 0;
            while (_queue.Count > 0 && _count < config.ConfigData.numCount)
            {
                _damgeNums[_count] = _queue.Dequeue();
                _count++;
            }
            _damageNumBuffer.SetData(_damgeNums);
            vfx.SetInt(_countID, _count);
            vfx.Play();
        }

        /// <summary>
        /// 添加飘字
        /// </summary>
        /// <param name="position">飘字位置</param>
        /// <param name="damage">飘字数字</param>
        /// <param name="color">飘字颜色</param>
        public void AddDamageNum(Vector2 position, float damage, DamageNumColor color = DamageNumColor.Red)
        {
            _queue.Enqueue(new Vector4(position.x, position.y, damage, (float)color));
        }
        

        private void OnDestroy()
        {
            _damageNumBuffer?.Release();
            _colorBuffer?.Release();
            if(_damgeNums.IsCreated) _damgeNums.Dispose();
            if(_queue.IsCreated) _queue.Dispose();
            DamageNumTool.Dispose();
        }
    }
}
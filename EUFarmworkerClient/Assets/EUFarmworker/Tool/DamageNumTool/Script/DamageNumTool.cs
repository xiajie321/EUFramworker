using System.Collections.Generic;
using EUFarmworker.Tool.DamageNumTool.Script.Data;
using EUFarmworker.Tool.DamageNumTool.Script.Generate;
using UnityEngine;

namespace EUFarmworker.Tool.DamageNumTool.Script
{
    public static class DamageNumTool
    {
        private static GameObject root;
        private static SODamageNumViewConfig _damageNumViewConfig;
        private static DamageNumToolRunTimeMono _damageNumToolRunTimeMono;
        /// <summary>
        /// 添加飘字
        /// </summary>
        /// <param name="position">飘字位置</param>
        /// <param name="damage">飘字数字</param>
        /// <param name="color">飘字颜色</param>
        public static void AddDamageNum(Vector2 position, float damage, DamageNumColor color = DamageNumColor.Red)
        {
            if (!root)
            {
                Debug.LogWarning("[DamageNumTool] 没有初始化DamageNumTool工具");
                return;
            }
            _damageNumToolRunTimeMono.AddDamageNum(position,damage,color);
        }

        public static void Init()
        {
            if (root)
            {
                Object.DestroyImmediate(root);
            }
            root = Object.Instantiate(Resources.Load<GameObject>("EUFarmworker/DamageNumTool/DamageNumTool"));
            _damageNumToolRunTimeMono = root.GetComponent<DamageNumToolRunTimeMono>();
            _damageNumViewConfig = _damageNumToolRunTimeMono.Config;
            _damageNumViewConfig.ConfigData.Init();
        }


        public static void Dispose()
        {
            _damageNumViewConfig.ConfigData?.Dispose();
        }
    }
}
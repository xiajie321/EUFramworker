using System;
using System.Collections.Generic;
using System.IO;
using PrimeTween;
using UnityEngine;
using UnityEngine.VFX;

namespace EUFarmworker.Tool.DamageNumTool.Script.Data.DamageNumConfig
{
    [CreateAssetMenu(fileName = "DamageNumConfigData", menuName = "EUTool/DamageNum/DamageNumConfigData")]
    [Serializable]
    public class SODamageNumScriptableObject:ScriptableObjectEditorBase,IDisposable
    {
        #if UNITY_EDITOR
        public string ScriptPath;
        public List<DamageGrData> Names = new()
        {
            new()
            {
                Color = Color.red,
                Name = "Red",
            }
        };
        #endif
        public int numCount = 1000;//每帧最大显示数量
        public float life = 1.5f;
        public float bounds = 64;
        public List<Color> colors = new()
        {
            Color.red,
            Color.green,
        }; //飘字颜色
        public Texture2DArray texture;//纹理
        public AnimationCurve alphaCurve;//动画
        public AnimationCurve posXCurve;
        public AnimationCurve posYCurve;
        public AnimationCurve scaleCurve;

        public void Init()
        {
            
        }
        public void Dispose()
        {
            
        }
        #if UNITY_EDITOR
        //枚举生成
        [NonSerialized]
        private string _enumName = "DamageNumColor";
        [NonSerialized]
        private string _namespaceName = "EUFarmworker.Tool.DamageNumTool.Script.Generate";
        [NonSerialized]
        private string _fileName = "DamageNumColor.cs";

        public void EnumGenerate()
        {
            if (string.IsNullOrEmpty(ScriptPath))
            {
                Debug.LogError("保存文件夹路径不能为空！");
                return;
            }

            // 确保文件夹存在
            if (!Directory.Exists(ScriptPath))
            {
                Directory.CreateDirectory(ScriptPath);
                Debug.Log("文件夹不存在");
            }
            string filePath = Path.Combine(ScriptPath, _fileName);
            try
            {
                string enumCode = GenerateEnumCode();
                File.WriteAllText(filePath, enumCode);
                Debug.Log($"枚举文件生成成功！路径: {filePath}");
            
                // 刷新Unity资源数据库
                UnityEditor.AssetDatabase.Refresh();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成枚举文件失败: {e.Message}");
            }
        }

        private string GenerateEnumCode()
        {
            // 生成枚举值部分
            string enumValuesCode = "";
            for (int i = 0; i < Names.Count; i++)
            {
                string valueName = Names[i].Name;
                // 验证枚举值名称是否合法
                if (string.IsNullOrEmpty(valueName))
                {
                    Debug.LogWarning($"跳过空的枚举值，索引: {i}");
                    continue;
                }
                
                if (string.IsNullOrEmpty(valueName))
                {
                    Debug.LogWarning($"跳过无效的枚举值，索引: {i}");
                    continue;
                }

                // 如果以数字开头，添加下划线前缀
                if (char.IsDigit(valueName[0]))
                {
                    valueName = "_" + valueName;
                }

                enumValuesCode += $"    {valueName}";

                // 最后一个值不加逗号
                if (i < Names.Count - 1)
                {
                    enumValuesCode += ",\n";
                }
            }

            // 构建完整的枚举代码
            string code = $@"// 自动生成的枚举文件

namespace {_namespaceName}
{{
    public enum {_enumName}
    {{
{enumValuesCode}
    }}
}}";

            return code;
        }
        [Serializable]
        public class DamageGrData
        {
            public string Name;
            public Color Color;
        }
        #endif
    }
}
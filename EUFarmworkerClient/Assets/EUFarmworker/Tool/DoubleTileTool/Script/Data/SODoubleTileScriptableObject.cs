using System;
using System.Collections.Generic;
using System.IO;
using EUFarmworker.Tool.DoubleTileTool.Script.Generate;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace EUFarmworker.Tool.DoubleTileTool.Script.Data
{
    [CreateAssetMenu(fileName = "DoubleTileConfigData", menuName = "EUTool/DoubleTile/DoubleTileConfigData")]
    [Serializable]
    public class SODoubleTileScriptableObject : ScriptableObjectEditorBase
    {
        public string ScriptPath = "Assets/EUFarmworker/Tool/DoubleTileTool/Script/Generate"; //脚本生成的路径
        public string TilePath = "Assets/Resources"; //瓦片生成的路径
        public bool IsRuntimeGenerate = true;//运行时生成
        public float FrameRate = 1;//播放速度
        public int Frame;//帧数量
        public TileObjectType TileObjectType = TileObjectType.Sprite;
        public List<string> TileNames = new(); //瓦片名称设置
        public List<DoubleTileData> TileDatas = new(); //瓦片数据
        internal void Generate()
        {
            EnumGenerate();
            if(IsRuntimeGenerate) return;//运行时执行部分不会进行生成
            TileResourceGenerate();
        }

        internal void TileResourceGenerate()
        {
            DoubleTileToolTileGenerate.Init(this);
            string lsname = "";
            foreach (var i in Enum.GetValues(typeof(TileType)))
            {
                foreach (var j in Enum.GetValues(typeof(TileType)))
                {
                    foreach (var k in Enum.GetValues(typeof(TileType)))
                    {
                        foreach (var e in Enum.GetValues(typeof(TileType)))
                        {
                            lsname = $"{i}{j}{k}{e}";
                            
                        }
                    }
                }
            }
        }
        #region 枚举生成
        private string _enumName = "TileType";
        private string _namespaceName = "EUFarmworker.Tool.DoubleTileTool.Script.Generate";
        private string _fileName = "TileType.cs";

        private void EnumGenerate()
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
            for (int i = 0; i < TileNames.Count; i++)
            {
                string valueName = TileNames[i];
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
                if (i < TileNames.Count - 1)
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
        #endregion

    }
}
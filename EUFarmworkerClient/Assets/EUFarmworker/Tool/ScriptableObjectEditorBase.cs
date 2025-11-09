using UnityEditor;
using UnityEngine;

namespace EUFarmworker.Tool
{
    public class ScriptableObjectEditorBase:ScriptableObject
    {
        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
#endif
        }
    }
}
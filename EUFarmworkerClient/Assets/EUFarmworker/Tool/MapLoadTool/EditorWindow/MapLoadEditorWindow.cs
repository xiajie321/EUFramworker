using System;
using EUFarmworker.Tool.MapLoadTool.Script.Data;
using EUFarmworker.Tool.MapLoadTool.Script.Data.MapLoadConfig;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class MapLoadEditorWindow : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [SerializeField] private SOMapLoadViewConfig ViewConfig;

    [MenuItem("EUTool/MapLoadTool")]
    public static void ShowExample()
    {
        MapLoadEditorWindow wnd = GetWindow<MapLoadEditorWindow>();
        wnd.minSize = new Vector2(1080, 720);
        wnd.maxSize = new Vector2(1080, 720);
        wnd.titleContent = new GUIContent("MapLoadTool");
    }

    #region 字段
    private ObjectField _viewConfig;
    private ObjectField _blockLoadConfig;
    private ObjectField _mapGenerateConfig;
    private ObjectField _noiseConfig;

    private VisualElement _blockLoadView;
    private VisualElement _mapGenerateView;
    private VisualElement _noiseView;
    #endregion
    private void GetUI(VisualElement root)
    {
        _viewConfig = root.Q<ObjectField>("ViewConfig");
        _blockLoadConfig = root.Q<ObjectField>("BlockLoadConfig");
        _mapGenerateConfig = root.Q<ObjectField>("MapGenerateConfig");
        _noiseConfig = root.Q<ObjectField>("NoiseConfig");
        
        _blockLoadView = root.Q<VisualElement>("BlockLoadView");
        _mapGenerateView = root.Q<VisualElement>("MapGenerateView");
        _noiseView = root.Q<VisualElement>("NoiseView");
    }
    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        m_VisualTreeAsset.CloneTree(root);
        GetUI(root);
        Config();
    }

    #region 主配置更改
    private void Config()
    {
        _viewConfig.objectType =  typeof(SOMapLoadScriptableObject);
        _viewConfig.value =  ViewConfig.ConfigData;
        _viewConfig.RegisterValueChangedCallback(v =>
        {
            ViewConfig.ConfigData = v.newValue as SOMapLoadScriptableObject;
            ConfigChange();
            ViewConfig.Save();
        });
    }
    private void ConfigChange()//主配置改变时执行的方法
    {
        if (ViewConfig.ConfigData)
        {
            _blockLoadView.style.display = DisplayStyle.Flex;
            _mapGenerateView.style.display = DisplayStyle.Flex;
            _noiseConfig.style.display = DisplayStyle.Flex;
            return;
        }
        _blockLoadView.style.display = DisplayStyle.None;
        _mapGenerateView.style.display = DisplayStyle.None;
        _noiseConfig.style.display = DisplayStyle.None;
        ViewConfig.Save();
    }

    #endregion




    private void OnDisable()
    {
        ViewConfig.ConfigData.Save();
        ViewConfig.Save();
    }
}

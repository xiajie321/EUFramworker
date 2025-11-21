using System.Collections.Generic;
using EUFarmworker.Tool.DamageNumTool.EditorWindow;
using EUFarmworker.Tool.DamageNumTool.Script;
using EUFarmworker.Tool.DamageNumTool.Script.Data;
using EUFarmworker.Tool.DamageNumTool.Script.Data.DamageNumConfig;
using EUFarmworker.Tool.MapLoadTool.Script.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DamageNumToolEditor : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    [SerializeField]
    SODamageNumViewConfig ViewConfig;

    [MenuItem("EUTool/DamageNumTool")]
    public static void ShowExample()
    {
        DamageNumToolEditor wnd = GetWindow<DamageNumToolEditor>();
        wnd.minSize = new Vector2(1080, 720);
        wnd.maxSize = new Vector2(1080, 720);
        wnd.titleContent = new GUIContent("DamageNumTool");
    }

    #region 字段

    private ObjectField _viewConfig;
    private TextField _scriptPath;
    private Button _scriptPathButton;
    private UnsignedIntegerField _maxSum;
    private FloatField _life;
    private CurveField _alpha;
    private CurveField _x;
    private CurveField _y;
    private CurveField _scale;
    private ObjectField _fontArrayTex;
    private Foldout _tileTypes;
    private Button _addTypeButton;
    
    private VisualElement _scriptPathShow;
    private VisualElement _set2Show;
    private VisualElement _set3Show;
    private VisualElement _set4Show;
    private VisualElement _typesShow;

    #endregion
    private void GetUI(VisualElement root)
    {
        _viewConfig = root.Q<ObjectField>("ViewConfig");
        _scriptPath = root.Q<TextField>("ScriptPath");
        _scriptPathButton = root.Q<Button>("ScriptPathButton");
        _maxSum = root.Q<UnsignedIntegerField>("MaxSum");
        _life = root.Q<FloatField>("Life");
        _alpha = root.Q<CurveField>("Alpha");
        _x = root.Q<CurveField>("X");
        _y = root.Q<CurveField>("Y");
        _scale = root.Q<CurveField>("Scale");
        _fontArrayTex = root.Q<ObjectField>("FontArrayTex");
        _tileTypes = root.Q<Foldout>("TileTypes");
        _addTypeButton = root.Q<Button>("AddTypeButton");
        
        _scriptPathShow = root.Q<VisualElement>("ScriptPathShow");
        _set2Show = root.Q<VisualElement>("Set2Show");
        _set3Show = root.Q<VisualElement>("Set3Show");
        _set4Show = root.Q<VisualElement>("Set4Show");
        _typesShow = root.Q<VisualElement>("TypesShow");
    }
    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        m_VisualTreeAsset.CloneTree(root);
        GetUI(root);
        Register();
        Init();
    }

    private void Register()
    {
        _viewConfig.RegisterValueChangedCallback(v =>
        {
            if(v.newValue == ViewConfig.ConfigData || v.newValue == v.previousValue) return;
            ViewConfig.ConfigData = v.newValue as SODamageNumScriptableObject;
            ViewChangeShow(ViewConfig.ConfigData);
            Save();
        });
        _fontArrayTex.RegisterValueChangedCallback(v =>
        {
            if(v.newValue == ViewConfig.ConfigData.texture || v.newValue == v.previousValue) return;
            ViewConfig.ConfigData.texture = v.newValue as Texture2DArray;
            Save();
        });
        _scriptPath.RegisterValueChangedCallback(v =>
        {
            if(v.newValue == ViewConfig.ConfigData.ScriptPath || v.newValue == v.previousValue) return;
            ViewConfig.ConfigData.ScriptPath = v.newValue;
            Save();
        });
        _scriptPathButton.clickable.clicked += () =>
        {
            string path = EditorUtility.OpenFolderPanel("选择文件夹", "Assets", "");
            string projectPath = Application.dataPath;
            if (path.StartsWith(projectPath))
            {
                path = "Assets" + path.Substring(projectPath.Length);
            }

            if (path.Equals("")) return;
            _scriptPath.value = path;
        };
        _alpha.RegisterValueChangedCallback(v =>
        {
            if(v.newValue.Equals(ViewConfig.ConfigData.alphaCurve) || v.newValue.Equals(v.previousValue)) return;
            ViewConfig.ConfigData.alphaCurve = v.newValue;
            Save();
        });
        _addTypeButton.clickable.clicked += () =>
        {
            ViewConfig.ConfigData.Names.Add(AddItem(items.Count));
        };
    }
    private void Init()
    {
        _viewConfig.objectType = typeof(SODamageNumScriptableObject);
        _fontArrayTex.objectType = typeof(Texture2DArray);
        Assignment();
    }
    List<DamageNumColorItem> items = new List<DamageNumColorItem>();
    private void Assignment()
    {
        _viewConfig.value = ViewConfig.ConfigData;
        if(!_viewConfig.value) return;
        _scriptPath.value = ViewConfig.ConfigData.ScriptPath;
        _fontArrayTex.value = ViewConfig.ConfigData.texture;
        _alpha.value = ViewConfig.ConfigData.alphaCurve;
        _x.value = ViewConfig.ConfigData.posXCurve;
        _y.value = ViewConfig.ConfigData.posYCurve;
        _scale.value = ViewConfig.ConfigData.scaleCurve;
        _maxSum.value = (uint)ViewConfig.ConfigData.numCount;
        _life.value = ViewConfig.ConfigData.life;
        for (int i = 0; i < ViewConfig.ConfigData.Names.Count; i++)
        {
            AddItem(i,ViewConfig.ConfigData.Names[i]);
        }
    }

    private SODamageNumScriptableObject.DamageGrData AddItem(int index,SODamageNumScriptableObject.DamageGrData data =  null)
    {
        items.Add(new DamageNumColorItem());
        items[index].Index = index;
        items[index].Name = "颜色a" + index;
        items[index].Color = Color.white;
        SODamageNumScriptableObject.DamageGrData ls = null;
        if (data == null)
        {
            ls = new SODamageNumScriptableObject.DamageGrData();
            ls.Color = Color.white;
            ls.Name = items[index].Name;
        }

        if (data != null)
        {
            items[index].Color = data.Color;
            items[index].Name = data.Name;
        }
        var ls2 = items[index];
        int lsindex = index;
        ls2.RegisterValueChangedCallback(v =>
        {
            if(v.newValue.Equals(ViewConfig.ConfigData.Names[ls2.Index].Name) || v.newValue.Equals(v.previousValue)) return;
            if (ViewConfig.ConfigData.Names.Find(va => va.Name == v.newValue) != null)
            {
                ls2.style.color = Color.red;
                return;
            }
            ViewConfig.ConfigData.Names[ls2.Index].Name = v.newValue;
            ls2.style.color = Color.white;
        });
        ls2.RegisterColorChangedCallback(v =>
        {
            if(v.newValue.Equals(ViewConfig.ConfigData.Names[ls2.Index].Color) || v.newValue.Equals(v.previousValue)) return;
            ViewConfig.ConfigData.Names[ls2.Index].Color = v.newValue;
        });
        ls2.AddButtonClick(() =>
        {
            RemoveItem(ls2.Index);
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Index = i;
            }
            
        });
        _tileTypes.Add(ls2);
        return ls;
    }

    private void RemoveItem(int index)
    { 
        Debug.Log(index);
        _tileTypes.Remove(items[index]);
        items.RemoveAt(index);
        ViewConfig.ConfigData.Names.RemoveAt(index);
    }

    private void ViewChangeShow(bool show)
    {
        _set2Show.visible = show;
        _set3Show.visible = show;
        _set4Show.visible = show;
        _typesShow.visible = show;
    }
    private void OnDisable()
    {
        Save();
    }

    private void Save()
    {
        ViewConfig.Save();
        if(!ViewConfig.ConfigData) return;
        var ls = new List<Color>();
        foreach (var i in ViewConfig.ConfigData.Names)
        {
            ls.Add(i.Color);
        }
        ViewConfig.ConfigData.colors = ls;
        ViewConfig.ConfigData.Save();
        ViewConfig.ConfigData.EnumGenerate();
    }
}

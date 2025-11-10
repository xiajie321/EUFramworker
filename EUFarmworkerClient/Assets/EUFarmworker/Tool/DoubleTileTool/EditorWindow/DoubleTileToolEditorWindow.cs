using System;
using System.Collections.Generic;
using System.Linq;
using EUFarmworker.Tool.DoubleTileTool.Script.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace EUFarmworker.Tool.DoubleTileTool.EditorWindow
{
    public class DoubleTileToolEditorWindow : UnityEditor.EditorWindow
    {
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

        [SerializeField] private SODoubleTileViewConfig ViewConfig;

        private string[] _selectNames =
        {
            "基础设置",
            "瓦片设置",
        };

        [MenuItem("EUTool/DoubleTileTool")]
        public static void ShowExample()
        {
            DoubleTileToolEditorWindow wnd = GetWindow<DoubleTileToolEditorWindow>();
            wnd.minSize = new Vector2(1080, 720);
            wnd.maxSize = new Vector2(1080, 720);
            wnd.titleContent = new GUIContent("DoubleTileTool");
        }

        #region Setting面板字段

        private ObjectField _viewConfig;
        private ListView _selectionListView;
        private Button _tilePathButton;
        private Button _scriptPathButton;
        private TextField _scriptPath;
        private TextField _tilePath;
        private Foldout _tileTypes;
        private Button _addTileTypeButton;
        private Toggle _isRuntimeGenerate;
        private IntegerField _frame;
        private FloatField _frameRate;
        private DropdownField _tileObjectType;

        #endregion

        #region TileSetting面板字段

        private Button _generatorButton;
        private ScrollView _tileSettingScroll;

        #endregion

        #region 配置数据文件为空时影响显示的组件

        private VisualElement _scriptPathShow;
        private VisualElement _tilePathShow;
        private VisualElement _tileTypesShow;
        private VisualElement _togglesShow;
        #endregion

        private void GetUI(VisualElement root)
        {
            #region Setting面板

            _selectionListView = root.Q<ListView>("Left");
            _viewConfig = root.Q<ObjectField>("ViewConfig");
            _tilePathButton = root.Q<Button>("TilePathButton");
            _scriptPathButton = root.Q<Button>("ScriptPathButton");
            _scriptPath = root.Q<TextField>("ScriptPath");
            _tilePath = root.Q<TextField>("TilePath");
            _tileTypes = root.Q<Foldout>("TileTypes");
            _addTileTypeButton = root.Q<Button>("AddTileTypeButton");
            _isRuntimeGenerate = root.Q<Toggle>("IsRuntimeGenerate");
            _frame = root.Q<IntegerField>("Frame");
            _frameRate = root.Q<FloatField>("FrameRate");
            _tileObjectType = root.Q<DropdownField>("TileObjectType");

            _scriptPathShow = root.Q<VisualElement>("ScriptPathShow");
            _tilePathShow = root.Q<VisualElement>("TilePathShow");
            _tileTypesShow = root.Q<VisualElement>("TileTypesShow");
            _togglesShow = root.Q<VisualElement>("TogglesShow");
            #endregion

            #region TileSetting面板

            _generatorButton = root.Q<Button>("GeneratorButton");
            _tileSettingScroll = root.Q<ScrollView>("TileSettingScroll");

            #endregion
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement; //获取当前的视觉元素
            m_VisualTreeAsset.CloneTree(root); //复制到视觉树
            GetUI(root);
            //Setting面板

            #region 选择菜单列表

            _selectionListView.itemsSource = _selectNames; //组件选项的信息
            _selectionListView.fixedItemHeight = 50; //选项组件高度
            _selectionListView.makeItem = SelectionListItem; //生成的组件绑定
            _selectionListView.bindItem = SelectionBindItem; //绑定信息
            _selectionListView.selectedIndex = 0; //默认选中的选项
            _selectionListView.selectedIndicesChanged += selectedIndexChanged; //绑定选择事件

            #endregion

            #region 配置文件

            _viewConfig.objectType = typeof(SODoubleTileScriptableObject);
            _viewConfig.value = ViewConfig.ConfigData;
            _viewConfig.RegisterValueChangedCallback(ConfigChange);
            ConfigChangeShowVisual(ViewConfig.ConfigData);

            #endregion

            #region 路径

            //脚本路径
            _scriptPath.value = ViewConfig?.ConfigData?.ScriptPath ?? "";
            _scriptPath.RegisterValueChangedCallback(ScriptPathChange);
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
            //瓦片路径
            _tilePath.value = ViewConfig?.ConfigData?.TilePath ?? "";
            _tilePath.RegisterValueChangedCallback(TilePathChange);
            _tilePathButton.clickable.clicked += () =>
            {
                string path = EditorUtility.OpenFolderPanel("选择文件夹", "Assets", "");
                string projectPath = Application.dataPath;
                if (path.StartsWith(projectPath))
                {
                    path = "Assets" + path.Substring(projectPath.Length);
                }
                if (path.Equals("")) return;
                _tilePath.value = path;
            };

            #endregion

            #region 单选选项
            _isRuntimeGenerate.value = (bool)ViewConfig?.ConfigData?.IsRuntimeGenerate;
            _isRuntimeGenerate.RegisterValueChangedCallback(v =>
            {
                ViewConfig.ConfigData.IsRuntimeGenerate = v.newValue;
                Save();
            });
            //类型设置
            foreach (var v in  Enum.GetValues(typeof(TileObjectType)))
            {
                _tileObjectType.choices.Add(v.ToString());
            }
            _tileObjectType.index = (int)ViewConfig.ConfigData.TileObjectType;
            _tileObjectType.RegisterValueChangedCallback(v =>
            {
                TileObjectType lsType = (TileObjectType)Enum.Parse(typeof(TileObjectType), v.newValue);
                foreach (var i in _settingItems)
                {
                    i.SetTileObjectType(lsType);
                }

                ViewConfig.ConfigData.TileObjectType = lsType;
                Save();
            });
            //帧
            _frame.value = ViewConfig.ConfigData.Frame;
            _frame.RegisterValueChangedCallback(v =>
            {
                ViewConfig.ConfigData.Frame = v.newValue;
                foreach (var i in ViewConfig.ConfigData.TileDatas)
                {
                    if(!i.IsDynamic) break;
                    if (i.ObjectList.Count > v.newValue)
                    {
                        int lsIndex = i.ObjectList.Count - v.newValue;
                        for (int j = 0; j < lsIndex; j++)
                        {
                            if(i.ObjectList.Count <= 0) break;
                            i.ObjectList.RemoveAt(i.ObjectList.Count - 1);
                        }
                    }
                    else if(i.ObjectList.Count < v.newValue)
                    {
                        int lsIndex = v.newValue - i.ObjectList.Count;
                        for (int j = 0; j < lsIndex; j++)
                        {
                            i.ObjectList.Add(new());
                        }
                    }
                }
            });
            //帧速率
            _frameRate.value = ViewConfig.ConfigData.FrameRate;
            _frameRate.RegisterValueChangedCallback(v =>
            {
                ViewConfig.ConfigData.FrameRate = v.newValue;
            });
            #endregion

            #region 类型配置列表

            _addTileTypeButton.clickable.clicked += () => //添加类型按钮
            {
                if (!ViewConfig.ConfigData) return;
                var ls = new TileTypesItem();
                var lsindex = ViewConfig.ConfigData.TileNames;
                ls.Index = lsindex.Count;
                string lsName = $"瓦片{ls.Index.ToString()}";
                int lsIndex = 0;
                while (ViewConfig.ConfigData.TileDatas.Any(v => v.TileName == lsName) )
                {
                    if (!ViewConfig.ConfigData) return;
                    lsName = $"瓦片{lsIndex}";
                    lsIndex++;
                }
                lsindex.Add(lsName);
                ls.Name = lsName;
                RegisterValueChange(ls);
                RemoveTileTypeButtonEvent(ls);
                var lsData = new DoubleTileData();
                lsData.TileName = ls.Name;
                ViewConfig.ConfigData.TileDatas.Add(lsData);
                lsData.ObjectList.Add(new());
                _typesItems.Add(ls);
                _tileTypes.Add(ls);
                Save();
            };
            TypeConfigInit();

            #endregion

            #region 瓦片配置

            _generatorButton.clickable.clicked += () =>
            {
                ViewConfig.ConfigData.Generate();
            };
            TileConfigInit();
            #endregion

            //TileSetting面板
        }


        #region 选择菜单列表具体逻辑

        private void selectedIndexChanged(IEnumerable<int> selectedIndices) //选项方法
        {
            foreach (var index in selectedIndices)
            {
                if (index == 0)
                {
                    Setting();
                }
                else if (index == 1)
                {
                    TileSetting();
                }
            }
        }

        private void Setting() //选项逻辑
        {
            rootVisualElement.Q<VisualElement>("TileSetting").style.display = new StyleEnum<DisplayStyle>()
            {
                value = DisplayStyle.None,
            };
            rootVisualElement.Q<VisualElement>("Setting").style.display = new StyleEnum<DisplayStyle>()
            {
                value = DisplayStyle.Flex,
            };
        }

        private void TileSetting() //选项逻辑
        {
            rootVisualElement.Q<VisualElement>("TileSetting").style.display = new StyleEnum<DisplayStyle>()
            {
                value = DisplayStyle.Flex,
            };
            rootVisualElement.Q<VisualElement>("Setting").style.display = new StyleEnum<DisplayStyle>()
            {
                value = DisplayStyle.None,
            };
            TileConfigInit();
        }

        private void SelectionBindItem(VisualElement arg1, int arg2) //选择的组件信息
        {
            var item = arg1 as Label;
            item.text = _selectNames[arg2];
        }

        private VisualElement SelectionListItem() //选择的组件
        {
            var item = new Label();
            item.style.unityTextAlign = TextAnchor.MiddleCenter;
            return item;
        }

        #endregion

        #region 配置文件改变事件

        private void ConfigChange(ChangeEvent<Object> evt) //主文件
        {
            var ls = evt.newValue as SODoubleTileScriptableObject;
            ViewConfig.ConfigData = ls;
            TypeConfigInit();
            if (!ViewConfig.ConfigData)
            {
                ConfigChangeShowVisual(false);
                return;
            }

            ConfigChangeShowVisual(true);
            _scriptPath.value = ViewConfig.ConfigData.ScriptPath;
            _tilePath.value = ViewConfig.ConfigData.TilePath;
            _frame.value = ViewConfig.ConfigData.Frame;
            _frameRate.value = ViewConfig.ConfigData.FrameRate;
            _tileObjectType.index = (int)ViewConfig.ConfigData.TileObjectType;
            _isRuntimeGenerate.value = ViewConfig.ConfigData.IsRuntimeGenerate;
            Save();
        }

        private void ConfigChangeShowVisual(bool show)
        {
            _scriptPathShow.style.display = new StyleEnum<DisplayStyle>()
                { value = show ? DisplayStyle.Flex : DisplayStyle.None };
            _tilePathShow.style.display = new StyleEnum<DisplayStyle>()
                { value = show ? DisplayStyle.Flex : DisplayStyle.None };
            _tileTypesShow.style.display = new StyleEnum<DisplayStyle>()
                { value = show ? DisplayStyle.Flex : DisplayStyle.None };
            _togglesShow.style.display = new StyleEnum<DisplayStyle>()
                { value = show ? DisplayStyle.Flex : DisplayStyle.None };
        }

        private void ScriptPathChange(ChangeEvent<string> evt)
        {
            if (!ViewConfig.ConfigData) return;
            ViewConfig.ConfigData.ScriptPath = evt.newValue;
            Save();
        }

        private void TilePathChange(ChangeEvent<string> evt)
        {
            if (!ViewConfig.ConfigData) return;
            ViewConfig.ConfigData.TilePath = evt.newValue;
            Save();
        }

        #endregion

        #region 类型配置列表

        private readonly List<TileTypesItem> _typesItems = new();

        private void TypeConfigInit()
        {
            foreach (var type in _typesItems)
            {
                _tileTypes.Remove(type);
            }

            _typesItems.Clear();
            if (!ViewConfig.ConfigData)
            {
                return;
            }

            for (int i = 0; i < ViewConfig.ConfigData.TileNames.Count; i++)
            {
                var ls = new TileTypesItem();
                ls.Index = i;
                ls.Name = ViewConfig.ConfigData.TileNames[i];
                RegisterValueChange(ls);
                RemoveTileTypeButtonEvent(ls);
                _tileTypes.Add(ls);
                _typesItems.Add(ls);
            }
            Save();
        }

        private void RegisterValueChange(TileTypesItem ls)
        {
            ls.RegisterValueChangedCallback(v =>
            {
                var ls2 = ls;
                var Name = ViewConfig.ConfigData.TileNames[ls2.Index];
                if (!ViewConfig.ConfigData) return;
                if (ViewConfig.ConfigData.TileDatas.Any(a => a.TileName == v.newValue) && Name != v.newValue)
                {
                    ls2.style.backgroundColor = Color.red;
                    return;
                }

                ls2.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0));
                var lsData = ViewConfig.ConfigData.TileDatas.Find(v=>v.TileName == Name);
                lsData.TileName = v.newValue;
                ViewConfig.ConfigData.TileNames[ls2.Index] = v.newValue;
                Save();
            });
        }

        private void RemoveTileTypeButtonEvent(TileTypesItem ls) //删除类型按钮
        {
            ls.AddButtonClick(() =>
            {
                int index = ls.Index;
                var ls2 = ViewConfig.ConfigData.TileNames;
                var ls3 = _typesItems;
                for (int j = index; j < ls2.Count; j++)
                {
                    ls3[j].Index = j - 1;
                }

                ls3.RemoveAt(index);
                ViewConfig.ConfigData.TileDatas.Remove(ViewConfig.ConfigData.TileDatas.Find(v => v.TileName == ls2[index]));
                ls2.RemoveAt(index);
                _tileTypes.RemoveAt(index);
                Save();
            });
        }

        #endregion

        #region 瓦片配置
        private readonly List<TileSettingItem> _settingItems = new();
        private void TileConfigInit()
        {
            foreach (var i in  _settingItems)
            {
                _tileSettingScroll.Remove(i);
            }
            _settingItems.Clear();
            if(!ViewConfig.ConfigData) return;
            var lsData = ViewConfig.ConfigData.TileDatas;
            foreach (var i in lsData)
            {
                TileConfigRegisterValueChangeInit(CreateTileSettingItem(i));
            }
            Save();
        }

        private void TileConfigRegisterValueChangeInit(TileSettingItem ls)
        {
            ls.ObjectField.RegisterValueChangedCallback(v =>//占位图标
            {
                ViewConfig.ConfigData.TileDatas[ls.Index].TagTexture = v.newValue as Sprite;
                Save();
            });
            ls.OnTileObjectTypeChange += v =>//瓦片类型切换
            {
                ViewConfig.ConfigData.TileDatas[ls.Index].TileObjectType = v;
                for (int i=0;i<ViewConfig.ConfigData.TileDatas[ls.Index].ObjectList.Count;i++)
                {
                    for (int j = 0; j < ViewConfig.ConfigData.TileDatas[ls.Index].ObjectList[i].Objects.Count; j++)
                    {
                        ViewConfig.ConfigData.TileDatas[ls.Index].ObjectList[i].Objects[j] = null;
                    }
                }
                Save();
            };
        }
        private TileSettingItem CreateTileSettingItem(DoubleTileData data)
        {
            var ls = new TileSettingItem();
            ls.ObjectField.value = data.TagTexture;
            ls.TileTypeName = data.TileName;
            ls.Index = _settingItems.Count;
            ls.ImageBg = data.TagTexture;
            ls.IsDynamic.value = data.IsDynamic;
            ls.IsDynamic.RegisterValueChangedCallback(v =>
            {
                data.IsDynamic = v.newValue;
                if (!v.newValue)
                {
                    int count = ls.Count;
                    for (int i=1;i< count;i++)
                    {
                        ls.Remove(ls.Count - 1);
                    }
                    var bc = data.ObjectList[0];
                    data.ObjectList.Clear();
                    data.ObjectList.Add(bc);
                }
                else
                {
                    for (int i = 1; i<ViewConfig.ConfigData.Frame; i++)
                    {
                        var l = new TileGroupItem();
                        l.Index = i;
                        l.SetTileType(data.TileObjectType);
                        ls.Add(l);
                        data.ObjectList.Add(new());
                    }
                }
                Save();
            });
            int index = 0;
            foreach (var i in data.ObjectList)
            {
                var l = new TileGroupItem();
                l.Index = index;
                l.SetTileType(data.TileObjectType);
                TileGroupItemInitAndRegister(l,i);
                ls.Add(l);
                index++;
            }
            ls.SetTileObjectType(data.TileObjectType);
            _tileSettingScroll.Add(ls);
            _settingItems.Add(ls);
            return ls;
        }
        

        private void TileGroupItemInitAndRegister(TileGroupItem l,DoubleTileDataItem i)
        {
            for (int j = 0; j < i.Objects.Count; j++)
            {
                switch (l.GetTileObjectType())
                {
                    case TileObjectType.Sprite:
                        l.GetObjectField(j).value = i.Objects[j] as  Sprite;
                        l.SetImage(j,i.Objects[j] as Sprite);
                        break;
                    case TileObjectType.GameObject:
                        l.GetObjectField(j).value = i.Objects[j] as GameObject;
                        l.SetImage(j,i.Objects[j] as GameObject);
                        break;
                }

                var j1 = j;
                l.GetObjectField(j).RegisterValueChangedCallback(v =>
                {
                    int lsindex = j1;
                    switch (l.GetTileObjectType())
                    {
                        case TileObjectType.Sprite:
                            i.Objects[lsindex] = v.newValue as Sprite;
                            break;
                        case TileObjectType.GameObject:
                            i.Objects[lsindex] = v.newValue as GameObject;
                            break;
                    }
                    Save();
                });
            }
        }
        #endregion

        private void OnDisable()
        {
            Save();
        }

        private void Save()
        {
            ViewConfig.Save();
            if(!ViewConfig.ConfigData) return;
            ViewConfig.ConfigData.Save();
        }
    }
}
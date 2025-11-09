using System;
using System.Collections.Generic;
using EUFarmworker.Tool.DoubleTileTool.Script.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EUFarmworker.Tool.DoubleTileTool.EditorWindow
{
    public class TileSettingItem : VisualElement
    {
        private TemplateContainer _container;
        public new class UxmlFactory : UxmlFactory<TileSettingItem> {}

        private Label _tileTypeName;
        private DropdownField _type;
        private ObjectField  _objectField;//占位瓦片
        private Foldout _tileGroup;//瓦片组(多个为帧动画)
        private Toggle _isDynamic;
        private List<TileGroupItem> _tileGroupItems = new();
        private TileObjectType _objectType = TileObjectType.Sprite;
        private VisualElement _imageBg;
        public TileSettingItem()
        {
            _container = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/EUFarmworker/Tool/EditorResources/DoubleTileTool/TileSettingItem.uxml").Instantiate();
            hierarchy.Add(_container);
            _tileTypeName = _container.Q<Label>("TileTypeName");
            _type = _container.Q<DropdownField>("Type");
            _objectField = _container.Q<ObjectField>("Tile0");
            _imageBg = _container.Q<VisualElement>("Image0");
            _objectField.objectType = typeof(Sprite);
            _tileGroup = _container.Q<Foldout>("TileGroup");
            _isDynamic = _container.Q<Toggle>("IsDynamic");
            foreach (var v in  Enum.GetValues(typeof(TileObjectType)))
            {
                _type.choices.Add(v.ToString());
            }
            _type.index = 0;
            _type.RegisterValueChangedCallback(v =>
            {
                SetTileObjectType((TileObjectType)Enum.Parse(typeof(TileObjectType),v.newValue));
            });
            
            _objectField.RegisterValueChangedCallback(v =>
            {
                _imageBg.style.backgroundImage = new(v.newValue as Sprite);
            });
        }
        public int Count => _tileGroupItems.Count;

        public Sprite ImageBg
        {
            get => _imageBg.style.backgroundImage.value.sprite;
            set => _imageBg.style.backgroundImage = new StyleBackground(value);
        }

        public Toggle IsDynamic => _isDynamic;

        public TileGroupItem GetTileGroupItem(int index) => _tileGroupItems[index];
        
        public ObjectField ObjectField => _objectField;
        public DropdownField Type => _type;
        public string TileTypeName
        {
            get => _tileTypeName.text;
            set => _tileTypeName.text = value;
        }
        public int Index { get; set; }
        public void Add(TileGroupItem tileGroupItem)
        {
            _tileGroup.Add(tileGroupItem);
            _tileGroupItems.Add(tileGroupItem);
        }

        public void Remove(int index)
        {
            _tileGroup.RemoveAt(index);
            _tileGroupItems.RemoveAt(index);
        }

        public void Remove(TileGroupItem tileGroupItem)
        {
            _tileGroup.Remove(tileGroupItem);
            _tileGroupItems.Remove(tileGroupItem);
        }
        public void SetTileObjectType(TileObjectType tileObjectType)
        {
            if(_objectType == tileObjectType)return;
            _objectType = tileObjectType;
            _type.index = (int)tileObjectType;
            TileObjectTypeChange();
            OnTileObjectTypeChange?.Invoke(_objectType);
        }
        public event Action<TileObjectType> OnTileObjectTypeChange;
        private void TileObjectTypeChange()
        {
            foreach (var i in  _tileGroupItems)
            {
                i.SetTileType(_objectType);
            }
        }
    }
}

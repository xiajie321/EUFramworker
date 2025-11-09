using System;
using EUFarmworker.Tool.DoubleTileTool.Script;
using EUFarmworker.Tool.DoubleTileTool.Script.Data;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace EUFarmworker.Tool.DoubleTileTool.EditorWindow
{
    public class TileGroupItem : VisualElement
    {
        private TemplateContainer _container;
        public new class UxmlFactory : UxmlFactory<TileGroupItem> {}
        private ObjectField[] _objectFields;
        private VisualElement[] _imageBg;
        private TileObjectType _type = TileObjectType.Sprite;
        private Label _label;
        public TileGroupItem()
        {
            _container = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/EUFarmworker/Tool/EditorResources/DoubleTileTool/TileGroupItem.uxml").Instantiate();
            hierarchy.Add(_container);
            _objectFields = new ObjectField[5];
            _imageBg = new VisualElement[5];
            _objectFields[0] = _container.Q<ObjectField>("Tile1");
            _objectFields[1] = _container.Q<ObjectField>("Tile2");
            _objectFields[2] = _container.Q<ObjectField>("Tile3");
            _objectFields[3] = _container.Q<ObjectField>("Tile4");
            _objectFields[4] = _container.Q<ObjectField>("Tile5");
            _imageBg[0] = _container.Q<VisualElement>("Image1");
            _imageBg[1] = _container.Q<VisualElement>("Image2");
            _imageBg[2] = _container.Q<VisualElement>("Image3");
            _imageBg[3] = _container.Q<VisualElement>("Image4");
            _imageBg[4] = _container.Q<VisualElement>("Image5");
            _label = _container.Q<Label>("Index");
            int i = 0;
            foreach (var obj in _objectFields)
            {
                obj.objectType = typeof(Sprite);
                int lsindex = i;
                obj.RegisterValueChangedCallback(v =>
                {
                    if (_type == TileObjectType.Sprite)
                    {
                        _imageBg[lsindex].style.backgroundImage = new StyleBackground(v.newValue as Sprite);
                        return;
                    }
                    _imageBg[lsindex].style.backgroundImage = new StyleBackground(AssetPreview.GetAssetPreview(v.newValue));
                });
                i++;
            }
        }

        public int Count
        {
            get { return _objectFields.Length; }
        }

        private int _index;
        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                _label.text = _index.ToString();
            } 
        }

        public ObjectField GetObjectField(int index) =>_objectFields[index];

        public void SetImage(int index,Sprite sprite)
        {
            _imageBg[index].style.backgroundImage = new StyleBackground(sprite);
        }

        public void SetImage(int index, GameObject go)
        {
            _imageBg[index].style.backgroundImage = new StyleBackground(AssetPreview.GetAssetPreview(go));
        }
        public void SetTileType(TileObjectType type)
        {
            if(_type == type) return;
            _type = type;
            TileTypeObjectChange();
            OnTileTypeObjectChange?.Invoke(type);
        }

        public TileObjectType GetTileObjectType()
        {
            return _type;
        }

        public event Action<TileObjectType> OnTileTypeObjectChange;
        
        private void TileTypeObjectChange()
        {
            switch (_type)
            {
                case TileObjectType.Sprite:
                    foreach (var obj in _objectFields)
                    {
                        obj.objectType = typeof(Sprite);
                    }
                    break;
                case TileObjectType.GameObject:
                    foreach (var obj in _objectFields)
                    {
                        obj.objectType = typeof(GameObject);
                    }
                    break;
            }

            foreach (var i in  _objectFields)
            {
                i.value = null;
            }
        }
    }
}

using System;
using EUFarmworker.Tool.DoubleTileTool.EditorWindow;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EUFarmworker.Tool.DamageNumTool.EditorWindow
{
    public class DamageNumColorItem: VisualElement
    {
        private TemplateContainer _container;
        public new class UxmlFactory : UxmlFactory<DamageNumColorItem> {}
        private Button _button;
        private TextField  _inputField;
        private Label _index;
        private ColorField _colorField;
        public DamageNumColorItem()
        {
            _container = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/EUFarmworker/Tool/EditorResources/DamageNumTool/DamageNumColorsItem.uxml").Instantiate();
            hierarchy.Add(_container);
            _index = _container.Q<Label>("Index");
            _button = _container.Q<Button>("RemoveButton");
            _inputField = _container.Q<TextField>("TileTypeInput");
            _colorField = _container.Q<ColorField>("Color");
        }
        private int index;

        public int Index
        {
            get => index;
            set
            {
                index = value;
                _index.text = index.ToString();
            }
        }

        private string inputName;

        public string Name
        {
            get => inputName;
            set
            {
                inputName = value;
                _inputField.value = inputName;
            }
        }
        Color _color;
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                _colorField.value = value;
            }
        }

        public void RegisterColorChangedCallback(EventCallback<ChangeEvent<Color>> evt)
        {
            _colorField.RegisterValueChangedCallback(evt);
        }
        
        public void RegisterValueChangedCallback(EventCallback<ChangeEvent<string>> evt)
        {
            _inputField.RegisterValueChangedCallback(evt);
        }
        public void AddButtonClick(Action onClick)
        {
            _button.clicked += onClick;
        }
    }
}
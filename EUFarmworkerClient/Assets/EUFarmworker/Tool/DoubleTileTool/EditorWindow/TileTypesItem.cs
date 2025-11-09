using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace EUFarmworker.Tool.DoubleTileTool.EditorWindow
{
    public class TileTypesItem : VisualElement
    {
        private TemplateContainer _container;
        public new class UxmlFactory : UxmlFactory<TileTypesItem> {}
        private Button _button;
        private TextField  _inputField;
        private Label _index;
        public TileTypesItem()
        {
            _container = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/EUFarmworker/Tool/EditorResources/DoubleTileTool/TileTypesItem.uxml").Instantiate();
            hierarchy.Add(_container);
            _index = _container.Q<Label>("Index");
            _button = _container.Q<Button>("RemoveButton");
            _inputField = _container.Q<TextField>("TileTypeInput");
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

//  Copyright (c) 2020-present amlovey
//  
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace APlus2
{
    public class SearchInput : Element
    {
        private TextField textField;
        private Label clearLabel;
        private Label searchIcon;
        private Label placeholderText;

        public Action<string> OnSubmit;
        public Action<string> OnValueChanged;

        public SearchInput(string placeholder = "")
        {
            this.style.height = 28;
            this.style.fontSize = 13;
            this.style.flexDirection = FlexDirection.Row;
            this.style.alignItems = Align.Center;
            this.AddToClassList("ap-search-input");

            this.textField = new TextField();
            textField.RegisterCallback<FocusInEvent>(evt => Input.imeCompositionMode = IMECompositionMode.On);
            textField.RegisterCallback<FocusOutEvent>(evt => Input.imeCompositionMode = IMECompositionMode.Auto);
            textField.style.height = 28;
            textField.style.position = Position.Absolute;
            textField.style.left = 0;
            textField.style.right = 0;
            this.textField.style.marginBottom = 0;
            textField.RegisterValueChangedCallback(OnTextFieldValueChanged);
            textField.Q(TextField.textInputUssName).RegisterCallback<KeyDownEvent>(OnKeyboradDown);

            searchIcon = new Label();
            searchIcon.AddToClassList("ap-icon");
            searchIcon.text = Icons.Search.value;
            searchIcon.style.width = 24;
            searchIcon.style.height = 24;
            searchIcon.style.fontSize = 12;
            searchIcon.style.marginLeft = 5;
            searchIcon.style.unityTextAlign = TextAnchor.MiddleCenter;

            clearLabel = new Label();
            clearLabel.AddToClassList("ap-icon");
            clearLabel.text = Icons.False.value;
            clearLabel.style.position = Position.Absolute;
            clearLabel.style.right = 0;
            clearLabel.style.width = 24;
            clearLabel.style.height = 24;
            clearLabel.style.fontSize = 12;
            clearLabel.style.marginRight = 5;
            clearLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            clearLabel.style.color = ColorHelper.Parse(Themes.Current.ForegroundColor);
            clearLabel.RegisterCallback<MouseDownEvent>(OnClearButtonClick);

#if UNITY_2021_2_OR_NEWER
            var stylesheet = Utilities.GetStylesheet();
            var path = UnityEditor.AssetDatabase.GetAssetPath(stylesheet);
            var fontAssetPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), "fontawesome.asset");
            var fontAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextCore.Text.FontAsset>(fontAssetPath);
            clearLabel.style.unityFontDefinition = new StyleFontDefinition(fontAsset);
            searchIcon.style.unityFontDefinition = new StyleFontDefinition(fontAsset);
#endif

            placeholderText = new Label();
            placeholderText.text = placeholder;
            placeholderText.style.position = Position.Absolute;
            placeholderText.style.left = 24;
            placeholderText.style.unityTextAlign = TextAnchor.MiddleLeft;

            this.Add(placeholderText);
            this.Add(textField);
            this.Add(searchIcon);
            this.Add(clearLabel);
            this.UpdateUIState();
        }

        public void Reset()
        {
            SetText(string.Empty);
        }

        public void SetText(string text)
        {
            this.textField.value = text;
            this.UpdateUIState();
        }

        private void OnKeyboradDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                if (OnSubmit != null)
                {
                    OnSubmit(this.textField.value);
                }
            }
        }

        private void OnTextFieldValueChanged(ChangeEvent<string> evt)
        {
            if (OnValueChanged != null)
            {
                OnValueChanged(this.textField.value);
            }
            this.UpdateUIState();
        }

        private void UpdateUIState()
        {
            this.clearLabel.visible = !string.IsNullOrEmpty(this.textField.value);
            this.searchIcon.style.color = ColorHelper.Parse(string.IsNullOrEmpty(this.textField.value) ? "#909090" : Themes.Current.ForegroundColor);
            this.placeholderText.visible = !this.clearLabel.visible;
        }

        private void OnClearButtonClick(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                this.textField.value = string.Empty;
                this.UpdateUIState();
                if (OnSubmit != null)
                {
                    OnSubmit(string.Empty);
                }
            }
        }
    }
}

//  Copyright (c) 2020-present amlovey
//  
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UIElements;

namespace APlus2
{
    [Serializable]
    public class Column : ScriptableObject
    {
        public const int DEFAULT_WIDTH = 120;

        public string key;
        public string header;
        public int width;
        public bool visible;
        public NullableBoolean isDesc;
        public bool showSortIcon;
        public string tooltip;
        public string[] additionalKeys;

        private ColumnAction mainAction;
        private List<ColumnAction> allActions;

        private void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;
        }

        public static Column CreateInstance(string key, string header, bool visible = true, string tooltip = "")
        {
            return CreateInstance(key, header, DEFAULT_WIDTH, visible, tooltip);
        }

        public static Column CreateCopy(Column src)
        {
            return CreateInstance(src.key, src.header, src.width, src.visible, src.tooltip);
        }

        public static Column CreateInstance(string key, string header, int width, bool visible = true, string tooltip = "")
        {
            var instance = ScriptableObject.CreateInstance<Column>();
            instance.header = header;
            instance.width = width;
            instance.visible = visible;
            instance.key = key;
            instance.tooltip = string.IsNullOrEmpty(tooltip) ? header : tooltip;
            return instance;
        }

        public List<APAsset> DoSort(List<APAsset> list)
        {
            if (mainAction == null)
            {
                mainAction = GetMainAction();
            }

            List<APAsset> newList = list;

            switch (mainAction.actionType)
            {
                case ColumnActionType.Index:
                case ColumnActionType.AssetName:
                    newList = mainAction.DoSort(list);
                    break;
                case ColumnActionType.FontIcon:
                    var fiAction = new FontIconColumnAction<APAsset>(asset => FontIonDataGetter(asset));
                    fiAction.SetColumn(this);
                    newList = fiAction.DoSort(list); 
                    break;
                case ColumnActionType.Label:
                    var lableAction = new LabelColumnAction<APAsset>(null, asset => LabelDataSelector(asset));
                    lableAction.SetColumn(this);
                    newList = lableAction.DoSort(list);
                    break;
            }
            
            return newList;
        }

        private bool? FontIonDataGetter(APAsset asset)
        {
            var action = GetColumnActionByData(asset);
            if (action != null)
            {
                return action.SelectData(asset) as bool?;
            }
            
            return null;
        }

        private object LabelDataSelector(APAsset asset)
        {
            var action = GetColumnActionByData(asset);
            if (action != null)
            {
                return action.SelectData(asset);
            }

            return null;
        }

        public void SetCellData(VisualElement element, object data)
        {
            var action = GetColumnActionByData(data);
            if (action != null)
            {
                action.SetCellData(element, data);
            }
            else
            {
                mainAction.SetCellData(element, null);
            }
        }

        private ColumnAction GetColumnActionByData(object data)
        {
            var dataType = data.GetType().ToString();

            if (allActions == null)
            {
                allActions = GetColumnActions();
            }

            foreach (var action in allActions)
            {
                if (action.key.StartsWith("apasset_") || action.key == "#" || action.GetDataType() == dataType)
                {
                    return action;
                }
            }

            return null;
        }

        public VisualElement CreateCellControl()
        {
            if (mainAction == null)
            {
                mainAction = GetMainAction();
            }

            return mainAction.CreateCellControl();
        }

        private ColumnAction GetMainAction()
        {
            var action = GetColumnActionFromKey(this.key);
            action.SetColumn(this);
            return action;
        }

        private List<ColumnAction> GetColumnActions()
        {
            List<ColumnAction> actions = new List<ColumnAction>();
            if (mainAction == null)
            {
                mainAction = GetMainAction();
            }

            actions.Add(mainAction);

            if (this.additionalKeys != null)
            {
                foreach (var item in this.additionalKeys)
                {
                    if (string.IsNullOrEmpty(item))
                    {
                        continue;
                    }

                    var action = GetColumnActionFromKey(item);
                    action.SetColumn(this);
                    actions.Add(action);
                }
            }

            return actions;
        }

        public static ColumnAction GetColumnActionFromKey(string key)
        {
            if (!string.IsNullOrEmpty(key) && TableDefinitions.ActionsMap.ContainsKey(key.ToLower()))
            {
                var columnAction = TableDefinitions.ActionsMap[key.ToLower()];
                return columnAction;
            }

            throw new Exception(string.Format("Key {0} is not exist in map", key));
        }
    }

    public enum ColumnActionType
    {
        Index,
        FontIcon,
        AssetName,
        Label
    }

    public abstract class ColumnAction
    {
        protected Column column;

        public string key;
        public ColumnActionType actionType;

        public void SetColumn(Column column)
        {
            this.column = column;
        }

        public virtual object SelectData(APAsset asset)
        {
            return null;
        }

        public virtual string GetDataType()
        {
            return string.Empty;
        }

        public abstract object GetRawData(APAsset asset);
        public abstract VisualElement CreateCellControl();
        public abstract void SetCellData(VisualElement element, object data);
        public abstract List<APAsset> DoSort(List<APAsset> list);
    }

    public class IndexColumnAction : ColumnAction
    {
        public IndexColumnAction()
        {
            actionType = ColumnActionType.Index;
        }

        public override VisualElement CreateCellControl()
        {
            var label = UIHelper.CreateNoMarginPaddingLabel();
            label.style.width = this.column.width;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            return label;
        }

        public override List<APAsset> DoSort(List<APAsset> list)
        {
            return list;
        }

        public override void SetCellData(VisualElement element, object data)
        {
            var label = element as Label;
            if (label != null)
            {
                label.text = data == null ? string.Empty : data.ToString();
            }
        }

        public override object GetRawData(APAsset asset)
        {
            return 0;
        }
    }

    public class FontIconColumnAction<T> : ColumnAction where T : APAsset
    {
        public Func<T, bool?> dataGetter;

        public FontIconColumnAction(Func<T, bool?> dataGetter)
        {
            this.dataGetter = dataGetter;
            actionType = ColumnActionType.FontIcon;
        }

        public override string GetDataType()
        {
            return typeof(T).ToString();
        }

        public override object SelectData(APAsset asset)
        {
            return this.dataGetter(asset as T);
        }

        public override VisualElement CreateCellControl()
        {
            var label = UIHelper.CreateNoMarginPaddingLabel();
            label.AddToClassList("ap-icon");

#if UNITY_2021_2_OR_NEWER
            var stylesheet = Utilities.GetStylesheet();
            var path = UnityEditor.AssetDatabase.GetAssetPath(stylesheet);
            var fontAssetPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), "fontawesome.asset");
            var fontAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextCore.Text.FontAsset>(fontAssetPath);
            label.style.unityFontDefinition = new StyleFontDefinition(fontAsset);
#endif

            label.style.width = 24;
            label.style.height = 24;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.flexShrink = 0;
            label.style.alignSelf = Align.Center;
            return label;
        }

        public override List<APAsset> DoSort(List<APAsset> list)
        {
            var newList = list.Cast<T>();
            switch (this.column.isDesc)
            {
                case NullableBoolean.True:
                    newList = newList.OrderByDescending(this.dataGetter);
                    break;
                case NullableBoolean.False:
                    newList = newList.OrderBy(this.dataGetter);
                    break;
            }

            return newList.Cast<APAsset>().ToList();
        }

        public override object GetRawData(APAsset asset)
        {
            return this.dataGetter(asset as T);
        }

        public override void SetCellData(VisualElement element, object data)
        {
            var label = element as Label;
            var asset = data as T;
            if (label != null && asset != null)
            {
                label.text = UIHelper.GetUnsedMark(this.dataGetter(asset)).value;
            }
        }
    }

    [Serializable]
    public class AssetNameColumnAction<T> : ColumnAction where T : APAsset
    {
        public AssetNameColumnAction()
        {
            actionType = ColumnActionType.AssetName;
        }

        public override string GetDataType()
        {
            return typeof(T).ToString();
        }

        public override VisualElement CreateCellControl()
        {
            var nameControl = new AssetNameElement(this.column.width);
            return nameControl;
        }

        public override List<APAsset> DoSort(List<APAsset> list)
        {
            var newList = list.Cast<T>();
            switch (column.isDesc)
            {
                case NullableBoolean.True:
                    newList = newList.OrderByDescending(asset => asset.Name);
                    break;
                case NullableBoolean.False:
                    newList = newList.OrderBy(asset => asset.Name);
                    break;
            }

            return newList.Cast<APAsset>().ToList();
        }

        public override object GetRawData(APAsset asset)
        {
            return asset.Name;
        }

        public override void SetCellData(VisualElement element, object data)
        {
            var control = element as AssetNameElement;
            var asset = data as APAsset;
            if (control != null && asset != null)
            {
                control.SetData(asset);
            }
        }
    }

    [Serializable]
    public class LabelColumnAction<T> : ColumnAction where T : APAsset
    {
        public LabelColumnAction(Func<T, string> dataGetter, Func<T, object> selector)
        {
            this.dataGetter = dataGetter;
            this.selector = selector;
            actionType = ColumnActionType.Label;
        }

        public Func<T, string> dataGetter;
        public Func<T, object> selector;

        public override VisualElement CreateCellControl()
        {
            var label = UIHelper.CreateNoMarginPaddingLabel();
            label.style.width = this.column.width;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.overflow = Overflow.Hidden;
            return label;
        }

        public override string GetDataType()
        {
            return typeof(T).ToString();
        }

        public override object SelectData(APAsset asset)
        {
            return this.selector(asset as T);
        }

        private string UpdateTextEllipsisIfNeeds(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var valMayWithEllipsis = value;
            var maxLength = Mathf.FloorToInt(this.column.width / 7);
            if (value.Length > maxLength)
            {
                valMayWithEllipsis = value.Substring(0, maxLength - 3) + "...";
            }

            return valMayWithEllipsis;
        }

        public override void SetCellData(VisualElement element, object data)
        {
            var label = element as Label;
            var asset = data as T;

            if (label != null)
            {
                if (asset == null)
                {
                    label.text = string.Empty;
                    label.tooltip = string.Empty;
                    return;
                }

                try
                {
                    var value = this.dataGetter(asset);
                    label.text = UpdateTextEllipsisIfNeeds(value);
                    label.tooltip = label.text.Equals(value) ? null : value;
                }
                catch
                {
                    label.text = string.Empty;
                    label.tooltip = string.Empty;
                }
            }
        }

        public override List<APAsset> DoSort(List<APAsset> list)
        {
            if (this.selector == null)
            {
                return list;
            }

            var newList = list.Select(item => item as T);
            switch (column.isDesc)
            {
                case NullableBoolean.True:
                    newList = newList.OrderByDescending(this.selector);
                    break;
                case NullableBoolean.False:
                    newList = newList.OrderBy(this.selector);
                    break;
            }

            return newList.Cast<APAsset>().ToList();
        }

        public override object GetRawData(APAsset asset)
        {
            return this.selector(asset as T);
        }
    }
}
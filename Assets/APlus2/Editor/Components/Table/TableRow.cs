//  Copyright (c) 2020-present amlovey
//  
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace APlus2
{
    public class TableRow : Element
    {
        private List<VisualElement> controls;
        private List<Column> columns;

        public TableRow(List<Column> columns)
        {
            this.columns = columns;
            this.AddToClassList("ap-table-row");

            this.style.flexDirection = FlexDirection.Row;
            this.style.paddingLeft = 8;
            this.style.paddingRight = 8;
            this.style.fontSize = 12;
            
            this.controls = new List<VisualElement>();
            foreach (var item in columns)
            {
                var control = item.CreateCellControl();
                controls.Add(control);
                this.Add(control);
            }
        }

        public void SetActive(bool active)
        {
            this.EnableInClassList("row-unused", !active);
        }

        public void SetData(int index, APAsset data)
        {
            this.EnableInClassList("ap-table-row_odd", index % 2 == 0);
            switch(data.Used)
            {
                case NullableBoolean.False:
                    this.SetActive(false);
                    break;
                case NullableBoolean.True:
                case NullableBoolean.Unkonwn:
                    this.SetActive(true);
                    break;
            }

            this.columns[0].SetCellData(this.controls[0], index);

            for (int i = 1; i < controls.Count; i++)
            {
                this.columns[i].SetCellData(this.controls[i], data);
                
                if (this.columns[i].visible)
                {
                    controls[i].style.visibility = Visibility.Visible;
                    controls[i].style.width = this.columns[i].width;
                }
                else
                {
                    controls[i].style.visibility = Visibility.Hidden;
                    controls[i].style.width = 0;
                }
            }
        }
    }
}
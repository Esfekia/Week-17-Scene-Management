//  Copyright (c) 2020-present amlovey
//  
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq;
using UnityEngine;

namespace APlus2
{
    public class Table : Container
    {
        public TableListView listView;
        public List<Column> columns;
        public Action<Column> OnHeaderClick;

        private Container headersContainer;
        private Container bodyContainer;
        private List<APAsset> dataSource;
        private Label noDataLabel;

        public Table(List<Column> columns)
        {
            this.columns = new List<Column>();
            Column indexColumn = Column.CreateInstance("#", "#", 56);
            this.columns.Add(indexColumn);
            this.columns.AddRange(columns);

            int headerHeight = TableHeader.DEFAULT_HEIGHT;
            headersContainer = new Container();
            headersContainer.style.height = headerHeight;
            headersContainer.style.flexDirection = FlexDirection.Row;
            headersContainer.style.paddingLeft = 8;
            headersContainer.style.paddingRight = 8;
            headersContainer.style.right = 15;
            headersContainer.style.overflow = Overflow.Hidden;
            this.RenderHeaders();

            this.bodyContainer = new Container();
            this.bodyContainer.style.top = headerHeight;


            this.RenderNoDataLabel();
            this.GenerateBody();
            this.Add(headersContainer);
            this.Add(bodyContainer);
        }

        public void ResetSortState()
        {
            foreach (var item in columns)
            {
                item.isDesc = NullableBoolean.Unkonwn;
                item.showSortIcon = false;
            }

            this.UpdateHeaders();
        }

        public void Update()
        {
            this.UpdateHeaders();
#if UNITY_2021_2_OR_NEWER
            this.listView.Rebuild();
#else
            this.listView.Refresh();
#endif
        }

        private void RenderNoDataLabel()
        {
            noDataLabel = new Label("No Assets Found!");
            noDataLabel.style.position = Position.Absolute;
            noDataLabel.style.left = 0;
            noDataLabel.style.right = 0;
            noDataLabel.style.top = 0;
            noDataLabel.style.bottom = float.NaN;
            noDataLabel.style.height = 300;
            noDataLabel.style.fontSize = 14;
            noDataLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            noDataLabel.visible = false;
            this.bodyContainer.Add(noDataLabel);
        }

        private void UpdateHeaders()
        {
            var columnControls = this.headersContainer.Children();
            foreach (var item in columnControls)
            {
                TableHeader header = item as TableHeader;
                if (header != null)
                {
                    header.UpdateShowOrHide();
                    header.ShowSortIconIfNeeds();
                }
            }
        }

        private void RenderHeaders()
        {
            for (int i = 0; i < this.columns.Count; i++)
            {
                var column = this.columns[i];
                var header = new TableHeader(column);

                // Index column will not clickable
                if (i != 0)
                {
                    header.OnClick = () =>
                    {
                        if (this.noDataLabel.visible)
                        {
                            return;
                        }

                        if (OnHeaderClick != null)
                        {

                            switch (column.isDesc)
                            {
                                case NullableBoolean.True:
                                    column.isDesc = NullableBoolean.False;
                                    break;
                                case NullableBoolean.False:
                                case NullableBoolean.Unkonwn:
                                    column.isDesc = NullableBoolean.True;
                                    break;
                            }

                            OnHeaderClick(column);
                        }
                    };
                }

                header.UpdateShowOrHide();
                header.ShowSortIconIfNeeds();
                this.headersContainer.Add(header);
            }
        }

        public void SetDataSource(List<APAsset> dataSource)
        {
            if (dataSource == null)
            {
                dataSource = new List<APAsset>();
            }

            this.listView.itemsSource = dataSource;
            this.dataSource = dataSource;
            noDataLabel.visible = dataSource.Count() == 0;
        }

        public int GetItemsCount()
        {
            return this.dataSource.Count;
        }

        private void GenerateBody()
        {
            listView = new TableListView();
            listView.SetItemHeight(32);
            listView.selectionType = SelectionType.Multiple;
            listView.style.flexGrow = 1.0f;
            listView.makeItem = () => new TableRow(this.columns);
            listView.bindItem = (row, index) =>
            {
                var tableRow = row as TableRow;
                if (index < this.dataSource.Count)
                {
                    tableRow.SetData(index + 1, this.dataSource[index]);
                }
            };

            this.bodyContainer.Add(listView);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

namespace Karuta.UIComponent
{
    public class TypeContainer : Container
    {
        [Header("Type Container")]
        [SerializeField] private VerticalCenteredGrid grid;
        [SerializeField] private float nameWidth;

        #region Add Items
        /// <summary>
        /// Add an item to the grid
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        override public void AddItem<T>(T item)
        {
            grid.AddItem(item);
            ResizeContainer();
        }

        /// <summary>
        /// Add a list of items to the grid
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        override public void AddItems<T>(List<T> items)
        {
            grid.AddItems(items);
            ResizeContainer();
        }
        #endregion Add Items

        #region Size
        override protected float CalculateSubContainerHeight()
        {
            return grid.GetSize().y;
        }
        override protected float CalculateSubContainerWidth()
        {
            return grid.GetSize().x;
        }
        #endregion Size

        public void SetNameWidth(float width)
        {
            nameWidth = width;
            nameRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        public float GetNameWidth()
        {
            return nameWidth;
        }

        public void SetGridParameters(Vector2 cellSize, Vector2 spacing, int columnNumber)
        {
            grid.SetCellSize(cellSize);
            grid.SetSpacing(spacing);
            grid.SetColumnNumber(columnNumber);

            ResizeContainer();
        }

        // Called if changes in code or editor (not called at runtime)
        override protected void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }
            
            SetNameWidth(nameWidth);

            base.OnValidate();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

namespace Karuta.UIComponent
{
    public class VerticalCenteredGrid : MonoBehaviour
    {
        [Header("Grid Values")]
        [SerializeField] private Vector2 cellSize = new (100, 100);
        [SerializeField] private Vector2 spacing;
        [SerializeField] private TextAnchor childAlignment;
        [Min(1)]
        [SerializeField] private int columnNumber;

        [Header("Intern Objects")]
        [SerializeField] private VerticalLayoutGroup generalLayout;
        [SerializeField] RectTransform generalLayoutRectTransform;
        [SerializeField] private GridLayoutGroup fullGrid;
        [SerializeField] private GridLayoutGroup nonFullGrid;
        [SerializeField] private Transform nonActiveObjects;

        #region Add Items
        /// <summary>
        /// Add a list of items to the grid
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        public void AddItems<T>(List<T> items) where T : Component
        {
            int count = 0;
            foreach (T item in items)
            {
                if (item.gameObject.activeSelf) { count++; }
            }

            int nonFullCount = count % columnNumber;
            int fullCount = count - nonFullCount;

            int i = 0;
            int itemCount = 0;
            while (itemCount < fullCount)
            {
                if (items[i].gameObject.activeSelf)
                {
                    items[i].transform.SetParent(fullGrid.transform);
                    itemCount++;
                }
                else
                {
                    items[i].transform.SetParent(nonActiveObjects);
                }
                i++;
            }

            for (int j = i; j < items.Count; j++)
            {
                if (items[j].gameObject.activeSelf)
                {
                    items[j].transform.SetParent(nonFullGrid.transform);
                }
                else
                {
                    items[j].transform.SetParent(nonActiveObjects);
                }
            }

            Resize();
            UpdateActive();
        }

        /// <summary>
        /// Add an item to the grid
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        public void AddItem<T>(T item) where T : Component
        {
            if (item.gameObject.activeSelf)
            {
                if (nonFullGrid.transform.childCount == columnNumber - 1)
                {
                    while (nonFullGrid.transform.childCount > 0)
                    {
                        nonFullGrid.transform.GetChild(0).SetParent(fullGrid.transform);
                    }
                    item.transform.SetParent(fullGrid.transform);

                    Resize();
                }

                else
                {
                    item.transform.SetParent(nonFullGrid.transform);
                }

                UpdateActive();
            }
            else
            {
                item.transform.SetParent(nonActiveObjects.transform);
            }
        }
        #endregion Add Items

        #region Size
        /// <summary>
        /// Update the rectTransform size to match the elements of the grid
        /// </summary>
        private void Resize()
        {
            generalLayoutRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CalculateWidth());
            generalLayoutRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CalculateHeight());
        }

        private float CalculateHeight()
        {
            int nonFull = 0;
            if (nonFullGrid.transform.childCount > 0)
            {
                nonFull = 1;
            }
            return (cellSize.y + spacing.y) * ((fullGrid.transform.childCount / columnNumber) + nonFull) - spacing.y;
        }

        private float CalculateWidth()
        {
            return (cellSize.x + spacing.x) * columnNumber - spacing.x;
        }

        public Vector2 CalculateSize()
        {
            return new Vector2(CalculateWidth(), CalculateHeight());
        }

        public Vector2 GetSize()
        {
            return generalLayoutRectTransform.sizeDelta;
        }
        #endregion Size

        #region Setter
        public void SetCellSize(Vector2 size)
        {
            cellSize = size;
            fullGrid.cellSize = cellSize;
            nonFullGrid.cellSize = cellSize;

            Resize();
        }
        public void SetSpacing(Vector2 spacing)
        {
            this.spacing = spacing;
            generalLayout.spacing = spacing.y;
            fullGrid.spacing = spacing;
            nonFullGrid.spacing = spacing;

            Resize();
        }
        public void SetChildAlignment(TextAnchor anchor)
        {
            childAlignment = anchor;
            fullGrid.childAlignment = anchor;
            nonFullGrid.childAlignment = anchor;

            Resize();
        }
        public void SetColumnNumber(int number)
        {
            if (number < 1) { number = 1; }

            columnNumber = number;
            fullGrid.constraintCount = number;
            nonFullGrid.constraintCount = number;

            Resize();
        }
        #endregion Setter

        #region Getter
        public Vector2 GetCellSize()
        {
            return cellSize;
        }
        public Vector2 GetSpacing()
        {
            return spacing;
        }
        public TextAnchor GetChildAlignment()
        {
            return childAlignment;
        }
        public int GetConstraintCount()
        {
            return columnNumber;
        }
        #endregion Getter

        private void UpdateActive()
        {
            fullGrid.gameObject.SetActive(fullGrid.transform.childCount > 0);
            Debug.Log(fullGrid.transform.childCount > 0);

            nonFullGrid.gameObject.SetActive(nonFullGrid.transform.childCount > 0);
        }

        // Called if changes in code or editor (not called at runtime)
        private void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            SetCellSize(cellSize);
            SetSpacing(spacing);
            SetChildAlignment(childAlignment);
            SetColumnNumber(columnNumber);
            UpdateActive();
        }
    }
}
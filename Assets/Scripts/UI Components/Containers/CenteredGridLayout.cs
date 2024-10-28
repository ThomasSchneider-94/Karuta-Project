using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Unity.VisualScripting;

namespace Karuta.UIComponent
{
    public class CenteredGridLayout : MonoBehaviour
    {
        [Header("Grid Values")]
        [SerializeField] protected Vector2 cellSize = new(100, 100);
        [SerializeField] protected Vector2 spacing;
        [SerializeField] protected TextAnchor childAlignment;
        [Min(1)]
        [SerializeField] protected int constraintCount;

        [Header("Intern Objects")]
        [SerializeField] private VerticalLayoutGroup generalLayout;
        [SerializeField] protected RectTransform generalLayoutRectTransform;
        [SerializeField] protected GridLayoutGroup fullGrid;
        [SerializeField] protected RectTransform fullGridRectTransform;
        [SerializeField] protected GridLayoutGroup nonFullGrid;
        [SerializeField] protected RectTransform nonFullGridRectTransform;
        [SerializeField] protected Transform nonActiveObjects;

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

            int nonFullCount = count % constraintCount;
            int fullCount = count - nonFullCount;

            int i = 0;
            int itemCount = 0;
            while (itemCount < fullCount)
            {
                if (items[i].gameObject.activeSelf)
                {
                    items[i].transform.SetParent(fullGrid.transform);
                    items[i].transform.SetAsLastSibling();
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
                    items[j].transform.SetAsLastSibling();
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
                if (nonFullGrid.transform.childCount == constraintCount - 1)
                {
                    while (nonFullGrid.transform.childCount > 0)
                    {
                        nonFullGrid.transform.GetChild(0).SetParent(fullGrid.transform);
                    }
                    item.transform.SetParent(fullGrid.transform);
                    item.transform.SetAsLastSibling();

                    Resize();
                }

                else
                {
                    item.transform.SetParent(nonFullGrid.transform);
                    item.transform.SetAsLastSibling();
                }

                UpdateActive();
            }
            else
            {
                item.transform.SetParent(nonActiveObjects.transform);
            }
        }

        /// <summary>
        /// Reposition the items in the grid if some of then became non active
        /// </summary>
        public void RepositionItems()
        {
            List<Transform> childs = new();
            for (int i = 0; i < fullGrid.transform.childCount; i++)
            {
                childs.Add(fullGrid.transform.GetChild(i));
            }
            for (int i = 0; i < nonFullGrid.transform.childCount; i++)
            {
                childs.Add(nonFullGrid.transform.GetChild(i));
            }
            for (int i = 0; i < nonActiveObjects.childCount; i++)
            {
                childs.Add(nonActiveObjects.GetChild(i));
            }
            AddItems(childs);
        }
        #endregion Add sItems

        #region Size
        /// <summary>
        /// Update the rectTransform size to match the elements of the grid
        /// </summary>
        private void Resize()
        {
            int fullNonEmpty = fullGrid.transform.childCount > 0 ? 1 : 0;
            int nonFullNonEmpty = nonFullGrid.transform.childCount > 0 ? 1 : 0;
            int nonEmpty = Mathf.Min(fullNonEmpty, nonFullNonEmpty);
            
            fullGridRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (cellSize.x + spacing.x) * constraintCount - spacing.x);
            fullGridRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (cellSize.y + spacing.y) * (fullGrid.transform.childCount / constraintCount) - spacing.y * fullNonEmpty);
            
            nonFullGridRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (cellSize.x + spacing.x) * constraintCount - spacing.x);
            nonFullGridRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellSize.y * nonFullNonEmpty);

            generalLayoutRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (cellSize.x + spacing.x) * constraintCount - spacing.x);
            generalLayoutRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, fullGridRectTransform.sizeDelta.y + nonFullGridRectTransform.sizeDelta.y + spacing.y * nonEmpty);
        }

        public Vector2 GetSize()
        {
            return generalLayoutRectTransform.sizeDelta;
        }
        #endregion Size

        #region Setter
        public void SetCellSize(Vector2 size)
        {
            this.cellSize = size;
            fullGrid.cellSize = cellSize;
            nonFullGrid.cellSize = cellSize;

            Resize();
        }

        public void SetSpacing(Vector2 spacing)
        {
            this.spacing = spacing;
            fullGrid.spacing = spacing;
            nonFullGrid.spacing = spacing;
            generalLayout.spacing = spacing.y;

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

            constraintCount = number;
            fullGrid.constraintCount = number;
            nonFullGrid.constraintCount = number;

            Resize();
        }

        public void SetAllParameters(Vector2 cellSize, Vector2 spacing, int columnNumber)
        {
            // Cell Size
            this.cellSize = cellSize;
            fullGrid.cellSize = cellSize;
            nonFullGrid.cellSize = cellSize;

            // Spacing
            this.spacing = spacing;
            fullGrid.spacing = spacing;
            nonFullGrid.spacing = spacing;
            generalLayout.spacing = spacing.y;

            // Column Number
            this.constraintCount = columnNumber;
            fullGrid.constraintCount = columnNumber;
            nonFullGrid.constraintCount = columnNumber;

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
            return constraintCount;
        }
        #endregion Getter

        private void UpdateActive()
        {
            fullGrid.gameObject.SetActive(fullGrid.transform.childCount > 0);
            nonFullGrid.gameObject.SetActive(nonFullGrid.transform.childCount > 0);
        }

        // Called if changes in code or editor (not called at runtime)
        private void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            fullGrid.cellSize = cellSize;
            nonFullGrid.cellSize = cellSize;

            fullGrid.spacing = spacing;
            nonFullGrid.spacing = spacing;
            generalLayout.spacing = spacing.y;

            fullGrid.childAlignment = childAlignment;
            nonFullGrid.childAlignment = childAlignment;

            fullGrid.constraintCount = constraintCount;
            nonFullGrid.constraintCount = constraintCount;

            Resize();

            UpdateActive();
        }
    }
}
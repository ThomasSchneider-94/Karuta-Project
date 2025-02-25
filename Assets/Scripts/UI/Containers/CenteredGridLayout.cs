using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

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
            foreach (Transform child in fullGrid.transform)
            {
                childs.Add(child);
            }
            foreach (Transform child in nonFullGrid.transform)
            {
                childs.Add(child);
            }
            foreach (Transform child in nonActiveObjects.transform)
            {
                childs.Add(child);
            }
            AddItems(childs);
        }
        #endregion Add sItems

        #region Setter
        public void SetCellSize(Vector2 size)
        {
            this.cellSize = size;
            fullGrid.cellSize = cellSize;
            nonFullGrid.cellSize = cellSize;
        }

        public void SetSpacing(Vector2 spacing)
        {
            this.spacing = spacing;
            fullGrid.spacing = spacing;
            nonFullGrid.spacing = spacing;
            generalLayout.spacing = spacing.y;
        }

        public void SetChildAlignment(TextAnchor anchor)
        {
            childAlignment = anchor;
            fullGrid.childAlignment = anchor;
            nonFullGrid.childAlignment = anchor;
        }

        public void SetColumnNumber(int number)
        {
            if (number < 1) { number = 1; }

            constraintCount = number;
            fullGrid.constraintCount = number;
            nonFullGrid.constraintCount = number;
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

        public int GetChildNumber()
        {
            return fullGrid.transform.childCount + nonFullGrid.transform.childCount;
        }

#if UNITY_EDITOR
        // Called if changes in code or editor (not called at runtime)
        private void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            SetCellSize(cellSize);
            SetSpacing(spacing);
            SetChildAlignment(childAlignment);
            SetColumnNumber(constraintCount);

            UpdateActive();
        }
#endif
    }
}
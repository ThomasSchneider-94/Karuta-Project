using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace Karuta.UIComponent
{
    abstract public class Container : MonoBehaviour
    {
        [Header("Container Values")]
        [SerializeField] protected string containerName;
        [SerializeField] protected float nameScale;
        [SerializeField] protected float nameSpacing;

        [Header("Container Elements")]
        [SerializeField] protected VerticalLayoutGroup containerLayout;
        [SerializeField] protected RectTransform containerRectTransform;
        [SerializeField] protected TextMeshProUGUI nameTextMesh;
        [SerializeField] protected RectTransform nameRectTransform;
        [SerializeField] protected RectTransform subContainerRectTransform;

        #region Add Items
        /// <summary>
        /// Add a list of items to the container
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        public virtual void AddItems<T>(List<T> items) where T : Component
        {
            foreach(T item in items)
            {
                item.transform.SetParent(subContainerRectTransform);
            }
            ResizeContainer();
        }

        /// <summary>
        /// Add an item to the container
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        public virtual void AddItem<T>(T item) where T : Component
        {
            item.transform.SetParent(subContainerRectTransform);
            ResizeContainer();
        }
        #endregion Add Items

        #region Size
        /// <summary>
        /// Set the size of the container so that it fits the size of its components
        /// </summary>
        public void ResizeContainer()
        {
            ResizeSubContainer();

            containerRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CalculateWidth());
            containerRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CalculateHeight());
        }

        /// <summary>
        /// Set the size of the subcontainer so that it fits the size of its components
        /// </summary>
        private void ResizeSubContainer()
        {
            subContainerRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CalculateSubContainerWidth());
            subContainerRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CalculateSubContainerHeight());
        }

        abstract protected float CalculateSubContainerWidth();

        abstract protected float CalculateSubContainerHeight();

        protected float CalculateWidth()
        {
            return Mathf.Max(nameRectTransform.sizeDelta.x * nameScale, subContainerRectTransform.sizeDelta.x);
        }

        protected float CalculateHeight()
        {
            return nameRectTransform.sizeDelta.y * nameScale + nameSpacing + subContainerRectTransform.sizeDelta.y;
        }

        public Vector2 GetSize()
        {
            return containerRectTransform.sizeDelta;
        }
        #endregion Size

        #region Setter
        public void SetName(string name)
        {
            containerName = name;
            nameTextMesh.text = name;
        }

        public void SetNameScale(float scale)
        {
            nameScale = scale;
            nameRectTransform.localScale = new Vector3(scale, scale, scale);

            ResizeContainer();
        }

        public void SetNameSpacing(float spacing)
        {
            nameSpacing = spacing;
            containerLayout.spacing = spacing;

            ResizeContainer();
        }
        #endregion Setter

        #region Getter
        public string GetName()
        {
            return containerName;
        }

        public float GetNameScale()
        {
            return nameScale;
        }

        public float GetNameSpacing()
        {
            return nameSpacing;
        }

        public virtual RectTransform GetSubContainer()
        {
            return subContainerRectTransform;
        }
        #endregion Getter

        // Called if changes in code or editor (not called at runtime)
        virtual protected void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            SetName(containerName);
            SetNameScale(nameScale);
            SetNameSpacing(nameSpacing);

            ResizeContainer();
        }
    }
}
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Karuta.UIComponent {
    public class CategoryContainer : Container
    {
        [Header("Category Container")]
        [SerializeField] protected VerticalLayoutGroup subContainer;
        [SerializeField] protected float subContainerSpacing;

        #region Size
        override protected float CalculateSubContainerHeight()
        {
            float height = 0;
            for (int i = 0; i < subContainerRectTransform.childCount; i++)
            {
                height += subContainerRectTransform.GetChild(i).GetComponent<RectTransform>().sizeDelta.y;
                height += subContainerSpacing;
            }
            if (subContainerRectTransform.childCount > 0)
            {
                height -= subContainerSpacing;
            }
            return height;
        }

        override protected float CalculateSubContainerWidth()
        {
            float width = 0;
            if (subContainerRectTransform.childCount > 0)
            {
                width = subContainerRectTransform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x;
            }
            return width;
        }
        #endregion Size

        public void SetSubContainerSpacing(float spacing)
        {
            subContainerSpacing = spacing;
            subContainer.spacing = spacing;

            ResizeContainer();
        }

        public float GetSubContainerSpacing()
        {
            return subContainerSpacing;
        }

        // Called if changes in code or editor (not called at runtime)
        override protected void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            SetSubContainerSpacing(subContainerSpacing);

            base.OnValidate();
        }
    }
}
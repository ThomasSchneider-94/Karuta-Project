using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.InputSystem.Android;
using System.Linq;

namespace Karuta.UIComponent
{
    public class Container : MonoBehaviour
    {
        [Header("Container Values")]
        [SerializeField] protected string containerName;
        [SerializeField] protected float nameScale;
        [SerializeField] private float nameWidth;

        [Header("Spacing")]
        [SerializeField] private float nameSpacing;
        [SerializeField] private float spacing;

        [Header("Container Elements")]
        [SerializeField] protected VerticalLayoutGroup layout;
        [SerializeField] protected RectTransform layoutRectTransform;
        [SerializeField] protected TextMeshProUGUI nameTextMesh;
        [SerializeField] protected RectTransform nameRectTransform;
        [SerializeField] protected VerticalLayoutGroup subLayout;
        [SerializeField] protected RectTransform subLayoutRectTransform;

        private readonly List<RectTransform> childRectTransforms = new();

        public void FindChildRectTransforms()
        {
            childRectTransforms.Clear();

            for (int i = 0; i < subLayout.transform.childCount; i++)
            {
                childRectTransforms.Add(subLayout.transform.GetChild(i).GetComponent<RectTransform>());
            }
        }

        public void ResizeContainer()
        {
            if (childRectTransforms.Count == 0)
            {
                FindChildRectTransforms();
            }

            int empty = childRectTransforms.Count == 0 ? 0 : 1;

            float subWidth = 0;
            float subHeight = -spacing * empty;
            foreach (RectTransform rectTransform in childRectTransforms)
            {
                if (rectTransform.gameObject.activeSelf)
                {
                    subWidth = Mathf.Max(subWidth, rectTransform.sizeDelta.x);
                    subHeight += rectTransform.sizeDelta.y + spacing;
                }
            }

            subLayoutRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, subWidth);
            subLayoutRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, subHeight);

            float width = Mathf.Max(subWidth, nameRectTransform.sizeDelta.x * nameScale);
            float height = nameRectTransform.sizeDelta.y * nameScale + nameSpacing * empty + subHeight;

            layoutRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            layoutRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        #region Setter
        public void SetName(string name)
        {
            containerName = name;
            nameTextMesh.text = name;
        }

        public void SetNameScale(float scale)
        {
            this.nameScale = scale;
            nameRectTransform.localScale = new Vector3(scale, scale, scale);

            ResizeContainer();
        }

        public void SetNameWidth(float width)
        {
            this.nameWidth = width;
            nameRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

            ResizeContainer();
        }

        public void SetSpacing(float spacing)
        {
            this.spacing = spacing;
            subLayout.spacing = spacing;

            ResizeContainer();
        }

        public void SetNameSpacing(float nameSpacing)
        {
            this.nameSpacing = nameSpacing;
            layout.spacing = nameSpacing;

            ResizeContainer();
        }

        public void SetAllParameters(string name, float nameScale, float nameWidth, float nameSpacing, float spacing)
        {
            // Name
            this.containerName = name;
            nameTextMesh.text = name;

            // Name Scale
            this.nameScale = nameScale;
            nameRectTransform.localScale = new Vector3(nameScale, nameScale, nameScale);

            // Name Width
            this.nameWidth = nameWidth;
            nameRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, nameWidth);

            // Name Spacing
            this.nameSpacing = nameSpacing;
            layout.spacing = nameSpacing;

            // Spacing
            this.spacing = spacing;
            subLayout.spacing = spacing;


            ResizeContainer();
        }
        #endregion Setter

        public Transform GetSubContainer()
        {
            return subLayout.transform;
        }

        private void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            nameTextMesh.text = containerName;
            nameRectTransform.localScale = new Vector3(nameScale, nameScale, nameScale);
            nameRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, nameWidth);
            subLayout.spacing = spacing;
            layout.spacing = nameSpacing;

            ResizeContainer();
        }
    }
}
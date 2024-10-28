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
        [SerializeField] private float spacing;

        [Header("Container Elements")]
        [SerializeField] protected VerticalLayoutGroup containerLayout;
        [SerializeField] protected RectTransform containerRectTransform;
        [SerializeField] protected TextMeshProUGUI nameTextMesh;
        [SerializeField] protected RectTransform nameRectTransform;

        private readonly List<RectTransform> childRectTransforms = new();

        public void FindChildRectTransforms()
        {
            childRectTransforms.Clear();

            for (int i = 1; i < containerRectTransform.childCount; i++)
            {
                childRectTransforms.Add(containerRectTransform.GetChild(i).GetComponent<RectTransform>());
            }
        }

        public void ResizeContainer()
        {
            if (childRectTransforms.Count == 0)
            {
                FindChildRectTransforms();
            }

            float width = nameRectTransform.sizeDelta.x * nameScale;
            if (childRectTransforms.Count > 0)
            {
                width = Mathf.Max(width, childRectTransforms.Max(child => child.sizeDelta.x));
            }
            float height = nameRectTransform.sizeDelta.y * nameScale + childRectTransforms.Sum(child => child.sizeDelta.y) + spacing * (childRectTransforms.Count - 1);


            containerRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            containerRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
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
            containerLayout.spacing = spacing;

            ResizeContainer();
        }

        public void SetAllParameters(string name, float nameScale, float nameWidth, float spacing)
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

            // Spacing
            this.spacing = spacing;
            containerLayout.spacing = spacing;

            ResizeContainer();
        }
        #endregion Setter

        private void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            nameTextMesh.text = containerName;
            nameRectTransform.localScale = new Vector3(nameScale, nameScale, nameScale);
            nameRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, nameWidth);
            containerLayout.spacing = spacing;

            ResizeContainer();
        }
    }
}
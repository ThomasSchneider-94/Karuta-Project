using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

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

        public void ForceResize()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(subLayoutRectTransform);

            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRectTransform);
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
        }

        public void SetNameWidth(float width)
        {
            this.nameWidth = width;
            nameRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        public void SetSpacing(float spacing)
        {
            this.spacing = spacing;
            subLayout.spacing = spacing;
        }

        public void SetNameSpacing(float nameSpacing)
        {
            this.nameSpacing = nameSpacing;
            layout.spacing = nameSpacing;
        }
        #endregion Setter

        public Transform GetSubContainer()
        {
            return subLayout.transform;
        }
        
        public TextMeshProUGUI GetNameTextMesh()
        {
            return nameTextMesh;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            nameTextMesh.text = containerName;
            nameRectTransform.localScale = new Vector3(nameScale, nameScale, nameScale);
            nameRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, nameWidth);
            subLayout.spacing = spacing;
            layout.spacing = nameSpacing;
        }
#endif
    }
}
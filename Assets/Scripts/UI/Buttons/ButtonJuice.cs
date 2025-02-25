using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static UnityEngine.UI.Button;

namespace Karuta.UIComponent
{
    [RequireComponent(typeof(Button))]
    public class ButtonJuice : MonoBehaviour
    {
        [SerializeField] private float movementTime;

        public UnityEvent functionToTrigger;

        public void MoveButton(float movement)
        {
            StartCoroutine(MoveButtonCoroutine(movement));
        }

        private IEnumerator MoveButtonCoroutine(float movement)
        {
            float t = 0;
            float initialPosition = transform.localPosition.x;

            while (t < movementTime)
            {
                transform.localPosition = new Vector2(Mathf.Lerp(initialPosition, initialPosition + movement, t / movementTime), transform.localPosition.y);
                t += Time.deltaTime;
                yield return null;
            }

            functionToTrigger.Invoke();
            transform.localPosition = new Vector2(initialPosition, transform.localPosition.y);
        }
    }
}
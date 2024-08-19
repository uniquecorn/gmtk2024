using Castle;
using Cysharp.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace TinyGame
{
    [RequireComponent(typeof(Canvas),typeof(Image))]
    public class DragSelector : MonoBehaviour
    {
        [HideInInspector,SerializeField]
        private new RectTransform transform;
        public RectTransform Transform => transform ? transform : transform = base.transform as RectTransform;
        public TextMeshProUGUI count;
        public void StartDrag(Vector3 startPosition)
        {
            Transform.position = startPosition.RepZ(-1);
            Transform.sizeDelta = Vector2.zero;
            count.canvasRenderer.SetAlpha(0);
        }
        public void Drag(Vector3 endPosition)
        {
            var dist = endPosition - Transform.position;
            bool pivotX = dist.x > 0;
            bool pivotY = dist.y > 0;
            Transform.pivot = Transform.anchorMin =
                Transform.anchorMax = new Vector2(pivotX ? 0 : 1, pivotY ? 0 : 1);
            Transform.sizeDelta = new Vector2(Mathf.Abs(dist.x),Mathf.Abs(dist.y));
        }

        public void SetCount(int num)
        {
            if (num == 0)
            {
                count.canvasRenderer.SetAlpha(0);
            }
            else
            {
                count.SetText(num);
                count.canvasRenderer.SetAlpha(1);
            }
        }
        public void EndDrag()
        {
            Transform.sizeDelta = Vector2.zero;
            count.canvasRenderer.SetAlpha(0);
        }
    }
}
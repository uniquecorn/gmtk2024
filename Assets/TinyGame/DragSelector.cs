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
        [System.NonSerialized]
        private static WorldSpawn[] dragAlloc;
        public int c;

        public void Start()
        {
            dragAlloc = new WorldSpawn[Chunk.ChunkMag];
        }
        public void StartDrag(Vector3 startPosition)
        {
            c = 0;
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
            for (var i = 0; i < c; i++)
            {
                if(dragAlloc[i] == null) continue;
                dragAlloc[i].TempSelect(false);
            }
            c = 0;
            if (Transform.rect.size.sqrMagnitude > 0.001f)
            {
                foreach (var e in World.Current.playerEntities)
                {
                    if (!Transform.rect.Contains(e.virtualPosition - transform.position)) continue;
                    if (!e.IsSpawned(out var spawn)) continue;
                    dragAlloc[c] = spawn;
                    dragAlloc[c].TempSelect(true);
                    c++;
                    if(c>= dragAlloc.Length) break;
                }
            }
            SetCount(c);
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
        public int EndDrag(out WorldSpawn[] selectedUnits)
        {
            for (var i = 0; i < c; i++)
            {
                if(dragAlloc[i] == null) continue;
                dragAlloc[i].TempSelect(false);
            }
            selectedUnits = dragAlloc;
            Transform.sizeDelta = Vector2.zero;
            count.canvasRenderer.SetAlpha(0);
            return c;
        }
    }
}
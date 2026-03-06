using UnityEngine;
using UnityEngine.UI;

namespace SlotExample
{
    public static class ReelMath
    {
        public static float Step(float cellHeight, float spacing) => cellHeight + spacing;

        public static float CalculateSnapTarget(RectTransform content, Image[] images, RectTransform snapCenter)
        {
            if (!snapCenter || !content || content.parent == null || images == null || images.Length == 0)
                return content ? content.anchoredPosition.y : 0f;

            Vector3 centerWorldPos = snapCenter.position;

            float closestDist = float.MaxValue;
            RectTransform closestSlot = null;

            foreach (var img in images)
            {
                if (!img) continue;
                var slotRt = (RectTransform)img.transform;
                float dist = Mathf.Abs(slotRt.position.y - centerWorldPos.y);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestSlot = slotRt;
                }
            }

            if (!closestSlot) return content.anchoredPosition.y;

            float centerLocalY = content.parent.InverseTransformPoint(centerWorldPos).y;
            float slotLocalY = content.parent.InverseTransformPoint(closestSlot.position).y;

            return content.anchoredPosition.y + (centerLocalY - slotLocalY);
        }
    }
}

using UnityEngine;
using UnityEngine.Sprites;

namespace Devil.Utility
{
   
    public enum UIPivot
    {
        TopLeft,
        Top,
        TopRight,
        Left,
        Center,
        Right,
        BottomLeft,
        Bottom,
        BottomRight,
    }
   
  
    public static class _2DUtil
    {
        public static bool AlignHCenter(UIPivot pivot)
        {
            return pivot == UIPivot.Center || pivot == UIPivot.Top || pivot == UIPivot.Bottom;
        }

        public static bool AlignVCenter(UIPivot pivot)
        {
            return pivot == UIPivot.Center || pivot == UIPivot.Left || pivot == UIPivot.Right;
        }

        public static bool AlignLeft(UIPivot pivot)
        {
            return pivot == UIPivot.Left || pivot == UIPivot.TopLeft || pivot == UIPivot.BottomLeft;
        }

        public static bool AlignRight(UIPivot pivot)
        {
            return pivot == UIPivot.Right || pivot == UIPivot.TopRight || pivot == UIPivot.BottomRight;
        }

        public static bool AlignTop(UIPivot pivot)
        {
            return pivot == UIPivot.Top || pivot == UIPivot.TopLeft || pivot == UIPivot.TopRight;
        }

        public static bool AlignBottom(UIPivot pivot)
        {
            return pivot == UIPivot.Bottom || pivot == UIPivot.BottomLeft || pivot == UIPivot.BottomRight;
        }

        public static bool ContainsRect(Rect rect, Rect subRect)
        {
            float x0, x1, y0, y1;
            if (rect.width >= 0)
            {
                x0 = rect.min.x;
                x1 = x0 + rect.width;
            }
            else
            {
                x1 = rect.min.x;
                x0 = x1 + rect.width;
            }
            if (rect.height >= 0)
            {
                y0 = rect.min.y;
                y1 = y0 += rect.height;
            }
            else
            {
                y1 = rect.min.y;
                y0 = y1 + rect.height;
            }
            return subRect.min.x >= x0 && subRect.min.x <= x1
                && subRect.max.x >= x0 && subRect.max.x <= x1
                && subRect.min.y >= y0 && subRect.min.y <= y1
                && subRect.max.y >= y0 && subRect.max.y <= y1;
        }

        public static Rect CalculateRelativeRect(Transform localRect, RectTransform relativeRect)
        {
            if (localRect == null || relativeRect == null)
                return default(Rect);
            if (localRect == relativeRect)
                return relativeRect.rect;
            Matrix4x4 m = localRect.worldToLocalMatrix * relativeRect.localToWorldMatrix;
            Rect rect = relativeRect.rect;
            Vector2 min = m.MultiplyPoint(rect.min);
            Vector2 max = m.MultiplyPoint(rect.max);
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        // width / height
        public static float TextureRatio(Texture tex)
        {
            return tex.width / (float)tex.height;
        }

        public static Bounds ScreenBoundsRelativeTo(this Camera c, Transform parent)
        {
            if (!c)
                return default(Bounds);
            Vector3 min = c.ScreenToWorldPoint(Vector3.zero);
            Vector3 max = c.ScreenToWorldPoint(new Vector3(c.pixelWidth, c.pixelHeight));
            if (parent)
            {
                Matrix4x4 m = parent.worldToLocalMatrix;
                min = m.MultiplyPoint(min);
                max = m.MultiplyPoint(max);
            }
            Bounds b = new Bounds((min + max) * 0.5f, (max - min));
            return b;
        }

    }
}

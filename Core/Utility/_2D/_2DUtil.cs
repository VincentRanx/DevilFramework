using Boo.Lang;
using UnityEngine;

namespace DevilTeam.Utility
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
   
  
    public class _2DUtil
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
    }
}

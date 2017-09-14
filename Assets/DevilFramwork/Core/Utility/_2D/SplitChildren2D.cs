using UnityEngine;

namespace DevilTeam.Utility
{
    public enum GathorType
    {
        gathor,
        dispersed,
    }


    [System.Serializable]
    public class SplitChildren2D
    {

        public enum LimitType
        {
            none,
            keepRow,
            keepCol,
            limitRow,
            limitCol,
        }

        public GathorType m_GathorType;
        public Vector2 m_RangeSize;
        public UIPivot m_Align;
        public LimitType m_RowColLimit;
        public int m_LimitValue;
        public Vector2 m_MaxCellSize;

        void GetRowCol(int count, out int row, out int col)
        {
            if (m_RowColLimit == LimitType.keepCol && m_LimitValue > 0)
            {
                col = m_LimitValue;
                row = count / col;
                if (count % col > 0)
                    row++;
            }
            else if (m_RowColLimit == LimitType.keepRow && m_LimitValue > 0)
            {
                row = m_LimitValue;
                col = count / row;
                if (count % row > 0)
                    col++;
            }
            else
            {
                if (m_RangeSize.y == 0f)
                {
                    col = count;
                    row = 1;
                }
                else if (m_RangeSize.x == 0f)
                {
                    col = 1;
                    row = count;
                }
                else
                {
                    float pow2 = Mathf.Abs(count * m_RangeSize.x / m_RangeSize.y);
                    col = Mathf.CeilToInt(Mathf.Sqrt(pow2));
                    row = count / col;
                    if (count % col != 0)
                        row++;
                }
                if (m_RowColLimit == LimitType.limitCol && m_LimitValue > 0 && m_LimitValue < col)
                {
                    col = m_LimitValue;
                    row = count / col;
                    if (count % col > 0)
                        row++;
                }
                else if (m_RowColLimit == LimitType.limitRow && m_LimitValue > 0 && m_LimitValue < row)
                {
                    row = m_LimitValue;
                    col = count / row;
                    if (count % row > 0)
                        col++;
                }
            }
        }

        public Vector3[] GetChildrenPosition(int count)
        {
            Vector3[] vs = new Vector3[count];
            if (count < 1)
                return vs;
            int col, row;
            GetRowCol(count, out row, out col);
            int empty = row * col - count;
            Vector2 unit;
            unit.x = m_RangeSize.x / (float)col;
            if (m_MaxCellSize.x > 0 && m_MaxCellSize.x < unit.x)
                unit.x = m_MaxCellSize.x;
            unit.y = m_RangeSize.y / (float)row;
            if (m_MaxCellSize.y > 0 && m_MaxCellSize.y < unit.y)
                unit.y = m_MaxCellSize.y;
            Vector2 START, END;
            Vector2 start, end;
            GetValueRange(new Vector2(unit.x * col, unit.y * row), out START, out END);
            GetValueRange(new Vector2((col - empty) * unit.x, unit.y * row), out start, out end);
            for (int i = 0; i < count; i++)
            {

                int r = i / col;
                int c = i % col;
                float realC = r == row - 1 ? (col - empty) : col;
                vs[i].x = (1f / realC) * ((float)c + 0.5f);//x lerp value
                vs[i].y = (1f / (float)row) * ((float)r + 0.5f); //y lerp value
                if (realC < col)
                    vs[i].x = Mathf.Lerp(start.x, end.x, vs[i].x);
                else
                    vs[i].x = Mathf.Lerp(START.x, END.x, vs[i].x);
                vs[i].y = Mathf.Lerp(start.y, end.y, vs[i].y);
            }
            return vs;
        }

        void GetValueRange(Vector2 size, out Vector2 start, out Vector2 end)
        {
            float xDirect = _2DUtil.AlignRight(m_Align) ? -1f : 1f;
            float yDirect = _2DUtil.AlignTop(m_Align) ? -1f : 1f;
            Vector2 useSize = m_GathorType == GathorType.gathor ? size : m_RangeSize;
            Vector2 alignSize = new Vector2(_2DUtil.AlignHCenter(m_Align) ? useSize.x : m_RangeSize.x,
                _2DUtil.AlignVCenter(m_Align) ? useSize.y : m_RangeSize.y);
            start.x = -alignSize.x * 0.5f * xDirect;
            end.x = start.x + useSize.x * xDirect;
            start.y = -alignSize.y * 0.5f * yDirect;
            end.y = start.y + useSize.y * yDirect;
        }

        public void DrawGizmos(Transform root, int pointsCount, Color c, Vector3 offset)
        {
            if (!root)
                return;
            Matrix4x4 m = Gizmos.matrix;
            Gizmos.matrix = root.localToWorldMatrix;
            Gizmos.color = c * 0.5f;
            Gizmos.DrawCube(offset, m_RangeSize);
            Gizmos.color = c;
            Vector3[] p = GetChildrenPosition(pointsCount);
            for (int i = 0; i < p.Length; i++)
            {
                GizmosUtil.MarkInScene(Gizmos.matrix.MultiplyPoint(p[i] + offset), 10f, 0f);
            }
            Gizmos.matrix = m;
        }
    }
}
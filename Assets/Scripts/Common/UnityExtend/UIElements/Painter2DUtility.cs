using UnityEngine;
using UnityEngine.UIElements;

namespace Common.UnityExtend.UIElements
{
    public static class Painter2DUtility
    {
        public static void DrawCrossSign(Painter2D painter, Vector2 pos, float size, Color strokeColor, float strokeSize = 2f)
        {
            var topLeft = pos + Vector2.up * size + Vector2.left * size;
            var bottomRight = pos + Vector2.down * size + Vector2.right * size;
            var topRight = pos + Vector2.up * size + Vector2.right * size;
            var bottomLeft = pos + Vector2.down * size + Vector2.left * size;

            var prevColor = painter.strokeColor;
            var prevLineWidth = painter.lineWidth;
            painter.strokeColor = strokeColor;
            painter.lineWidth = strokeSize;

            painter.BeginPath();
            painter.MoveTo(topLeft);
            painter.LineTo(bottomRight);
            painter.MoveTo(topRight);
            painter.LineTo(bottomLeft);
            painter.Stroke();

            painter.strokeColor = prevColor;
            painter.lineWidth = prevLineWidth;

        }
        public static void DrawRect(Painter2D painter, Rect rect, Color strokeColor, float strokeSize = 2f)
        {
            var topLeft = new Vector2(rect.xMin, rect.yMin);
            var bottomLeft = new Vector2(rect.xMin, rect.yMax);
            var bottomRight = new Vector2(rect.xMax, rect.yMax);
            var topRight = new Vector2(rect.xMax, rect.yMin);

            var prevColor = painter.strokeColor;
            var prevLineWidth = painter.lineWidth;
            painter.strokeColor = strokeColor;
            painter.lineWidth = strokeSize;

            painter.BeginPath();
            painter.MoveTo(topLeft);
            painter.LineTo(bottomLeft);
            painter.LineTo(bottomRight);
            painter.LineTo(topRight);
            painter.LineTo(topLeft);
            painter.Stroke();

            painter.strokeColor = prevColor;
            painter.lineWidth = prevLineWidth;
        }
    }
}
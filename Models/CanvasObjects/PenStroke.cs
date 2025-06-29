// C:\Users\kikuchi\Dropbox\PaintTool\Models\CanvasObjects\PenStroke.cs

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json.Serialization;


namespace PaintTool
{
    public class PenStroke : CanvasObject
    {// publicなプロパティに変更し、setterをinitにする
        public List<Point> Points { get; init; }
        public Color Color { get; init; }
        public float Width { get; init; }

        // JSONデシリアライズ用のコンストラクタ
        [JsonConstructor]
        public PenStroke(List<Point> points, Color color, float width)
        {
            Points = points;
            Color = color;
            Width = width;
        }

        public PenStroke(Color color, float width)
        {
            Points = new List<Point>();
            Color = color;
            Width = width;
        }

        public void AddPoint(Point point)
        {
            Points.Add(point);
        }

        public override void Draw(Graphics g)
        {
            if (Points.Count < 2) return;

            using (var pen = new Pen(Color, Width))
            {
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                g.DrawLines(pen, Points.ToArray());
            }
        }

        public override bool HitTest(Point point)
        {
            return GetBounds().Contains(point);
        }

        // Moveメソッドの引数を float に変更
        public override void Move(float dx, float dy)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] = new Point((int)(Points[i].X + dx), (int)(Points[i].Y + dy));
            }
        }

        // GetBoundsの戻り値を RectangleF に変更
        public override RectangleF GetBounds()
        {
            if (Points.Count == 0) return RectangleF.Empty;

            var minX = Points.Min(p => p.X);
            var minY = Points.Min(p => p.Y);
            var maxX = Points.Max(p => p.X);
            var maxY = Points.Max(p => p.Y);

            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
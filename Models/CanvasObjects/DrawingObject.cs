// PaintTool/Models/CanvasObjects/DrawingObject.cs (修正後)

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json.Serialization; // ★ using を追加

namespace PaintTool
{
    public class DrawingObject : CanvasObject
    {
        public List<PenStroke> Strokes { get; set; }

        // プログラム上で新規作成する際に使うコンストラクタ
        public DrawingObject()
        {
            Strokes = new List<PenStroke>();
        }

        // JSONデシリアライズ用のコンストラクタ
        [JsonConstructor]
        public DrawingObject(List<PenStroke> strokes)
        {
            Strokes = strokes;
        }

        public override bool CanSelect => false;

        public override void Draw(Graphics g)
        {
            foreach (var stroke in Strokes)
            {
                stroke.Draw(g);
            }
        }

        public override bool HitTest(Point point)
        {
            return Strokes.Any(s => s.HitTest(point));
        }

        public void RemoveStrokesNear(Point point, float radius)
        {
            Strokes.RemoveAll(stroke =>
            {
                var bounds = stroke.GetBounds();
                var eraserRect = new RectangleF(point.X - radius, point.Y - radius, radius * 2, radius * 2);
                return bounds.IntersectsWith(eraserRect);
            });
        }
    }
}
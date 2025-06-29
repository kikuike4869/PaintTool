// PaintTool/Models/CanvasObjects/TextObject.cs (修正後)

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Serialization;

namespace PaintTool
{
    public class TextObject : TransformableObject
    {
        // public set を init に変更
        public string Text { get; set; }
        public Font Font { get; set; }
        public Color Color { get; set; }

        // JSONデシリアライズ用のコンストラクタ
        // ベースクラスのプロパティも引数で受け取る
        [JsonConstructor]
        public TextObject(string text, Font font, Color color, PointF location, SizeF size, float angle, float scale)
        {
            Text = text;
            Font = font;
            Color = color;
            Location = location;
            Size = size;
            Angle = angle;
            Scale = scale;
        }

        // プログラム上で新規作成する際に使うコンストラクタ
        public TextObject(string text, PointF location, Font font, Color color)
        {
            Text = text;
            Location = location;
            Font = font;
            Color = color;
            Angle = 0f;
            Scale = 1f;

            // テキストに基づいてサイズを計算
            UpdateSize();
        }

        private void UpdateSize()
        {
            if (string.IsNullOrEmpty(Text))
            {
                Size = new SizeF(10, 10);
                return;
            }
            using (var g = Graphics.FromImage(new Bitmap(1, 1)))
            {
                Size = g.MeasureString(Text, Font);
            }
        }

        public override void Draw(Graphics g, Matrix transform)
        {
            var state = g.Save();
            g.Transform = transform;
            using (var brush = new SolidBrush(Color))
            {
                g.DrawString(Text, Font, brush, Location);
            }
            g.Restore(state);
        }
    }
}
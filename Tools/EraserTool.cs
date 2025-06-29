using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PaintTool
{
    public class EraserTool : ITool
    {
        private PictureBox pictureBox;
        private Canvas canvas; // Canvasクラスへの参照
        private bool isErasing = false;

        public float EraserWidth { get; set; } = 10f;

        public EraserTool(PictureBox pictureBox, Canvas canvas)
        {
            this.pictureBox = pictureBox;
            this.canvas = canvas;
        }
        public void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isErasing = true;
                EraseAt(e.Location);
            }
        }

        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isErasing)
            {
                EraseAt(e.Location);
            }
        }

        public void OnMouseUp(object sender, MouseEventArgs e)
        {
            isErasing = false;
        }

        private void EraseAt(Point location)
        {
            bool changed = false;
            // CanvasからDrawingObjectを取得して処理
            foreach (var drawingObject in canvas.GetCanvasObjects().OfType<DrawingObject>())
            {
                int initialCount = drawingObject.Strokes.Count;
                drawingObject.RemoveStrokesNear(location, EraserWidth / 2);
                if (initialCount != drawingObject.Strokes.Count)
                {
                    changed = true;
                }
            }

            if (changed)
            {
                pictureBox.Invalidate();
            }
        }

        public void DrawPreview(Graphics g)
        {
            // 必要であれば消しゴムのカーソルなどを描画
        }
    }
}
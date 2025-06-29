// Tools/PenTool.cs (修正・または新規作成)
using System.Drawing;
using System.Windows.Forms;

namespace PaintTool
{
    public class PenTool : ITool
    {
        private readonly Canvas _canvas;
        private readonly PictureBox _pictureBox;
        
        // 現在描画中の線(PenStroke)を保持する
        private PenStroke? _currentStroke;
        // 全ての線(PenStroke)を格納する親オブジェクト
        private DrawingObject? _drawingObject;

        public Color PenColor { get; set; } = Color.Black;
        public float PenWidth { get; set; } = 2f;

        public PenTool(PictureBox pictureBox, Canvas canvas)
        {
            _pictureBox = pictureBox;
            _canvas = canvas;
        }

        public void OnMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // 1. まず、手書き用の親オブジェクトを取得または新規作成する
                _drawingObject = _canvas.GetOrCreateDrawingObject();

                // 2. ★★★ ここが最重要ポイント ★★★
                // もし親オブジェクトがまだキャンバスになければ、ここで追加する
                // これにより、この後の描画がすべて保存されるようになる
                if (!_canvas.GetCanvasObjects().Contains(_drawingObject))
                {
                    // この操作はUndo/Redoの対象となる
                    _canvas.AddObject(_drawingObject);
                }

                // 3. 新しい線を作成し、親オブジェクトに追加する
                _currentStroke = new PenStroke(PenColor, PenWidth);
                _drawingObject.Strokes.Add(_currentStroke);

                // 4. 最初の点を追加する
                _currentStroke.AddPoint(e.Location);

                // 再描画を要求
                _pictureBox.Invalidate();
            }
        }

        public void OnMouseMove(object? sender, MouseEventArgs e)
        {
            // マウスボタンが押されていて、かつ描画中の線があれば点を追加
            if (e.Button == MouseButtons.Left && _currentStroke != null)
            {
                _currentStroke.AddPoint(e.Location);
                _pictureBox.Invalidate();
            }
        }

        public void OnMouseUp(object? sender, MouseEventArgs e)
        {
            // 描画中の線をクリアする（描画処理の終了）
            _currentStroke = null;
        }

        public void DrawPreview(Graphics g)
        {
            // このツールには特別なプレビューはないので、何もしない
        }
    }
}
using System.Drawing;
using System.Windows.Forms;

namespace PaintTool
{
    public interface ITool
    {
        // ControllerからCanvas(Model)とUIイベント情報を受け取る
        void OnMouseDown(object sender, MouseEventArgs e);
        void OnMouseMove(object sender, MouseEventArgs e);
        void OnMouseUp(object sender, MouseEventArgs e);
        void DrawPreview(Graphics g);
    }

    // ツールが必要とする「状態」をまとめたコンテキストクラス
    public class ToolContext
    {
        public Color PrimaryColor { get; set; }
        public int BrushSize { get; set; }
    }
}
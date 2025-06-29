using System.Drawing;
using System.Windows.Forms;

namespace PaintTool
{
    public interface ITool
    {
        void OnMouseDown(object sender, MouseEventArgs e);
        void OnMouseMove(object sender, MouseEventArgs e);
        void OnMouseUp(object sender, MouseEventArgs e);
        void DrawPreview(Graphics g); 
    }
}
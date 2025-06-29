using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PaintTool.Commands;

namespace PaintTool
{
    public enum HandleType
    {
        None,
        Body,
        Resize_TopLeft, Resize_TopRight, Resize_BottomLeft, Resize_BottomRight,
        Rotate
    }

    public class SelectTool : ITool
    {
        private PictureBox pictureBox;
        private Canvas canvas;
        private TransformableObject? selectedObject;
        private Point lastMousePosition;
        private bool isTextInsertMode = false;

        private enum DragMode { None, Move, Resize, Rotate }
        private DragMode currentDragMode = DragMode.None;
        private HandleType activeHandle = HandleType.None;

        private TransformObjectCommand? pendingTransformCommand;

        public SelectTool(PictureBox pictureBox, Canvas canvas)
        {
            this.pictureBox = pictureBox;
            this.canvas = canvas;
        }

        public void SetTextInsertMode(bool enabled)
        {
            isTextInsertMode = enabled;
        }

        public void OnMouseDown(object? sender, MouseEventArgs e)
        {
            if (isTextInsertMode)
            {
                CreateTextObject(e.Location, pictureBox);
                isTextInsertMode = false; // 一度挿入したらモードを解除
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                if (selectedObject != null)
                {
                    activeHandle = selectedObject.HitTestHandles(e.Location);
                    if (currentDragMode != DragMode.None && activeHandle != HandleType.None)
                    {
                        currentDragMode = activeHandle == HandleType.Rotate ? DragMode.Rotate : DragMode.Resize;
                        lastMousePosition = e.Location;
                        TransformState beforeState = new TransformState(
                            selectedObject.Location,
                            selectedObject.Size,
                            selectedObject.Angle,
                            selectedObject.Scale
                            );
                        pendingTransformCommand = new TransformObjectCommand(selectedObject, beforeState);
                        pictureBox.Invalidate();
                        return;
                    }
                }

                DeselectAll();
                selectedObject = null;

                // 後ろにあるオブジェクトから順に当たり判定
                foreach (var obj in canvas.GetCanvasObjects().Reverse())
                {
                    if (obj is TransformableObject tObj)
                    {
                        if (tObj.HitTest(e.Location))
                        {
                            selectedObject = tObj;
                            selectedObject.IsSelected = true;
                            currentDragMode = DragMode.Move;
                            lastMousePosition = e.Location;
                            break;
                        }
                    }
                }
                pictureBox.Invalidate();
            }
        }

        public void OnMouseMove(object? sender, MouseEventArgs e)
        {
            if (currentDragMode == DragMode.None || selectedObject == null)
            {
                if (selectedObject != null)
                {
                    var handle = selectedObject.HitTestHandles(e.Location);
                    pictureBox.Cursor = handle switch
                    {
                        HandleType.Rotate => Cursors.Hand,
                        HandleType.None => Cursors.Default,
                        _ => Cursors.SizeNWSE,
                    };
                }
                else
                {
                    pictureBox.Cursor = Cursors.Default;
                }
                return;
            }

            switch (currentDragMode)
            {
                case DragMode.Move:
                    float dx = e.X - lastMousePosition.X;
                    float dy = e.Y - lastMousePosition.Y;
                    selectedObject.Move(dx, dy);
                    break;
                case DragMode.Rotate:
                    RotateObject(e.Location);
                    break;
                case DragMode.Resize:
                    ResizeObject(e.Location);
                    break;
            }

            lastMousePosition = e.Location;
            pictureBox.Invalidate();
        }

        public void OnMouseUp(object? sender, MouseEventArgs e)
        {
            if (pendingTransformCommand != null)
            {
                pendingTransformCommand.CaptureAfterState();
                canvas.ExecuteCommand(pendingTransformCommand);
                pendingTransformCommand = null;
            }
            
            currentDragMode = DragMode.None;
            activeHandle = HandleType.None;
        }

        private void RotateObject(Point mousePosition)
        {
            if (selectedObject is null) return; // ★修正：nullチェック
            var center = selectedObject.GetCenter();
            var angleRad = Math.Atan2(mousePosition.Y - center.Y, mousePosition.X - center.X);
            selectedObject.Angle = (float)(angleRad * (180.0 / Math.PI)) + 90;
        }

        private void ResizeObject(Point mousePosition)
        {
            if (selectedObject is null) return;
            var center = selectedObject.GetCenter();
            var originalDist = GetDistance(center, lastMousePosition);
            var newDist = GetDistance(center, mousePosition);

            if (originalDist > 0)
            {
                var scaleDelta = newDist / originalDist;
                selectedObject.Scale *= (float)scaleDelta;
            }
        }

        private double GetDistance(PointF p1, PointF p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        public void DrawPreview(Graphics g) { }

        public TransformableObject? GetSelectedObject()
        {
            return selectedObject;
        }

        public void DeselectAll()
        {
            foreach (var obj in canvas.GetCanvasObjects())
            {
                if (obj is TransformableObject tObj)
                {
                    tObj.IsSelected = false;
                }
            }
            selectedObject = null;
        }

        private void CreateTextObject(Point location, Control parent)
        {
            var textBox = new TextBox { Location = location, BorderStyle = BorderStyle.FixedSingle };
            textBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    var text = textBox.Text;
                    parent.Controls.Remove(textBox);
                    if (!string.IsNullOrEmpty(text))
                    {
                        var textObject = new TextObject(text, location, textBox.Font, textBox.ForeColor);
                        canvas.AddObject(textObject);
                        DeselectAll();
                        textObject.IsSelected = true;
                        selectedObject = textObject;
                        pictureBox.Invalidate();
                    }
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    parent.Controls.Remove(textBox);
                }
            };
            parent.Controls.Add(textBox);
            textBox.Focus();
        }
    }
}
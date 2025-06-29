// Models/Canvas.cs (修正後)
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json.Serialization;
using PaintTool.Commands; // ★ usingを追加

namespace PaintTool
{
    public class Canvas
    {
        public Size Size { get; private set; }
        public Color BackgroundColor { get; init; }
        public List<CanvasObject> CanvasObjects { get; private set; }
        // ★ Undo/Redoスタックを追加
        [JsonIgnore]
        private readonly List<ICommand> undoHistory = new List<ICommand>();
        [JsonIgnore]
        private readonly List<ICommand> redoHistory = new List<ICommand>();
        private const int HistoryLimit = 10;

        public Canvas(Size size, Color backgroundColor)
        {
            Size = size;
            BackgroundColor = backgroundColor;
            CanvasObjects = new List<CanvasObject>();
        }

        [JsonConstructor]
        public Canvas(Size size, Color backgroundColor, List<CanvasObject> canvasObjects)
        {
            Size = size;
            BackgroundColor = backgroundColor;
            CanvasObjects = canvasObjects;
        }

        public void UpdateSize(Size newSize)
        {
            Size = newSize;
        }

        /// <summary>
        /// コマンドを実行し、Undo履歴に追加する
        /// </summary>
        /// <param name="command">実行されるコマンド</param>
        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            undoHistory.Add(command);

            // 履歴がHistoryLimitを超えたら最も古いものを削除
            if (undoHistory.Count > HistoryLimit)
            {
                undoHistory.RemoveAt(0);
            }

            // 新しい操作をしたらRedo履歴はクリア
            redoHistory.Clear();
        }

        public void AddObject(CanvasObject obj)
        {
            var command = new AddObjectCommand(this, obj);
            ExecuteCommand(command);
        }

        public void RemoveObject(CanvasObject obj)
        {
            var command = new RemoveObjectCommand(this, obj);
            ExecuteCommand(command);
        }

        internal void AddObjectInternal(CanvasObject obj)
        {
            CanvasObjects.Add(obj);
        }

        internal void RemoveObjectInternal(CanvasObject obj)
        {
            CanvasObjects.Remove(obj);
        }

        public void Draw(Graphics g)
        {
            using (var brush = new SolidBrush(BackgroundColor))
            {
                g.FillRectangle(brush, 0, 0, Size.Width, Size.Height);
            }
            foreach (var obj in CanvasObjects)
            {
                obj.Draw(g);
            }
        }

        public IReadOnlyList<CanvasObject> GetCanvasObjects()
        {
            return CanvasObjects.AsReadOnly();
        }

        public DrawingObject GetOrCreateDrawingObject()
        {
            var drawingObject = CanvasObjects.LastOrDefault(obj => obj is DrawingObject) as DrawingObject;
            if (drawingObject == null)
            {
                drawingObject = new DrawingObject();
                // ★ ここでは直接追加せず、PenTool側でAddObjectを呼ぶように変更
            }
            return drawingObject;
        }

        /// <summary>
        /// 最後に行った操作を取り消す
        /// </summary>
        public void Undo()
        {
            if (undoHistory.Count > 0)
            {
                var command = undoHistory.Last();
                undoHistory.RemoveAt(undoHistory.Count - 1);

                command.Unexecute();
                redoHistory.Add(command);
            }
        }

        /// <summary>
        /// 取り消した操作をやり直す
        /// </summary>
        public void Redo()
        {
            if (redoHistory.Count > 0)
            {
                var command = redoHistory.Last();
                redoHistory.RemoveAt(redoHistory.Count - 1);

                ExecuteCommand(command);
            }
        }
    }
}
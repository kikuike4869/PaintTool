// Controllers/CanvasController.cs (改訂版)
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PaintTool.Tools;

namespace PaintTool
{
    public class CanvasController
    {
        private readonly CanvasEditScreen _view;
        private readonly Painting _model;

        // 利用可能なツール群を保持
        private readonly Dictionary<string, ITool> _tools;
        // 現在アクティブなツール
        private ITool _currentTool;

        // ツールが必要とする状態(Context)
        private readonly ToolContext _toolContext;

        public CanvasController(CanvasEditScreen view, Painting model)
        {
            _view = view;
            _model = model;

            // ツール群を初期化して登録
            _tools = new Dictionary<string, ITool>
            {
                { "Pen", new PenTool() },
                { "Eraser", new EraserTool() }, // 他のツールも同様に追加
                { "Rectangle", new RectangleTool() },
            };

            // アプリケーションの状態を初期化
            _toolContext = new ToolContext
            {
                PrimaryColor = Color.Black,
                BrushSize = 2
            };

            // デフォルトのツールを選択
            SelectTool("Pen");
        }

        // --- Viewからのイベントを処理するハンドラ ---

        public void HandleMouseDown(MouseEventArgs e)
        {
            _currentTool?.MouseDown(_model.Canvas, e, _toolContext);
        }

        public void HandleMouseMove(MouseEventArgs e)
        {
            // MouseMoveは頻繁に呼ばれるので、変更があった場合のみ再描画
            var shapeCountBefore = _model.Canvas.Shapes.Count;

            _currentTool?.MouseMove(_model.Canvas, e, _toolContext);

            // 図形が追加されたらViewに再描画を依頼
            if (_model.Canvas.Shapes.Count > shapeCountBefore)
            {
                _view.RefreshCanvas();
            }
        }

        public void HandleMouseUp(MouseEventArgs e)
        {
            _currentTool?.MouseUp(_model.Canvas, e, _toolContext);
            // 念のためMouseUp後も再描画
            _view.RefreshCanvas();
        }

        public void HandlePaint(PaintEventArgs e)
        {
            _model.Canvas.Draw(e.Graphics);
        }

        // --- Viewからアプリケーションの状態を変更するためのメソッド ---

        public void SelectTool(string toolName)
        {
            if (_tools.ContainsKey(toolName))
            {
                _currentTool = _tools[toolName];
            }
        }

        public void ChangePrimaryColor(Color color)
        {
            _toolContext.PrimaryColor = color;
        }

        public void ChangeBrushSize(int size)
        {
            _toolContext.BrushSize = size;
        }
    }
}
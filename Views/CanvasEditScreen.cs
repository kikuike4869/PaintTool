using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintTool
{
    // Views/CanvasEditScreen.cs
    public partial class CanvasEditScreen : UserControl
    {
        // 自分の振る舞いを担当するコントローラーを保持する
        private readonly CanvasController _controller;
        private ToolStrip toolStrip = null!;
        private ToolStripButton selectToolButton = null!;
        private ToolStripButton penToolButton = null!;
        private ToolStripButton eraserToolButton = null!;
        private ToolStripDropDownButton insertToolButton = null!;
        private ToolStripMenuItem insertTextMenuItem = null!;
        private ToolStripMenuItem insertImageMenuItem = null!;
        private SplitContainer splitContainer = null!; // ★追加
        private ImageListPanel imageListPanel = null!; // ★追加
        private Button addImageButton = null!; // ★追加

        private Panel canvasContainer = null!;
        private PictureBox pictureBox = null!;
        private Canvas canvas = null!;
        private ITool currentTool = null!;
        private SelectTool selectTool = null!;
        private PenTool penTool = null!;
        private EraserTool eraserTool = null!;

        private ToolStripButton penColorButton = null!;
        private ToolStripLabel penWidthLabel = null!;
        private ToolStripComboBox penWidthComboBox = null!;
        private Panel penColorPreview = null!;
        private ToolStripLabel eraserWidthLabel = null!;
        private ToolStripComboBox eraserWidthComboBox = null!;

        private readonly double aspectRatio;

        private ToolStripButton saveButton = null!;
        private readonly Album album;


        // コンストラクタ
        public CanvasEditScreen(Painting paintingModel) // 上位コントローラからModelを受け取る
        {
            InitializeComponent();

            // ★★★ ここでCanvasControllerのインスタンスが生成される ★★★
            // 自分自身(this)と、受け取ったModelを渡してControllerを初期化する
            _controller = new CanvasController(this, paintingModel);

            // あとは、UIイベントをControllerのメソッドに繋ぎ込むだけ
            this.canvasPanel.MouseDown += (s, e) => _controller.HandleMouseDown(e);
            this.canvasPanel.MouseMove += (s, e) => _controller.HandleMouseMove(e);
            this.canvasPanel.MouseUp += (s, e) => _controller.HandleMouseUp(e);
            this.canvasPanel.Paint += (s, e) => _controller.HandlePaint(e);
        }


        // Controllerから再描画を指示されるためのメソッド
        public void RefreshCanvas()
        {
            this.canvasPanel.Invalidate();
        }
    }
}
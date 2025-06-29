using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintTool
{
    public class CanvasEditScreen : UserControl
    {
        private ToolStrip toolStrip = null!;
        private ToolStripButton selectToolButton = null!;
        private ToolStripButton penToolButton = null!;
        private ToolStripButton eraserToolButton = null!;
        private ToolStripDropDownButton insertToolButton = null!;
        private ToolStripMenuItem insertTextMenuItem = null!;
        private ToolStripMenuItem insertImageMenuItem = null!;

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

        public CanvasEditScreen(Album album)
        {
            InitializeComponent();
            this.album = album;
            this.canvas = album.Canvas;
            this.aspectRatio = (double)canvas.Size.Width / canvas.Size.Height;

            this.SizeChanged += (s, e) => SetupCanvasLayout();
            InitializeTools();
            SetupCanvasLayout();

            this.KeyDown += CanvasEditScreen_KeyDown;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.toolStrip = new ToolStrip { Dock = DockStyle.Top, GripStyle = ToolStripGripStyle.Hidden };
            this.selectToolButton = new ToolStripButton("選択");
            this.penToolButton = new ToolStripButton("ペン");
            this.eraserToolButton = new ToolStripButton("消しゴム");
            this.insertTextMenuItem = new ToolStripMenuItem("テキスト");
            this.insertImageMenuItem = new ToolStripMenuItem("画像");
            this.insertToolButton = new ToolStripDropDownButton("挿入", null, insertTextMenuItem, insertImageMenuItem);
            this.toolStrip.Items.Add(selectToolButton);
            this.toolStrip.Items.Add(penToolButton);
            this.toolStrip.Items.Add(eraserToolButton);
            this.toolStrip.Items.Add(insertToolButton);
            this.toolStrip.Items.Add(new ToolStripSeparator());
            this.penColorPreview = new Panel { Size = new Size(16, 16), BackColor = Color.Black, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(2, 2, 2, 1) };
            this.penColorButton = new ToolStripButton("ペンの色");
            this.penWidthLabel = new ToolStripLabel("太さ:");
            this.penWidthComboBox = new ToolStripComboBox();
            this.penWidthComboBox.Items.AddRange(new object[] { "1", "2", "5", "10", "20" });
            this.penWidthComboBox.Text = "2";
            this.toolStrip.Items.Add(penColorButton);
            var host = new ToolStripControlHost(penColorPreview);
            this.toolStrip.Items.Add(host);
            this.toolStrip.Items.Add(penWidthLabel);
            this.toolStrip.Items.Add(penWidthComboBox);
            this.toolStrip.Items.Add(new ToolStripSeparator());
            this.eraserWidthLabel = new ToolStripLabel("消しゴムの太さ:");
            this.eraserWidthComboBox = new ToolStripComboBox();
            this.eraserWidthComboBox.Items.AddRange(new object[] { "5", "10", "20", "40", "60" });
            this.eraserWidthComboBox.Text = "10";
            this.toolStrip.Items.Add(eraserWidthLabel);
            this.toolStrip.Items.Add(eraserWidthComboBox);
            this.canvasContainer = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = SystemColors.ControlDark };
            this.pictureBox = new PictureBox { Location = new Point(0, 0) };
            this.canvasContainer.Controls.Add(pictureBox);
            this.Controls.Add(this.canvasContainer);
            this.Controls.Add(this.toolStrip);

            this.saveButton = new ToolStripButton("保存");
            this.toolStrip.Items.Add(new ToolStripSeparator());
            this.toolStrip.Items.Add(this.saveButton);
            this.saveButton.Click += SaveButton_Click;

            this.selectToolButton.Click += (s, e) => SetTool(selectTool);
            this.penToolButton.Click += (s, e) => SetTool(penTool);
            this.eraserToolButton.Click += (s, e) => SetTool(eraserTool);
            this.insertTextMenuItem.Click += InsertTextMenuItem_Click;
            this.insertImageMenuItem.Click += InsertImageMenuItem_Click;
            this.pictureBox.MouseDown += (s, e) => currentTool?.OnMouseDown(s!, e);
            this.pictureBox.MouseMove += (s, e) => currentTool?.OnMouseMove(s!, e);
            this.pictureBox.MouseUp += (s, e) => currentTool?.OnMouseUp(s!, e);
            this.pictureBox.Paint += PictureBox_Paint;
            this.penColorButton.Click += PenColorButton_Click;
            this.penWidthComboBox.TextChanged += PenWidthComboBox_TextChanged;
            this.eraserWidthComboBox.TextChanged += EraserWidthComboBox_TextChanged;
            this.Load += (s, e) => this.Focus();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            // 1. 現在のPictureBoxの内容からプレビュー画像を生成します
            //    pictureBoxは、キャンバスを表示しているUserControl内のPictureBoxコントロールを指します
            Bitmap previewBitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
            pictureBox.DrawToBitmap(previewBitmap, new Rectangle(0, 0, pictureBox.Width, pictureBox.Height));

            // 2. プレビュー画像を渡して保存を実行します
            try
            {
                // ★修正点: previewBitmap を引数として渡します
                album.Save(previewBitmap);
                MessageBox.Show($"アルバム '{album.Name}' を保存しました。", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存中にエラーが発生しました: " + ex.Message, "保存エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 3. 作成したBitmapリソースは必ず解放します
                previewBitmap.Dispose();
            }
        }

        private void CanvasEditScreen_KeyDown(object? sender, KeyEventArgs e)
        {
            // Undo: Ctrl + Z
            if (e.Control && e.KeyCode == Keys.Z)
            {
                Console.WriteLine("Execute redo");
                canvas.Undo();
                pictureBox.Invalidate();
                e.Handled = true; // イベントが処理されたことを示す
            }
            // Redo: Ctrl + Y
            else if (e.Control && e.KeyCode == Keys.Y)
            {
                canvas.Redo();
                pictureBox.Invalidate();
                e.Handled = true;
            }
            // Delete: Deleteキー
            else if (e.KeyCode == Keys.Delete)
            {
                // SelectToolが選択しているオブジェクトを取得して削除
                var selectedObject = selectTool.GetSelectedObject(); // SelectToolにメソッドを追加する必要がある
                if (selectedObject != null)
                {
                    canvas.RemoveObject(selectedObject);
                    selectTool.DeselectAll(); // 削除後に選択解除
                    pictureBox.Invalidate();
                    e.Handled = true;
                }
            }
        }

        private void SetupCanvasLayout()
        {
            int parentWidth = canvasContainer.ClientSize.Width;
            int parentHeight = canvasContainer.ClientSize.Height;
            int width, height;
            if (parentWidth / aspectRatio < parentHeight)
            {
                width = parentWidth;
                height = (int)(width / aspectRatio);
            }
            else
            {
                height = parentHeight;
                width = (int)(height * aspectRatio);
            }
            canvas.UpdateSize(new Size(width, height));
            this.pictureBox.Size = canvas.Size;
            this.pictureBox.Image?.Dispose();
            this.pictureBox.Image = new Bitmap(canvas.Size.Width, canvas.Size.Height);
            this.pictureBox.Location = new Point((parentWidth - width) / 2, (parentHeight - height) / 2);
            pictureBox.Invalidate();
        }

        private void InitializeTools()
        {
            this.selectTool = new SelectTool(pictureBox, this.canvas);
            this.penTool = new PenTool(pictureBox, this.canvas);
            this.eraserTool = new EraserTool(pictureBox, this.canvas);
            SetTool(this.penTool);
        }

        private void SetTool(ITool tool)
        {
            currentTool = tool;
            penToolButton.Checked = (tool == penTool);
            selectToolButton.Checked = (tool == selectTool);
            eraserToolButton.Checked = (tool == eraserTool);
        }

        private void PictureBox_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            canvas.Draw(e.Graphics);
            currentTool?.DrawPreview(e.Graphics);
        }

        private void PenColorButton_Click(object? sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    penTool.PenColor = colorDialog.Color;
                    penColorPreview.BackColor = colorDialog.Color;
                }
            }
        }

        private void PenWidthComboBox_TextChanged(object? sender, EventArgs e)
        {
            if (float.TryParse(penWidthComboBox.Text, out float width))
            {
                penTool.PenWidth = width;
            }
        }

        private void EraserWidthComboBox_TextChanged(object? sender, EventArgs e)
        {
            if (float.TryParse(eraserWidthComboBox.Text, out float width))
            {
                eraserTool.EraserWidth = width;
            }
        }

        private void InsertTextMenuItem_Click(object? sender, EventArgs e)
        {
            selectTool.SetTextInsertMode(true);
            SetTool(selectTool);
            MessageBox.Show("テキストを挿入したい位置をクリックしてください。");
        }

        private void undoMenuItem_Click(object sender, EventArgs e)
        {
            canvas.Undo();
            pictureBox.Invalidate();
        }

        private void redoMenuItem_Click(object sender, EventArgs e)
        {
            canvas.Redo();
            pictureBox.Invalidate();
        }

        private void InsertImageMenuItem_Click(object? sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // ★ 修正点: ファイルパスを直接コンストラクタに渡す
                        var imageObject = new ImageObject(openFileDialog.FileName, Point.Empty);

                        // ★ 修正点: オブジェクト作成後に、その画像のサイズを使って中央配置の座標を計算する
                        float dx = (canvas.Size.Width - imageObject.Size.Width) / 2;
                        float dy = (canvas.Size.Height - imageObject.Size.Height) / 2;
                        imageObject.Move(dx, dy);

                        canvas.AddObject(imageObject);
                        pictureBox.Invalidate();
                        SetTool(selectTool);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("画像の読み込みに失敗しました: " + ex.Message);
                    }
                }
            }
        }
    }
}
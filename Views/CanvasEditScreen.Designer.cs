using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintTool
{
    // Views/CanvasEditScreen.cs
    public partial class CanvasEditScreen : UserControl
    {
        private Panel canvasPanel;

        private void InitializeComponent()
        {
            this.canvasPanel = new Panel();
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

            this.KeyDown += CanvasEditScreen_KeyDown;

            // 1. SplitContainerの作成
            this.splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                FixedPanel = FixedPanel.Panel2, // 右側パネルの幅を固定
            };

            // 2. 右側パネルにImageListPanelと追加ボタンを配置
            var rightPanel = this.splitContainer.Panel2;
            rightPanel.BackColor = SystemColors.ControlLight;

            this.addImageButton = new Button
            {
                Text = "追加",
                Dock = DockStyle.Top,
                Height = 30,
                Margin = new Padding(5)
            };
            // this.addImageButton.Click += AddImageButton_Click; // ダイアログは後で実装

            this.imageListPanel = new ImageListPanel
            {
                Dock = DockStyle.Fill
            };

            rightPanel.Controls.Add(this.imageListPanel);
            rightPanel.Controls.Add(this.addImageButton);
            this.canvasContainer = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = SystemColors.ControlDark };
            this.pictureBox = new PictureBox { Location = new Point(0, 0) };
            this.canvasContainer.Controls.Add(pictureBox);
            this.splitContainer.Panel1.Controls.Add(this.canvasContainer);

            // 4. メインコントロールにSplitContainerとToolStripを追加
            this.Controls.Add(this.splitContainer);
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
            this.Load += CanvasEditScreen_Load;
            this.Load += (s, e) => this.Focus();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // Controllerから再描画を指示されるためのメソッド
        public void RefreshCanvas()
        {
            this.canvasPanel.Invalidate();
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

        private void CanvasEditScreen_Load(object? sender, EventArgs e)
        {
            // このタイミングでSplitterDistanceを設定する
            // 画面幅から200を引いた値が0より大きいことを確認
            if (this.Width > 200)
            {
                this.splitContainer.SplitterDistance = this.Width - 200;
            }
            else
            {
                // 幅が200以下の場合は、デフォルトの分割位置にするなど調整
                this.splitContainer.SplitterDistance = this.Width / 2;
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
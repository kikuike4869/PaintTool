using System;
using System.Drawing;
using System.Windows.Forms;

namespace PaintTool
{
    public class HomeScreen : UserControl
    {
        public event EventHandler<CanvasSettingsEventArgs> CreateCanvasClicked = null!;
        public event EventHandler<AlbumEventArgs> CreateNewAlbumClicked = null!;
        public event EventHandler<AlbumEventArgs> OpenAlbumClicked = null!;
        private RadioButton ratio43RadioButton = null!;
        private RadioButton ratio169RadioButton = null!;
        private RadioButton ratio11RadioButton = null!;
        private Button selectColorButton = null!;
        private Button createCanvasButton = null!;
        private TextBox albumNameTextBox = null!;
        private ListBox savedAlbumsListBox = null!;
        private Button openAlbumButton = null!;
        private Color selectedColor = Color.White;
        private Panel colorPreviewPanel = null!;

        public HomeScreen()
        {
            InitializeComponent();
            LoadSavedAlbums();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            var ratioLabel = new Label { Text = "アスペクト比", Location = new Point(50, 20), AutoSize = true };
            var colorLabel = new Label { Text = "背景色", Location = new Point(50, 150), AutoSize = true };
            this.ratio43RadioButton = new RadioButton { Text = "4:3", AutoSize = true, Location = new Point(50, 50), Checked = true };
            this.ratio169RadioButton = new RadioButton { Text = "16:9", AutoSize = true, Location = new Point(50, 80) };
            this.ratio11RadioButton = new RadioButton { Text = "1:1", AutoSize = true, Location = new Point(50, 110) };
            this.selectColorButton = new Button { Text = "色を選択", Location = new Point(50, 180) };
            this.selectColorButton.Click += SelectColorButton_Click;
            this.colorPreviewPanel = new Panel { Size = new Size(30, 30), Location = new Point(160, 180), BackColor = selectedColor, BorderStyle = BorderStyle.FixedSingle };
            this.createCanvasButton = new Button { Text = "キャンバスを作成", Location = new Point(50, 250), Size = new Size(150, 40) };
            this.createCanvasButton.Click += CreateCanvasButton_Click;
            this.Controls.Add(ratioLabel);
            this.Controls.Add(this.ratio43RadioButton);
            this.Controls.Add(this.ratio169RadioButton);
            this.Controls.Add(this.ratio11RadioButton);
            this.Controls.Add(colorLabel);
            this.Controls.Add(this.selectColorButton);
            this.Controls.Add(this.colorPreviewPanel);
            this.Controls.Add(this.createCanvasButton);


            // アルバム名入力
            var albumNameLabel = new Label { Text = "新しいアルバム名", Location = new Point(300, 20), AutoSize = true };
            this.albumNameTextBox = new TextBox { Location = new Point(300, 50), Size = new Size(200, 20) };

            // 保存済みアルバムリスト
            var savedAlbumsLabel = new Label { Text = "保存したアルバム", Location = new Point(550, 20), AutoSize = true };
            this.savedAlbumsListBox = new ListBox { Location = new Point(550, 50), Size = new Size(200, 150) };

            // アルバムを開くボタン
            this.openAlbumButton = new Button { Text = "選択したアルバムを開く", Location = new Point(550, 210), Size = new Size(200, 40) };
            this.openAlbumButton.Click += OpenAlbumButton_Click;

            this.Controls.Add(albumNameLabel);
            this.Controls.Add(this.albumNameTextBox);
            this.Controls.Add(savedAlbumsLabel);
            this.Controls.Add(this.savedAlbumsListBox);
            this.Controls.Add(this.openAlbumButton);

            this.createCanvasButton.Text = "新しいアルバムを作成";

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadSavedAlbums()
        {
            savedAlbumsListBox.Items.Clear();
            var albumNames = Album.GetAlbumNames();
            savedAlbumsListBox.Items.AddRange(albumNames);
        }

        private void SelectColorButton_Click(object? sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedColor = colorDialog.Color;
                    colorPreviewPanel.BackColor = selectedColor;
                }
            }
        }

        private void CreateCanvasButton_Click(object? sender, EventArgs e)
        {
            string albumName = albumNameTextBox.Text;
            if (string.IsNullOrWhiteSpace(albumName))
            {
                MessageBox.Show("アルバム名を入力してください。");
                return;
            }

            double aspectRatio = 1.0;
            if (ratio43RadioButton.Checked) aspectRatio = 4.0 / 3.0;
            if (ratio169RadioButton.Checked) aspectRatio = 16.0 / 9.0;

            // 仮のサイズでCanvasを作成（実際のサイズはEditScreenで調整）
            var canvas = new Canvas(new Size(800, 600), selectedColor);
            var album = new Album(albumName, canvas);

            CreateNewAlbumClicked?.Invoke(this, new AlbumEventArgs(album));
        }

        private void OpenAlbumButton_Click(object? sender, EventArgs e)
        {
            if (savedAlbumsListBox.SelectedItem is not string albumName)
            {
                MessageBox.Show("開くアルバムを選択してください。");
                return;
            }

            var album = Album.Load(albumName);
            if (album != null)
            {
                OpenAlbumClicked?.Invoke(this, new AlbumEventArgs(album));
            }
            else
            {
                MessageBox.Show("アルバムの読み込みに失敗しました。");
            }
        }
    }
}
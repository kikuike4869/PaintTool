using System.Drawing;
using System.Windows.Forms;

namespace PaintTool
{
    public class ImageListPanel : UserControl
    {
        private FlowLayoutPanel flowLayoutPanel;
        public event EventHandler<ImageEventArgs> ImageDoubleClick;

        public ImageListPanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.flowLayoutPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false, // 横に並ばないようにする
                AutoScroll = true, // スクロールを有効にする
                Padding = new Padding(5)
            };

            this.Controls.Add(this.flowLayoutPanel);
        }

        public void AddImage(Image image)
        {
            var pictureBox = new PictureBox
            {
                Size = new Size(150, 100),
                SizeMode = PictureBoxSizeMode.Zoom, // 150x100の領域に収まるように表示
                Image = image,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5)
            };

            pictureBox.DoubleClick += PictureBox_DoubleClick;

            this.flowLayoutPanel.Controls.Add(pictureBox);
        }

        // ★追加: PictureBoxがダブルクリックされたときに呼ばれるメソッド
        private void PictureBox_DoubleClick(object? sender, EventArgs e)
        {
            if (sender is PictureBox pictureBox && pictureBox.Image != null)
            {
                // ImageDoubleClickイベントを発生させる
                ImageDoubleClick?.Invoke(this, new ImageEventArgs(pictureBox.Image));
            }
        }
    }
}
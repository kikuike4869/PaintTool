using System.Drawing;
using System.Windows.Forms;

namespace PaintTool
{
    public partial class MainForm : Form
    {
        private HomeScreen homeScreen = null!;
        private CanvasEditScreen canvasEditScreen = null!;

        public MainForm()
        {
            InitializeComponent();
            this.Text = "Paint Tool";
            this.Size = new Size(1024, 768);
            ShowHomeScreen();
        }

        private void ShowHomeScreen()
        {
            this.Controls.Clear();
            homeScreen = new HomeScreen
            {
                Dock = DockStyle.Fill
            };
            // イベントハンドラを新しいものに更新
            homeScreen.CreateNewAlbumClicked += HomeScreen_CreateNewAlbumClicked;
            homeScreen.OpenAlbumClicked += HomeScreen_OpenAlbumClicked;
            this.Controls.Add(homeScreen);
        }

        // 新しいアルバム作成時のイベントハンドラ
        private void HomeScreen_CreateNewAlbumClicked(object? sender, AlbumEventArgs e)
        {
            ShowCanvasEditScreen(e.Album);
        }

        // 既存アルバムを開くときのイベントハンドラ
        private void HomeScreen_OpenAlbumClicked(object? sender, AlbumEventArgs e)
        {
            ShowCanvasEditScreen(e.Album);
        }

        // 引数をAlbumに変更
        private void ShowCanvasEditScreen(Album album)
        {
            this.Controls.Clear();
            canvasEditScreen = new CanvasEditScreen(album)
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(canvasEditScreen);
        }
    }
}
namespace PaintTool
{
    public partial class MainForm : Form
    {
        private readonly ApplicationController _appController;

        public MainForm()
        {
            InitializeComponent();
            this.Text = "Photo Album Application";
            this.Size = new Size(1024, 768);
            _appController = new ApplicationController(this);
            _appController.ShowHomeScreen(); // 最初の画面表示をコントローラーに依頼
        }

        // 外部から渡されたUserControlを表示するメソッド
        public void ShowControl(UserControl control)
        {
            this.Controls.Clear();
            control.Dock = DockStyle.Fill;
            this.Controls.Add(control);
        }
    }
}
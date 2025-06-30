// Controllers/ApplicationController.cs (新規作成)
using System.Windows.Forms;

namespace PaintTool
{
    public class ApplicationController
    {
        private readonly MainForm _mainForm;
        private readonly PhotoLibrary _photoLibrary; // アプリケーションの全データを管理するModel

        // 各画面のインスタンスを保持
        private HomeScreen _homeScreen;
        private PhotoListScreen _photoListScreen; // (今後作成)
        private AlbumListScreen _albumListScreen; // (今後作成)
        private CanvasEditScreen _canvasEditScreen;

        public ApplicationController(MainForm mainForm)
        {
            _mainForm = mainForm;
            _photoLibrary = new PhotoLibrary(); // Modelを初期化
            _photoLibrary.LoadData(); // データの読み込み
        }

        public void ShowHomeScreen()
        {
            if (_homeScreen == null)
            {
                _homeScreen = new HomeScreen();
                // イベントハンドラでこのControllerのメソッドを呼ぶ
                _homeScreen.OpenPhotoListClicked += (s, e) => ShowPhotoListScreen();
                _homeScreen.OpenAlbumListClicked += (s, e) => ShowAlbumListScreen();
            }
            _mainForm.ShowControl(_homeScreen);
        }

        public void ShowPhotoListScreen()
        {
            // PhotoController を通じて写真リストを取得し、画面に渡す
            // (実装は後述)
        }

        public void ShowAlbumListScreen()
        {
            // AlbumController を通じてアルバムリストを取得し、画面に渡す
            // (実装は後述)
        }

        // public void ShowCanvasEditScreen(Album album)
        // {
        //     _canvasEditScreen = new CanvasEditScreen(album);
        //     // Homeに戻るイベントなどを設定
        //     // _canvasEditScreen.BackToHomeClicked += (s, e) => ShowHomeScreen();
        //     _mainForm.ShowControl(_canvasEditScreen);
        // }

        public void ShowCanvasEditor(Painting paintingToEdit)
        {
            // 編集対象のModelを渡して、Viewを生成する
            var canvasScreen = new CanvasEditScreen(paintingToEdit);

            // MainFormにViewを表示させる
            _mainForm.ShowControl(canvasScreen);
        }
    }
}
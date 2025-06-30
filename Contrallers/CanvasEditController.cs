using System.Drawing;
using PaintTool;

namespace PaintTool
{
    public class CanvasEditController
    {
        private readonly Album _album;

        public CanvasEditController(Album album)
        {
            _album = album;
        }

        // 画像をキャンバスに追加するロジック
        public void AddImageToCanvas(Image image)
        {
            if (_album.ActiveCanvas == null) return;

            var imageObject = new ImageObject(image)
            {
                // キャンバスの中央に配置する
                X = (_album.ActiveCanvas.Width - image.Width) / 2,
                Y = (_album.ActiveCanvas.Height - image.Height) / 2,
                Width = image.Width,
                Height = image.Height
            };

            _album.ActiveCanvas.AddObject(imageObject);
        }
    }
}
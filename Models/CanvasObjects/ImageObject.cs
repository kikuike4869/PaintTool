// PaintTool/Models/CanvasObjects/ImageObject.cs (修正後)

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text.Json.Serialization;

namespace PaintTool
{
    public class ImageObject : TransformableObject
    {
        /// <summary>
        /// 保存後はアルバムフォルダからの相対パスになります。
        /// </summary>
        public string ImagePath { get; set; }

        [JsonIgnore] // Imageオブジェクト自体はシリアライズしない
        public Image? LoadedImage { get; private set; }

        // JSONからの復元時に使用されるコンストラクタ
        [JsonConstructor]
        public ImageObject(string imagePath, PointF location, SizeF size, float angle, float scale)
        {
            ImagePath = imagePath;
            Location = location;
            Size = size;
            Angle = angle;
            Scale = scale;
            // この時点では画像はまだ読み込まない
        }

        // 新規に画像ファイルを指定してオブジェクトを作成する際に使用するコンストラクタ
        public ImageObject(string originalFilePath, PointF location)
        {
            if (!File.Exists(originalFilePath))
            {
                throw new FileNotFoundException("指定された画像ファイルが見つかりません。", originalFilePath);
            }

            ImagePath = originalFilePath; // 最初は元の絶対パスを保持
            Location = location;
            LoadImageFromPath(ImagePath); // パスから画像を読み込む

            if (LoadedImage != null)
            {
                Size = LoadedImage.Size;
            }
            Angle = 0f;
            Scale = 1f;
        }

        /// <summary>
        /// 指定されたパスから画像を読み込み、LoadedImageプロパティにセットします。
        /// </summary>
        public void LoadImageFromPath(string path)
        {
            try
            {
                ImagePath = path;
                // ファイルをロックしないように、一度メモリに読み込んでからImageオブジェクトを生成します。
                using (var bmpTemp = new Bitmap(ImagePath))
                {
                    LoadedImage = new Bitmap(bmpTemp);
                }
            }
            catch (Exception)
            {
                // 画像が読み込めなかった場合、代替のダミー画像を表示する
                LoadedImage = CreatePlaceholderImage();
                Size = new SizeF(100, 100); // ダミー画像のサイズ
            }
        }

        // 読み込み失敗時に表示する代替画像を生成するヘルパーメソッド
        private Image CreatePlaceholderImage()
        {
            var bmp = new Bitmap(100, 100);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.LightGray);
                using (var pen = new Pen(Color.Red, 2))
                {
                    g.DrawLine(pen, 0, 0, 100, 100);
                    g.DrawLine(pen, 100, 0, 0, 100);
                }
                using (var font = new Font("Arial", 8))
                using (var brush = new SolidBrush(Color.Black))
                {
                    g.DrawString("Not Found", font, brush, new PointF(10, 40));
                }
            }
            return bmp;
        }

        public override void Draw(Graphics g, Matrix transform)
        {
            // --- ここからデバッグログ ---
            Console.WriteLine($"ImageObject.Draw called. Using parent's transform.");
            // --- デバッグログここまで ---
            
            if (LoadedImage != null)
            {
                var state = g.Save();
                g.Transform = transform;

                // 変形はすべて行列に任せるので、ここではオブジェクトの
                // 本来の位置とサイズで画像を描画します。
                g.DrawImage(LoadedImage, new RectangleF(Location, Size));

                g.Restore(state);
            }
            else
            {
                // 画像が見つからない場合にプレースホルダーを描画
                g.FillRectangle(Brushes.LightGray, new RectangleF(Location, Size));
                g.DrawRectangle(Pens.Red, Location.X, Location.Y, Size.Width, Size.Height);
                using (var font = new Font("Arial", 10))
                {
                    g.DrawString("Image not found", font, Brushes.Red, new PointF(Location.X + 5, Location.Y + 5));
                }
            }
        }
    }
}
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintTool
{
    public abstract class TransformableObject : CanvasObject
    {
        public PointF Location { get; protected set; }
        public SizeF Size { get; protected set; }
        public float Angle { get; set; } = 0f;
        public float Scale { get; set; } = 1f;

        public override void Move(float dx, float dy)
        {
            Location = new PointF(Location.X + dx, Location.Y + dy);
        }

        public override RectangleF GetBounds()
        {
            // このメソッドは主に選択枠の大きさを決めるために使われます。
            // 回転を考慮した正確な外接矩形は複雑ですが、ひとまずスケールのみ反映します。
            var scaledWidth = Size.Width * Scale;
            var scaledHeight = Size.Height * Scale;
            // 中心の位置は変わらないように調整
            var scaledX = Location.X + (Size.Width - scaledWidth) / 2;
            var scaledY = Location.Y + (Size.Height - scaledHeight) / 2;
            return new RectangleF(scaledX, scaledY, scaledWidth, scaledHeight);
        }

        // 中心点を取得するヘルパーメソッド
        public PointF GetCenter()
        {
            var bounds = GetBounds();
            return new PointF(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
        }


        // マトリックス変換を適用して描画するための抽象メソッド
        public abstract void Draw(Graphics g, Matrix transform);


        public override void Draw(Graphics g)
        {
            var transform = new Matrix();
            var center = GetCenter();

            // 行列の計算順序： 後から追加した操作が先に適用される Prepend を使います
            // 1. オブジェクトを最終的な位置(Center)に移動
            transform.Translate(center.X, center.Y, MatrixOrder.Prepend);
            // 2. 回転
            transform.Rotate(Angle, MatrixOrder.Prepend);
            // 3. 拡大・縮小
            transform.Scale(Scale, Scale, MatrixOrder.Prepend);
            // 4. オブジェクトの中心が原点(0,0)に来るように移動
            transform.Translate(-center.X, -center.Y, MatrixOrder.Prepend);

            // ログ出力（任意）
            Console.WriteLine($"--- Drawing {this.GetType().Name} ---");
            Console.WriteLine($"Location={Location}, Size={Size}, Angle={Angle}, Scale={Scale}");
            Console.WriteLine($"Matrix Elements: {string.Join(", ", transform.Elements)}");

            Draw(g, transform);

            if (IsSelected)
            {
                DrawSelection(g);
            }
        }

        // 選択されている時の枠やハンドルを描画する
        private void DrawSelection(Graphics g)
        {
            var bounds = GetBounds();
            using (var pen = new Pen(Color.Blue, 1) { DashStyle = DashStyle.Dash })
            {
                // バウンディングボックスを回転させて描画
                var points = new PointF[]
                {
                    new PointF(bounds.Left, bounds.Top),
                    new PointF(bounds.Right, bounds.Top),
                    new PointF(bounds.Right, bounds.Bottom),
                    new PointF(bounds.Left, bounds.Bottom)
                };

                using (var m = new Matrix())
                {
                    var center = GetCenter();
                    m.RotateAt(Angle, center);
                    m.TransformPoints(points);
                }
                g.DrawPolygon(pen, points);
            }

            // 各ハンドルを描画
            foreach (var handle in GetHandles())
            {
                g.FillRectangle(Brushes.White, handle.Value);
                g.DrawRectangle(Pens.Black, Rectangle.Round(handle.Value));
            }
        }

        public override bool HitTest(Point point)
        {
            // --- ログ出力 ---
            Console.WriteLine($"--- HitTest on {this.GetType().Name} ---");
            Console.WriteLine($"Mouse Click (World Coords): {point}");

            // 逆行列を作成して、ワールド座標(クリック位置)をオブジェクトのローカル座標に変換します
            var inverseMatrix = new Matrix();
            var center = GetCenter();

            // Drawとは逆の順序で、逆の操作を追加していきます
            // 1. オブジェクトを最終的な位置に移動 (Translateの逆)
            inverseMatrix.Translate(center.X, center.Y, MatrixOrder.Prepend);
            // 2. 拡大・縮小を元に戻す
            if (Scale != 0) inverseMatrix.Scale(1 / Scale, 1 / Scale, MatrixOrder.Prepend);
            // 3. 回転を元に戻す
            inverseMatrix.Rotate(-Angle, MatrixOrder.Prepend);
            // 4. オブジェクトの中心が原点に来るように移動
            inverseMatrix.Translate(-center.X, -center.Y, MatrixOrder.Prepend);

            var points = new PointF[] { point };
            inverseMatrix.TransformPoints(points); // クリック座標を逆変換
            var transformedPoint = points[0];

            // オブジェクトの元の（変形していない）矩形
            var originalBounds = new RectangleF(Location, Size);

            // --- ログ出力 ---
            Console.WriteLine($"Transformed Click (Local Coords): {transformedPoint}");
            Console.WriteLine($"Object Bounds (Local Coords): {originalBounds}");

            bool isHit = originalBounds.Contains(transformedPoint);

            Console.WriteLine($"Hit Result: {isHit}");
            Console.WriteLine("-----------------------------");

            return isHit;
        }

        public HandleType HitTestHandles(Point point)
        {
            foreach (var handle in GetHandles())
            {
                if (handle.Value.Contains(point))
                {
                    return handle.Key;
                }
            }
            return HandleType.None;
        }


        public Dictionary<HandleType, RectangleF> GetHandles()
        {
            var handles = new Dictionary<HandleType, RectangleF>();
            var bounds = GetBounds();
            float handleSize = 8;
            float halfHandleSize = handleSize / 2;

            var handlePoints = new Dictionary<HandleType, PointF>
            {
                { HandleType.Resize_TopLeft, new PointF(bounds.Left, bounds.Top) },
                { HandleType.Resize_TopRight, new PointF(bounds.Right, bounds.Top) },
                { HandleType.Resize_BottomLeft, new PointF(bounds.Left, bounds.Bottom) },
                { HandleType.Resize_BottomRight, new PointF(bounds.Right, bounds.Bottom) },
                // 回転ハンドルは上部中央に配置
                { HandleType.Rotate, new PointF(bounds.Left + bounds.Width / 2, bounds.Top - 20) }
            };

            var center = GetCenter();
            using (var m = new Matrix())
            {
                m.RotateAt(Angle, center);

                foreach (var hp in handlePoints)
                {
                    var points = new PointF[] { hp.Value };
                    m.TransformPoints(points);
                    handles[hp.Key] = new RectangleF(points[0].X - halfHandleSize, points[0].Y - halfHandleSize, handleSize, handleSize);
                }
            }
            return handles;
        }

        /// <summary>
        /// コマンドからオブジェクトの状態を一括で設定するためのメソッド
        /// </summary>
        public void SetState(Commands.TransformState state)
        {
            Location = state.Location;
            Size = state.Size;
            Angle = state.Angle;
            Scale = state.Scale;
        }
    }
}
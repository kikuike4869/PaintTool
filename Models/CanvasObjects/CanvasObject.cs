using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Serialization;

namespace PaintTool
{
    // このクラスがポリモーフィズムの基底クラスであることを定義します。
    // "$type" という名前のプロパティで具象クラスを判断するように指定します。
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    // "$type" の値と、それに対応するC#のクラスをマッピングします。
    [JsonDerivedType(typeof(DrawingObject), typeDiscriminator: "drawing")]
    [JsonDerivedType(typeof(ImageObject), typeDiscriminator: "image")]
    [JsonDerivedType(typeof(TextObject), typeDiscriminator: "text")]
    public abstract class CanvasObject
    {
        public virtual bool CanSelect => true;
        [JsonIgnore]
        public bool IsSelected { get; set; }

        public abstract void Draw(Graphics g);
        public abstract bool HitTest(Point point);

        public virtual void Move(float dx, float dy) { } // intからfloatに変更
        public virtual RectangleF GetBounds() => RectangleF.Empty; // RectangleからRectangleFに変更
    }
}
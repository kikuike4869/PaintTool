namespace PaintTool
{
    // イベント引数クラスを新規作成
    public class AlbumEventArgs : EventArgs
    {
        public Album Album { get; }
        public AlbumEventArgs(Album album)
        {
            Album = album;
        }
    }
}
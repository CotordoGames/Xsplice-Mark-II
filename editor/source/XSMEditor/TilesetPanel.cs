using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace XSMEditor
{
    public class TilesetPanel : FrameworkElement
    {
        public BitmapSource? Tileset { get; private set; }
        public int SelectedTile { get; private set; } = 1;
        private const int TileSize = 16;
        private const int Scale = 2;
        private const int TilesPerRow = 16;

        public event Action<int>? TileSelected;

        public void SetTileset(BitmapSource? bmp)
        {
            Tileset = bmp;
            if (bmp != null)
            {
                Width = TilesPerRow * TileSize * Scale;
                Height = (bmp.PixelHeight / TileSize) * TileSize * Scale;
            }
            InvalidateVisual();
        }

        public void SetSelected(int tile)
        {
            SelectedTile = tile;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0x1C, 0x1E, 0x21)), null,
                new Rect(0, 0, ActualWidth, ActualHeight));

            if (Tileset == null) return;

            dc.DrawImage(Tileset, new Rect(0, 0, ActualWidth, ActualHeight));

            // grid
            var gridPen = new Pen(new SolidColorBrush(Color.FromArgb(60, 71, 140, 191)), 0.5);
            int cols = TilesPerRow;
            int rows = Tileset.PixelHeight / TileSize;
            for (int x = 0; x <= cols; x++)
                dc.DrawLine(gridPen, new Point(x * TileSize * Scale, 0), new Point(x * TileSize * Scale, ActualHeight));
            for (int y = 0; y <= rows; y++)
                dc.DrawLine(gridPen, new Point(0, y * TileSize * Scale), new Point(ActualWidth, y * TileSize * Scale));

            // selected highlight
            int selX = (SelectedTile % TilesPerRow) * TileSize * Scale;
            int selY = (SelectedTile / TilesPerRow) * TileSize * Scale;
            dc.DrawRectangle(
                new SolidColorBrush(Color.FromArgb(60, 71, 140, 191)),
                new Pen(new SolidColorBrush(Color.FromRgb(71, 140, 191)), 2),
                new Rect(selX, selY, TileSize * Scale, TileSize * Scale));
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (Tileset == null) return;
            var p = e.GetPosition(this);
            int tx = (int)(p.X / (TileSize * Scale));
            int ty = (int)(p.Y / (TileSize * Scale));
            SelectedTile = ty * TilesPerRow + tx;
            TileSelected?.Invoke(SelectedTile);
            InvalidateVisual();
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters p) =>
            new PointHitTestResult(this, p.HitPoint);
        protected override bool IsEnabledCore => true;
    }
}

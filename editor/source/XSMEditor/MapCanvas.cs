using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace XSMEditor
{
    public enum EditorMode { Tile, Collision }
    public enum TileTool { Draw, Erase, Fill, Pick, Rect }

    public class MapCanvas : FrameworkElement
    {
        // ── state ────────────────────────────────────────────────
        public XsmMap? Map { get; private set; }
        public BitmapSource? Tileset { get; private set; }
        public EditorMode Mode { get; set; } = EditorMode.Tile;
        public TileTool Tool { get; set; } = TileTool.Draw;
        public int SelectedTile { get; set; } = 1;
        public int Zoom { get; private set; } = 2;
        public CollisionBox? SelectedBox { get; private set; }

        private const int TileSize = 16;
        private const int TilesPerRow = 16; // tileset is always 256px wide

        // pan
        private double _panX, _panY;
        private bool _isPanning;
        private Point _panStart;
        private double _panOriginX, _panOriginY;

        // painting
        private bool _isPainting;
        private Point? _rectStart;
        private Point _rectEnd;
        private int _hoverTX = -1, _hoverTY = -1;

        // undo
        private readonly Stack<MapTile[]> _undoStack = new();
        private readonly Stack<MapTile[]> _redoStack = new();

        public event Action? Invalidated;
        public event Action<CollisionBox?>? SelectionChanged;
        public event Action<int>? TilePicked;
        public event Action? MapChanged;

        // ── public API ───────────────────────────────────────────
        public void SetMap(XsmMap map)
        {
            Map = map;
            _panX = 0; _panY = 0;
            _undoStack.Clear(); _redoStack.Clear();
            InvalidateVisual();
        }

        public void SetTileset(BitmapSource? bmp)
        {
            Tileset = bmp;
            InvalidateVisual();
        }

        public void SetZoom(int z)
        {
            Zoom = Math.Clamp(z, 1, 8);
            InvalidateVisual();
        }

        public void Undo()
        {
            if (_undoStack.Count == 0 || Map == null) return;
            _redoStack.Push(Map.Tiles.Select(t => new MapTile { Tile = t.Tile }).ToArray());
            var prev = _undoStack.Pop();
            for (int i = 0; i < Map.Tiles.Length; i++) Map.Tiles[i].Tile = prev[i].Tile;
            InvalidateVisual(); MapChanged?.Invoke();
        }

        public void Redo()
        {
            if (_redoStack.Count == 0 || Map == null) return;
            _undoStack.Push(Map.Tiles.Select(t => new MapTile { Tile = t.Tile }).ToArray());
            var next = _redoStack.Pop();
            for (int i = 0; i < Map.Tiles.Length; i++) Map.Tiles[i].Tile = next[i].Tile;
            InvalidateVisual(); MapChanged?.Invoke();
        }

        public void SelectBox(CollisionBox? box)
        {
            SelectedBox = box;
            SelectionChanged?.Invoke(box);
            InvalidateVisual();
        }

        private void Snapshot()
        {
            if (Map == null) return;
            _undoStack.Push(Map.Tiles.Select(t => new MapTile { Tile = t.Tile }).ToArray());
            _redoStack.Clear();
        }

        // ── render ───────────────────────────────────────────────
        protected override void OnRender(DrawingContext dc)
        {
            if (Map == null) return;

            int cellW = TileSize * Zoom;
            int cw = (int)ActualWidth, ch = (int)ActualHeight;

            // background
            dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0x1C, 0x1E, 0x21)), null,
                new Rect(0, 0, cw, ch));

            int w = Map.Width, h = Map.Height;

            // checkerboard + tiles
            for (int ty = 0; ty < h; ty++)
            {
                for (int tx = 0; tx < w; tx++)
                {
                    double px = _panX + tx * cellW;
                    double py = _panY + ty * cellW;
                    if (px + cellW < 0 || py + cellW < 0 || px > cw || py > ch) continue;

                    var checker = (tx + ty) % 2 == 0
                        ? Color.FromRgb(0x1E, 0x21, 0x24)
                        : Color.FromRgb(0x22, 0x25, 0x27);
                    dc.DrawRectangle(new SolidColorBrush(checker), null, new Rect(px, py, cellW, cellW));

                    var tile = Map.Tiles[ty * w + tx];
                    if (tile.Tile > 0 && Tileset != null)
                    {
                        int idx = tile.Tile;
                        int srcX = (idx % TilesPerRow) * TileSize;
                        int srcY = (idx / TilesPerRow) * TileSize;
                        var crop = new CroppedBitmap(Tileset,
                            new Int32Rect(srcX, srcY, TileSize, TileSize));
                        dc.DrawImage(crop, new Rect(px, py, cellW, cellW));
                    }
                }
            }

            // grid
            var gridPen = new Pen(new SolidColorBrush(Color.FromArgb(120, 58, 61, 64)), 0.5);
            for (int tx = 0; tx <= w; tx++)
            {
                double px = _panX + tx * cellW;
                dc.DrawLine(gridPen, new Point(px, _panY), new Point(px, _panY + h * cellW));
            }
            for (int ty = 0; ty <= h; ty++)
            {
                double py = _panY + ty * cellW;
                dc.DrawLine(gridPen, new Point(_panX, py), new Point(_panX + w * cellW, py));
            }

            // map border
            var borderPen = new Pen(new SolidColorBrush(Color.FromArgb(80, 71, 140, 191)), 1);
            dc.DrawRectangle(null, borderPen, new Rect(_panX, _panY, w * cellW, h * cellW));

            // collision boxes
            var boxFill = new SolidColorBrush(Color.FromArgb(30, 71, 140, 191));
            var boxPen = new Pen(new SolidColorBrush(Color.FromArgb(200, 71, 140, 191)), 1.5);
            var boxSelPen = new Pen(new SolidColorBrush(Color.FromArgb(230, 251, 191, 36)), 2)
                { DashStyle = new DashStyle(new double[] { 5, 3 }, 0) };
            var boxLabelBrush = new SolidColorBrush(Color.FromArgb(230, 71, 140, 191));
            var tf = new Typeface("Segoe UI");

            for (int i = 0; i < Map.CollisionBoxes.Count; i++)
            {
                var box = Map.CollisionBoxes[i];
                var r = new Rect(_panX + box.X * cellW, _panY + box.Y * cellW,
                                 box.W * cellW, box.H * cellW);
                dc.DrawRectangle(boxFill, boxPen, r);
                if (box == SelectedBox)
                    dc.DrawRectangle(null, boxSelPen,
                        new Rect(r.X - 1, r.Y - 1, r.Width + 2, r.Height + 2));

                var ft = new FormattedText($"#{i}", System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, tf, Math.Max(7, Math.Min(cellW / 2.0, 13)),
                    boxLabelBrush, 96);
                dc.DrawText(ft, new Point(r.X + 3, r.Y + 2));
            }

            // hover
            if (_hoverTX >= 0 && _hoverTY >= 0 && _hoverTX < w && _hoverTY < h && Mode == EditorMode.Tile)
            {
                var hoverFill = new SolidColorBrush(Color.FromArgb(40, 71, 140, 191));
                var hoverPen = new Pen(new SolidColorBrush(Color.FromArgb(180, 71, 140, 191)), 1.5);
                dc.DrawRectangle(hoverFill, hoverPen,
                    new Rect(_panX + _hoverTX * cellW, _panY + _hoverTY * cellW, cellW, cellW));
            }

            // rect preview
            if (_rectStart.HasValue && _isPainting)
            {
                int x0 = Math.Min((int)_rectStart.Value.X, (int)_rectEnd.X);
                int y0 = Math.Min((int)_rectStart.Value.Y, (int)_rectEnd.Y);
                int x1 = Math.Max((int)_rectStart.Value.X, (int)_rectEnd.X);
                int y1 = Math.Max((int)_rectStart.Value.Y, (int)_rectEnd.Y);
                var rectFill = new SolidColorBrush(Color.FromArgb(40, 71, 140, 191));
                var rectPen = new Pen(new SolidColorBrush(Color.FromArgb(200, 71, 140, 191)), 1.5)
                    { DashStyle = new DashStyle(new double[] { 4, 3 }, 0) };
                dc.DrawRectangle(rectFill, rectPen,
                    new Rect(_panX + x0 * cellW, _panY + y0 * cellW,
                             (x1 - x0 + 1) * cellW, (y1 - y0 + 1) * cellW));
            }

            Invalidated?.Invoke();
        }

        // ── mouse ────────────────────────────────────────────────
        private (int tx, int ty) GetTile(Point p) =>
            ((int)Math.Floor((p.X - _panX) / (TileSize * Zoom)),
             (int)Math.Floor((p.Y - _panY) / (TileSize * Zoom)));

        private bool InBounds(int tx, int ty) =>
            Map != null && tx >= 0 && ty >= 0 && tx < Map.Width && ty < Map.Height;

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Focus();
            var p = e.GetPosition(this);

            if (e.MiddleButton == MouseButtonState.Pressed ||
                (e.LeftButton == MouseButtonState.Pressed && Keyboard.IsKeyDown(Key.Space)))
            {
                _isPanning = true; _panStart = p; _panOriginX = _panX; _panOriginY = _panY;
                Cursor = Cursors.SizeAll; CaptureMouse(); return;
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isPainting = true;
                var (tx, ty) = GetTile(p);

                if (Mode == EditorMode.Tile)
                {
                    if (Tool == TileTool.Rect) { _rectStart = new Point(tx, ty); _rectEnd = new Point(tx, ty); }
                    else { Snapshot(); ApplyTool(tx, ty); InvalidateVisual(); }
                }
                else // collision
                {
                    // check hit on existing box
                    CollisionBox? hit = null;
                    if (Map != null)
                        for (int i = Map.CollisionBoxes.Count - 1; i >= 0; i--)
                        {
                            var b = Map.CollisionBoxes[i];
                            if (tx >= b.X && tx < b.X + b.W && ty >= b.Y && ty < b.Y + b.H)
                            { hit = b; break; }
                        }
                    if (hit != null) SelectBox(hit);
                    else { _rectStart = new Point(tx, ty); _rectEnd = new Point(tx, ty); }
                }
                CaptureMouse();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var p = e.GetPosition(this);
            if (_isPanning)
            {
                _panX = _panOriginX + (p.X - _panStart.X);
                _panY = _panOriginY + (p.Y - _panStart.Y);
                InvalidateVisual(); return;
            }

            var (tx, ty) = GetTile(p);
            if (tx != _hoverTX || ty != _hoverTY) { _hoverTX = tx; _hoverTY = ty; InvalidateVisual(); }

            if (_isPainting)
            {
                if (Mode == EditorMode.Tile)
                {
                    if (Tool == TileTool.Rect) { _rectEnd = new Point(tx, ty); InvalidateVisual(); }
                    else ApplyTool(tx, ty);
                }
                else if (_rectStart.HasValue) { _rectEnd = new Point(tx, ty); InvalidateVisual(); }
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (_isPanning) { _isPanning = false; Cursor = Cursors.Cross; ReleaseMouseCapture(); return; }

            var p = e.GetPosition(this);
            var (tx, ty) = GetTile(p);

            if (_isPainting && _rectStart.HasValue)
            {
                int x0 = Math.Min((int)_rectStart.Value.X, tx);
                int y0 = Math.Min((int)_rectStart.Value.Y, ty);
                int x1 = Math.Max((int)_rectStart.Value.X, tx);
                int y1 = Math.Max((int)_rectStart.Value.Y, ty);

                if (Mode == EditorMode.Tile)
                {
                    Snapshot();
                    for (int ry = y0; ry <= y1; ry++)
                        for (int rx = x0; rx <= x1; rx++)
                            WriteTile(rx, ry);
                    MapChanged?.Invoke();
                }
                else if (Map != null)
                {
                    var box = new CollisionBox { X = x0, Y = y0, W = x1 - x0 + 1, H = y1 - y0 + 1 };
                    Map.CollisionBoxes.Add(box);
                    SelectBox(box);
                    MapChanged?.Invoke();
                }
                _rectStart = null;
            }

            _isPainting = false;
            ReleaseMouseCapture();
            InvalidateVisual();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            _hoverTX = -1; _hoverTY = -1; InvalidateVisual();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            var p = e.GetPosition(this);
            int oldZ = Zoom;
            Zoom = Math.Clamp(Zoom + (e.Delta > 0 ? 1 : -1), 1, 8);
            if (Zoom != oldZ)
            {
                _panX = p.X - (p.X - _panX) * ((double)Zoom / oldZ);
                _panY = p.Y - (p.Y - _panY) * ((double)Zoom / oldZ);
                InvalidateVisual();
            }
        }

        // ── tile ops ─────────────────────────────────────────────
        private void ApplyTool(int tx, int ty)
        {
            if (!InBounds(tx, ty) || Map == null) return;
            switch (Tool)
            {
                case TileTool.Draw: WriteTile(tx, ty); InvalidateVisual(); MapChanged?.Invoke(); break;
                case TileTool.Erase: Map.Tiles[ty * Map.Width + tx].Tile = 0; InvalidateVisual(); MapChanged?.Invoke(); break;
                case TileTool.Fill: Snapshot(); FloodFill(tx, ty, Map.Tiles[ty * Map.Width + tx].Tile); InvalidateVisual(); MapChanged?.Invoke(); break;
                case TileTool.Pick:
                    SelectedTile = Map.Tiles[ty * Map.Width + tx].Tile;
                    TilePicked?.Invoke(SelectedTile);
                    break;
            }
        }

        private void WriteTile(int tx, int ty)
        {
            if (!InBounds(tx, ty) || Map == null) return;
            Map.Tiles[ty * Map.Width + tx].Tile = (byte)SelectedTile;
        }

        private void FloodFill(int sx, int sy, byte from)
        {
            if (Map == null || from == SelectedTile) return;
            var stack = new Stack<(int, int)>();
            stack.Push((sx, sy));
            while (stack.Count > 0)
            {
                var (x, y) = stack.Pop();
                if (!InBounds(x, y)) continue;
                if (Map.Tiles[y * Map.Width + x].Tile != from) continue;
                Map.Tiles[y * Map.Width + x].Tile = (byte)SelectedTile;
                stack.Push((x + 1, y)); stack.Push((x - 1, y));
                stack.Push((x, y + 1)); stack.Push((x, y - 1));
            }
        }

        protected override bool IsEnabledCore => true;
        protected override HitTestResult HitTestCore(PointHitTestParameters p) =>
            new PointHitTestResult(this, p.HitPoint);
    }
}

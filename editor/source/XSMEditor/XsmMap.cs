using System.Collections.ObjectModel;
using System.ComponentModel;

namespace XSMEditor
{
    public class MapTile
    {
        public byte Tile { get; set; }
    }

    public class CollisionBox : INotifyPropertyChanged
    {
        private int _x, _y, _w, _h;
        public int X { get => _x; set { _x = value; OnPropertyChanged(nameof(X)); } }
        public int Y { get => _y; set { _y = value; OnPropertyChanged(nameof(Y)); } }
        public int W { get => _w; set { _w = Math.Max(1, value); OnPropertyChanged(nameof(W)); } }
        public int H { get => _h; set { _h = Math.Max(1, value); OnPropertyChanged(nameof(H)); } }

        public string DisplayName => $"{W}×{H} @{X},{Y}";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            if (name != nameof(DisplayName))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
        }
    }

    public class XsmMap : INotifyPropertyChanged
    {
        private string _name = "untitled";
        private string _author = "DJ";
        private int _width = 20;
        private int _height = 15;
        private byte _tilemapIdx = 0;
        private byte _bgIdx = 0;
        private byte _bgmIdx = 0;

        public string Name { get => _name; set { _name = value.Length > 16 ? value[..16] : value; OnPropertyChanged(nameof(Name)); } }
        public string Author { get => _author; set { _author = value.Length > 16 ? value[..16] : value; OnPropertyChanged(nameof(Author)); } }
        public int Width { get => _width; set { _width = Math.Clamp(value, 1, 255); OnPropertyChanged(nameof(Width)); } }
        public int Height { get => _height; set { _height = Math.Clamp(value, 1, 255); OnPropertyChanged(nameof(Height)); } }
        public byte TilemapIdx { get => _tilemapIdx; set { _tilemapIdx = value; OnPropertyChanged(nameof(TilemapIdx)); } }
        public byte BgIdx { get => _bgIdx; set { _bgIdx = value; OnPropertyChanged(nameof(BgIdx)); } }
        public byte BgmIdx { get => _bgmIdx; set { _bgmIdx = value; OnPropertyChanged(nameof(BgmIdx)); } }

        public MapTile[] Tiles { get; set; } = Array.Empty<MapTile>();
        public ObservableCollection<CollisionBox> CollisionBoxes { get; set; } = new();

        public void InitTiles()
        {
            Tiles = new MapTile[Width * Height];
            for (int i = 0; i < Tiles.Length; i++)
                Tiles[i] = new MapTile();
        }

        public void ResizeTiles(int newW, int newH, int anchorX = 0, int anchorY = 0)
        {
            var old = Tiles;
            int oldW = Width, oldH = Height;
            var newTiles = new MapTile[newW * newH];
            for (int i = 0; i < newTiles.Length; i++) newTiles[i] = new MapTile();

            int offX = anchorX == 1 ? (newW - oldW) / 2 : anchorX == 2 ? newW - oldW : 0;
            int offY = anchorY == 1 ? (newH - oldH) / 2 : anchorY == 2 ? newH - oldH : 0;

            for (int y = 0; y < newH; y++)
                for (int x = 0; x < newW; x++)
                {
                    int ox = x - offX, oy = y - offY;
                    if (ox >= 0 && oy >= 0 && ox < oldW && oy < oldH)
                        newTiles[y * newW + x] = old[oy * oldW + ox];
                }

            Width = newW; Height = newH; Tiles = newTiles;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

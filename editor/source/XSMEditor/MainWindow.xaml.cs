using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace XSMEditor
{
    public partial class MainWindow : Window
    {
        private XsmMap _map = new();
        private string? _filePath;
        private bool _dirty;
        private bool _suppressEvents;
        private string _assetsPath = "";

        // asset lists: index -> filename
        private List<string> _tilemaps = new();
        private List<string> _bgs = new();
        private List<string> _bgms = new();

        public MainWindow()
        {
            InitializeComponent();

            Canvas.MapChanged += OnMapChanged;
            Canvas.TilePicked += OnTilePicked;
            Canvas.SelectionChanged += OnCanvasSelectionChanged;
            TilesetView.TileSelected += OnTilesetPicked;

            KeyDown += OnKeyDown;

            NewMap();
            TryLoadAssets(AppDomain.CurrentDomain.BaseDirectory);
        }

        // ── asset discovery ──────────────────────────────────────
        private void TryLoadAssets(string baseDir)
        {
            // look for assets relative to exe, or let user pick
            string[] candidates = {
                Path.Combine(baseDir, "assets", "sprites"),
                Path.Combine(baseDir, "..", "assets", "sprites"),
                Path.Combine(baseDir, "..", "..", "assets", "sprites"),
                Path.Combine(baseDir, "..", "..", "..", "assets", "sprites"),
                Path.Combine(baseDir, "..", "..", "..", "..", "assets", "sprites"),
            };

            foreach (var c in candidates)
            {
                if (Directory.Exists(c)) { LoadAssetsFrom(c); return; }
            }

            // prompt user
            SetStatus("No assets folder found — use File > Set Assets Path");
        }

        private void LoadAssetsFrom(string spritesDir)
        {
            _assetsPath = spritesDir;
            _tilemaps.Clear(); _bgs.Clear();

            // scan tm0..tm63
            for (int i = 0; i <= 63; i++)
            {
                string p = Path.Combine(spritesDir, $"tm{i}.png");
                if (File.Exists(p)) _tilemaps.Add(p);
                else if (i > 0 && _tilemaps.Count > 0) break; // stop at first gap after finding some
            }
            if (_tilemaps.Count == 0)
            {
                // add placeholders up to 31 anyway so dropdown is usable
                for (int i = 0; i <= 31; i++) _tilemaps.Add($"tm{i}.png (missing)");
            }

            // bg images
            for (int i = 0; i <= 63; i++)
            {
                string p = Path.Combine(spritesDir, $"bg{i}.png");
                if (File.Exists(p)) _bgs.Add(p);
                else if (i > 0 && _bgs.Count > 0) break;
            }
            if (_bgs.Count == 0) _bgs.Add("(none)");

            // bgm
            string soundsDir = Path.Combine(Path.GetDirectoryName(spritesDir)!, "sounds");
            _bgms.Clear();
            if (Directory.Exists(soundsDir))
            {
                foreach (var f in Directory.GetFiles(soundsDir, "*.ogg").Concat(
                                  Directory.GetFiles(soundsDir, "*.wav")).OrderBy(f => f))
                    _bgms.Add(f);
            }
            if (_bgms.Count == 0) _bgms.Add("(none)");

            PopulateDropdowns();
            LoadTileset(_map.TilemapIdx);
            SetStatus($"Assets loaded from {spritesDir}");
        }

        private void PopulateDropdowns()
        {
            _suppressEvents = true;
            CmbTilemap.ItemsSource = _tilemaps.Select((p, i) => $"tm{i} — {Path.GetFileName(p)}").ToList();
            CmbBg.ItemsSource = _bgs.Select((p, i) => $"bg{i} — {Path.GetFileName(p)}").ToList();
            CmbBgm.ItemsSource = _bgms.Select((p, i) => $"{i} — {Path.GetFileName(p)}").ToList();
            CmbTilemap.SelectedIndex = Math.Clamp(_map.TilemapIdx, 0, _tilemaps.Count - 1);
            CmbBg.SelectedIndex = Math.Clamp(_map.BgIdx, 0, _bgs.Count - 1);
            CmbBgm.SelectedIndex = Math.Clamp(_map.BgmIdx, 0, _bgms.Count - 1);
            _suppressEvents = false;
        }

        private void LoadTileset(int idx)
        {
            if (idx < 0 || idx >= _tilemaps.Count) { Canvas.SetTileset(null); TilesetView.SetTileset(null); return; }
            string path = _tilemaps[idx];
            if (!File.Exists(path)) { Canvas.SetTileset(null); TilesetView.SetTileset(null); return; }
            try
            {
                var bmp = new BitmapImage(new Uri(path, UriKind.Absolute));
                bmp.Freeze();
                Canvas.SetTileset(bmp);
                TilesetView.SetTileset(bmp);
            }
            catch { Canvas.SetTileset(null); TilesetView.SetTileset(null); }
        }

        // ── map lifecycle ────────────────────────────────────────
        private void NewMap()
        {
            _map = new XsmMap();
            _map.Name = "untitled";
            _map.Author = "DJ";
            _map.Width = 20; _map.Height = 15;
            _map.InitTiles();
            _filePath = null;
            _dirty = false;
            PushMapToUI();
        }

        private void PushMapToUI()
        {
            _suppressEvents = true;
            TxtName.Text = _map.Name;
            TxtAuthor.Text = _map.Author;
            TxtWidth.Text = _map.Width.ToString();
            TxtHeight.Text = _map.Height.ToString();
            _suppressEvents = false;

            Canvas.SetMap(_map);
            BoxList.ItemsSource = _map.CollisionBoxes;
            TxtBoxCount.Text = _map.CollisionBoxes.Count.ToString();
            _map.CollisionBoxes.CollectionChanged += (_, _) =>
            {
                TxtBoxCount.Text = _map.CollisionBoxes.Count.ToString();
            };
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            Title = $"{(_dirty ? "* " : "")}{_map.Name} — XSM Editor";
        }

        private void OnMapChanged()
        {
            _dirty = true;
            UpdateTitle();
        }

        // ── file ops ─────────────────────────────────────────────
        private void OnNew(object s, RoutedEventArgs e)
        {
            if (!ConfirmDiscard()) return;
            NewMap();
            SetStatus("New map");
        }

        private void OnOpen(object s, RoutedEventArgs e)
        {
            if (!ConfirmDiscard()) return;
            var dlg = new OpenFileDialog { Filter = "XSM Map (*.xsm)|*.xsm|All files|*.*", Title = "Open Map" };
            if (dlg.ShowDialog() != true) return;
            try
            {
                _map = XsmSerializer.Load(dlg.FileName);
                _filePath = dlg.FileName;
                _dirty = false;
                PushMapToUI();
                // try to load assets relative to file location
                string? dir = Path.GetDirectoryName(dlg.FileName);
                if (dir != null) TryLoadAssets(dir);
                else TryLoadAssets(AppDomain.CurrentDomain.BaseDirectory);
                SetStatus($"Opened {Path.GetFileName(dlg.FileName)}");
            }
            catch (Exception ex) { MessageBox.Show($"Failed to open: {ex.Message}"); }
        }

        private void OnSave(object s, RoutedEventArgs e)
        {
            if (_filePath == null) { OnSaveAs(s, e); return; }
            Save(_filePath);
        }

        private void OnSaveAs(object s, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "XSM Map (*.xsm)|*.xsm|All files|*.*",
                Title = "Save Map",
                FileName = _map.Name.Trim()
            };
            if (dlg.ShowDialog() != true) return;
            _filePath = dlg.FileName;
            Save(_filePath);
        }

        private void Save(string path)
        {
            // sync header from UI
            _map.Name = TxtName.Text;
            _map.Author = TxtAuthor.Text;
            try
            {
                XsmSerializer.Save(_map, path);
                _dirty = false;
                UpdateTitle();
                SetStatus($"Saved {Path.GetFileName(path)} ({new FileInfo(path).Length}B)");
            }
            catch (Exception ex) { MessageBox.Show($"Failed to save: {ex.Message}"); }
        }

        private bool ConfirmDiscard()
        {
            if (!_dirty) return true;
            var r = MessageBox.Show("Unsaved changes — discard?", "XSM Editor",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            return r == MessageBoxResult.Yes;
        }

        // ── toolbar events ───────────────────────────────────────
        private void OnUndo(object s, RoutedEventArgs e) => Canvas.Undo();
        private void OnRedo(object s, RoutedEventArgs e) => Canvas.Redo();

        private void OnModeTile(object s, RoutedEventArgs e)
        {
            Canvas.Mode = EditorMode.Tile;
            BtnModeTile.Background = FindResource("Accent4Brush") as System.Windows.Media.Brush;
            BtnModeTile.BorderBrush = FindResource("AccentBrush") as System.Windows.Media.Brush;
            BtnModeColl.Background = FindResource("Bg3Brush") as System.Windows.Media.Brush;
            BtnModeColl.BorderBrush = FindResource("BorderBrush") as System.Windows.Media.Brush;
        }

        private void OnModeColl(object s, RoutedEventArgs e)
        {
            Canvas.Mode = EditorMode.Collision;
            BtnModeColl.Background = FindResource("Accent4Brush") as System.Windows.Media.Brush;
            BtnModeColl.BorderBrush = FindResource("AccentBrush") as System.Windows.Media.Brush;
            BtnModeTile.Background = FindResource("Bg3Brush") as System.Windows.Media.Brush;
            BtnModeTile.BorderBrush = FindResource("BorderBrush") as System.Windows.Media.Brush;
        }

        private void OnToggleGrid(object s, RoutedEventArgs e) { /* grid always on for now */ }
        private void OnResetView(object s, RoutedEventArgs e) { Canvas.SetZoom(2); Canvas.InvalidateVisual(); }
        private void OnZoomIn(object s, RoutedEventArgs e) { Canvas.SetZoom(Canvas.Zoom + 1); ZoomLabel.Text = $"{Canvas.Zoom}×"; }
        private void OnZoomOut(object s, RoutedEventArgs e) { Canvas.SetZoom(Canvas.Zoom - 1); ZoomLabel.Text = $"{Canvas.Zoom}×"; }

        // ── tool buttons ─────────────────────────────────────────
        private Button[] ToolButtons => new[] { BtnDraw, BtnErase, BtnFill, BtnPick, BtnRect };

        private void SetActiveTool(Button active, TileTool tool)
        {
            Canvas.Tool = tool;
            foreach (var b in ToolButtons)
            {
                b.Background = FindResource("Bg3Brush") as System.Windows.Media.Brush;
                b.BorderBrush = FindResource("BorderBrush") as System.Windows.Media.Brush;
                b.Foreground = FindResource("TextMutedBrush") as System.Windows.Media.Brush;
            }
            active.Background = FindResource("Accent4Brush") as System.Windows.Media.Brush;
            active.BorderBrush = FindResource("AccentBrush") as System.Windows.Media.Brush;
            active.Foreground = FindResource("TextBrush") as System.Windows.Media.Brush;
        }

        private void OnToolDraw(object s, RoutedEventArgs e) => SetActiveTool(BtnDraw, TileTool.Draw);
        private void OnToolErase(object s, RoutedEventArgs e) => SetActiveTool(BtnErase, TileTool.Erase);
        private void OnToolFill(object s, RoutedEventArgs e) => SetActiveTool(BtnFill, TileTool.Fill);
        private void OnToolPick(object s, RoutedEventArgs e) => SetActiveTool(BtnPick, TileTool.Pick);
        private void OnToolRect(object s, RoutedEventArgs e) => SetActiveTool(BtnRect, TileTool.Rect);

        // ── header/size fields ───────────────────────────────────
        private void OnHeaderChanged(object s, TextChangedEventArgs e)
        {
            if (_suppressEvents) return;
            _dirty = true; UpdateTitle();
        }

        private void OnSizeChanged(object s, TextChangedEventArgs e)
        {
            if (_suppressEvents) return;
        }

        private void OnResize(object s, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtWidth.Text, out int w) || !int.TryParse(TxtHeight.Text, out int h)) return;
            _map.ResizeTiles(Math.Clamp(w, 1, 255), Math.Clamp(h, 1, 255));
            Canvas.SetMap(_map);
            _dirty = true; UpdateTitle();
            SetStatus($"Resized to {_map.Width}×{_map.Height}");
        }

        // ── asset dropdowns ──────────────────────────────────────
        private void OnTilemapChanged(object s, SelectionChangedEventArgs e)
        {
            if (_suppressEvents) return;
            int idx = CmbTilemap.SelectedIndex;
            _map.TilemapIdx = (byte)idx;
            TxtTilemapIdx.Text = $"#{idx}";
            LoadTileset(idx);
            _dirty = true; UpdateTitle();
        }

        private void OnBgChanged(object s, SelectionChangedEventArgs e)
        {
            if (_suppressEvents) return;
            int idx = CmbBg.SelectedIndex;
            _map.BgIdx = (byte)idx;
            TxtBgIdx.Text = $"#{idx}";
            _dirty = true; UpdateTitle();
        }

        private void OnBgmChanged(object s, SelectionChangedEventArgs e)
        {
            if (_suppressEvents) return;
            int idx = CmbBgm.SelectedIndex;
            _map.BgmIdx = (byte)idx;
            TxtBgmIdx.Text = $"#{idx}";
            _dirty = true; UpdateTitle();
        }

        // ── tileset picking ──────────────────────────────────────
        private void OnTilesetPicked(int tile)
        {
            Canvas.SelectedTile = tile;
            TxtTileIdx.Text = $"Tile: {tile}";
        }

        private void OnTilePicked(int tile)
        {
            TilesetView.SetSelected(tile);
            TxtTileIdx.Text = $"Tile: {tile}";
        }

        // ── collision boxes ──────────────────────────────────────
        private void OnAddBox(object s, RoutedEventArgs e)
        {
            var box = new CollisionBox { X = 0, Y = 0, W = 1, H = 1 };
            _map.CollisionBoxes.Add(box);
            Canvas.SelectBox(box);
            BoxList.SelectedItem = box;
            _dirty = true; UpdateTitle();
        }

        private void OnDupeBox(object s, RoutedEventArgs e)
        {
            if (Canvas.SelectedBox == null) return;
            var src = Canvas.SelectedBox;
            var box = new CollisionBox { X = src.X + 1, Y = src.Y + 1, W = src.W, H = src.H };
            _map.CollisionBoxes.Add(box);
            Canvas.SelectBox(box);
            BoxList.SelectedItem = box;
            _dirty = true; UpdateTitle();
        }

        private void OnDeleteBox(object s, RoutedEventArgs e)
        {
            if (Canvas.SelectedBox == null) return;
            _map.CollisionBoxes.Remove(Canvas.SelectedBox);
            Canvas.SelectBox(null);
            BoxPropsPanel.Visibility = Visibility.Collapsed;
            _dirty = true; UpdateTitle();
        }

        private void OnBoxListSelected(object s, SelectionChangedEventArgs e)
        {
            if (BoxList.SelectedItem is CollisionBox box)
            {
                Canvas.SelectBox(box);
                ShowBoxProps(box);
            }
        }

        private void OnCanvasSelectionChanged(CollisionBox? box)
        {
            BoxList.SelectedItem = box;
            if (box != null) ShowBoxProps(box);
            else BoxPropsPanel.Visibility = Visibility.Collapsed;
        }

        private bool _suppressBoxEvents;
        private void ShowBoxProps(CollisionBox box)
        {
            _suppressBoxEvents = true;
            BoxPropsPanel.Visibility = Visibility.Visible;
            TxtBoxIdx.Text = $"BOX #{_map.CollisionBoxes.IndexOf(box)}";
            BoxX.Text = box.X.ToString();
            BoxY.Text = box.Y.ToString();
            BoxW.Text = box.W.ToString();
            BoxH.Text = box.H.ToString();
            _suppressBoxEvents = false;
        }

        private void OnBoxPropChanged(object s, TextChangedEventArgs e)
        {
            if (_suppressBoxEvents || Canvas.SelectedBox == null) return;
            var box = Canvas.SelectedBox;
            if (int.TryParse(BoxX.Text, out int x)) box.X = x;
            if (int.TryParse(BoxY.Text, out int y)) box.Y = y;
            if (int.TryParse(BoxW.Text, out int w)) box.W = w;
            if (int.TryParse(BoxH.Text, out int h)) box.H = h;
            Canvas.InvalidateVisual();
            _dirty = true; UpdateTitle();
        }

        // ── keyboard shortcuts ───────────────────────────────────
        private void OnKeyDown(object s, KeyEventArgs e)
        {
            bool ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            if (ctrl && e.Key == Key.Z) { Canvas.Undo(); e.Handled = true; return; }
            if (ctrl && e.Key == Key.Y) { Canvas.Redo(); e.Handled = true; return; }
            if (ctrl && e.Key == Key.S) { OnSave(this, new()); e.Handled = true; return; }
            if (ctrl && e.Key == Key.O) { OnOpen(this, new()); e.Handled = true; return; }
            if (ctrl && e.Key == Key.N) { OnNew(this, new()); e.Handled = true; return; }

            if (e.Key == Key.Tab) { if (Canvas.Mode == EditorMode.Tile) OnModeColl(this, new()); else OnModeTile(this, new()); e.Handled = true; return; }
            if (e.Key == Key.D) { OnToolDraw(this, new()); e.Handled = true; }
            if (e.Key == Key.E) { OnToolErase(this, new()); e.Handled = true; }
            if (e.Key == Key.F) { OnToolFill(this, new()); e.Handled = true; }
            if (e.Key == Key.I) { OnToolPick(this, new()); e.Handled = true; }
            if (e.Key == Key.R) { OnToolRect(this, new()); e.Handled = true; }
            if ((e.Key == Key.Delete || e.Key == Key.Back) && Canvas.Mode == EditorMode.Collision)
            { OnDeleteBox(this, new()); e.Handled = true; }
            if (e.Key == Key.OemPlus || e.Key == Key.Add) { OnZoomIn(this, new()); e.Handled = true; }
            if (e.Key == Key.OemMinus || e.Key == Key.Subtract) { OnZoomOut(this, new()); e.Handled = true; }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (_dirty && !ConfirmDiscard()) e.Cancel = true;
        }

        private void SetStatus(string msg) => StatusLabel.Text = msg;
    }
}

using System.IO;
using System.Text;

namespace XSMEditor
{
    public static class XsmSerializer
    {
        private static readonly byte[] Magic = { 0x58, 0x53, 0x4D, 0x31 }; // XSM1

        public static XsmMap Load(string path)
        {
            using var fs = File.OpenRead(path);
            using var br = new BinaryReader(fs);

            var magic = br.ReadBytes(4);
            if (magic[0] != 'X' || magic[1] != 'S' || magic[2] != 'M' || magic[3] != '1')
                throw new InvalidDataException("Not a valid XSM1 file.");

            var map = new XsmMap();

            map.Name   = ReadFixedString(br, 16);
            map.Author = ReadFixedString(br, 16);
            map.Width  = br.ReadUInt16();
            map.Height = br.ReadUInt16();
            map.TilemapIdx = br.ReadByte();
            map.BgIdx      = br.ReadByte();
            map.BgmIdx     = br.ReadByte();
            int objCount   = br.ReadByte();

            int tileCount = map.Width * map.Height;
            map.Tiles = new MapTile[tileCount];
            for (int i = 0; i < tileCount; i++)
                map.Tiles[i] = new MapTile { Tile = br.ReadByte() };

            for (int i = 0; i < objCount; i++)
            {
                ushort x = br.ReadUInt16();
                ushort y = br.ReadUInt16();
                ushort w = br.ReadUInt16();
                ushort h = br.ReadUInt16();
                map.CollisionBoxes.Add(new CollisionBox { X = x, Y = y, W = w, H = h });
            }

            return map;
        }

        public static void Save(XsmMap map, string path)
        {
            using var fs = File.Create(path);
            using var bw = new BinaryWriter(fs);

            bw.Write(Magic);
            WriteFixedString(bw, map.Name, 16);
            WriteFixedString(bw, map.Author, 16);
            bw.Write((ushort)map.Width);
            bw.Write((ushort)map.Height);
            bw.Write(map.TilemapIdx);
            bw.Write(map.BgIdx);
            bw.Write(map.BgmIdx);
            bw.Write((byte)Math.Min(map.CollisionBoxes.Count, 255));

            foreach (var t in map.Tiles)
                bw.Write(t.Tile);

            foreach (var c in map.CollisionBoxes)
            {
                bw.Write((ushort)c.X);
                bw.Write((ushort)c.Y);
                bw.Write((ushort)c.W);
                bw.Write((ushort)c.H);
            }
        }

        private static string ReadFixedString(BinaryReader br, int len)
        {
            var bytes = br.ReadBytes(len);
            int end = Array.IndexOf(bytes, (byte)0);
            return Encoding.ASCII.GetString(bytes, 0, end < 0 ? len : end);
        }

        private static void WriteFixedString(BinaryWriter bw, string s, int len)
        {
            var bytes = new byte[len];
            var src = Encoding.ASCII.GetBytes(s);
            Array.Copy(src, bytes, Math.Min(src.Length, len));
            bw.Write(bytes);
        }
    }
}

using System.IO;
using System.Text;
using ToxicRagers.Helpers;
using ToxicRagers.Stainless.Formats;

namespace BsfEditor.Model
{
    public static class BsfExtensions
    {
        #region Public members
        // This extension method can be removed when it's added to ToxicRagers NuGet package (I made a PR)
        public static void Save(this BSF bsf, string path)
        {
            var fileInfo = new FileInfo(path);
            Logger.LogToFile(Logger.LogLevel.Info, $"BSF saving {path}");

            using (var writer = new BinaryWriter(File.Create(fileInfo.FullName), Encoding.Unicode))
            {
                writer.Write((byte)0x42); // B
                writer.Write((byte)0x5a); // Z
                writer.Write((byte)0x42); // B
                writer.Write((byte)0x54); // T

                //no clue what these mean
                writer.Write((ushort)1);
                writer.Write((ushort)2);
                writer.Write((uint)16);
                writer.Write((uint)0);

                foreach (var kvp in bsf)
                {
                    var key = kvp.Key;
                    var value = kvp.Value;
                    writer.Write((byte)0); //all entries seem prefixed with a 0 byte
                    writer.Write((byte)key.Length);
                    writer.Write((short)value.Length); //odd, not ushort? possible mistake in the read function? depends on endianness
                    writer.Write(key.ToCharArray());
                    writer.Write(value.ToCharArray());
                }
            }
        }
        #endregion
    }
}

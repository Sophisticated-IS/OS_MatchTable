using System.IO;
using System.IO.Compression;

namespace MessageHandler
{
    public static class ByteCompressor
    {
        public static byte[] Compress(byte[] data)
        {
            var output = new MemoryStream();
            using (var deflateStream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                deflateStream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public static byte[] DeCompress(byte[] data)
        {
            var input = new MemoryStream(data);
            var output = new MemoryStream();
            using (var deflateStream = new DeflateStream(input, CompressionMode.Decompress))
            {
                deflateStream.CopyTo(output);
            }
            return output.ToArray();
        }
    }
}
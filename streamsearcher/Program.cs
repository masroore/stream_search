using System;
using System.IO;
using System.Text;

namespace streamsearcher
{
    internal class Program
    {
        public static void ReplaceData(string filename, int position, byte[] data)
        {
            using (Stream stream = File.Open(filename, FileMode.Open))
            {
                stream.Position = position;
                stream.Write(data, 0, data.Length);
            }
        }

        private static void Main(string[] args)
        {
            var searchString  = "Optimized by JPEGmini 3.9.2.5L Internal 0x";
            var replaceString = "Optimized by JPEGCrunchr 1.0.1                    ";

            var searchBytes = Encoding.ASCII.GetBytes(searchString);
            var replaceBytes = Encoding.ASCII.GetBytes(replaceString);

            var searcher = new StreamSearcher(searchBytes);

            foreach (var fname in new[] {"1.jpg", "2.jpg", "3.jpg", "4.jpg"})
            {
                using (var fs = File.Open(fname, FileMode.Open))
                {
                    var position = searcher.Search(fs);
                    Console.WriteLine(position);
                    if (position > 0)
                    {
                        fs.Seek(position - searchBytes.Length, SeekOrigin.Begin);
                        fs.Write(replaceBytes, 0, replaceBytes.Length);
                    }
                }
            }
        }
    }
}
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace Utils
{
    public class FileUtil
    {
        public static string GetFileMD5(string path)
        {
            var hash = MD5.Create();
            var stream = new FileStream(path, FileMode.Open);
            byte[] hashByte = hash.ComputeHash(stream);
            stream.Close();
            hash.Dispose();
            return BitConverter.ToString(hashByte).ToLower().Replace("-", "");
        }

        public static string GetMD5(string content)
        {
            var data = Encoding.UTF8.GetBytes(content);
            return GetMD5(data);
        }

        public static string GetMD5(byte[] data)
        {
            var hash = MD5.Create();
            byte[] hashByte = hash.ComputeHash(data);
            hash.Dispose();
            return BitConverter.ToString(hashByte).ToLower().Replace("-", "");
        }
    }
}

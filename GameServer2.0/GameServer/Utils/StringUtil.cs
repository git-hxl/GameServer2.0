
using System;
using System.Net;
using System.Text.RegularExpressions;

namespace Utils
{
    public class StringUtil
    {
        public static IPAddress ParseIP(string str)
        {
            IPAddress ip;
            if (!IPAddress.TryParse(str, out ip))
            {
                IPHostEntry hostInfo = Dns.GetHostEntry(str);
                ip = hostInfo.AddressList[0];
            }
            return ip;
        }
        /// <summary>
        /// ֻ������ĸ����
        /// </summary>
        /// <param name="s">�Ƿ��Ƴ��ո�</param>
        /// <returns></returns>
        public static string ToAlphaNumber(string str, bool isTrim = true)
        {
            string pattern = @"[^a-zA-Z0-9\s]";
            if (isTrim)
                pattern = @"[^a-zA-Z0-9]";
            str = Regex.Replace(str, pattern, "");
            return str;
        }
        /// <summary>
        /// ֻ������ĸ���ֺ���
        /// </summary>
        /// <param name="s">�Ƿ��Ƴ��ո�</param>
        /// <returns></returns>
        public static string ToAlphaNumberAndChinese(string str, bool isTrim = true)
        {
            string pattern = @"[^a-zA-Z0-9\u4e00-\u9fa5\s]";
            if (isTrim)
                pattern = @"[^a-zA-Z0-9\u4e00-\u9fa5]";
            str = Regex.Replace(str, pattern, "");
            return str;
        }

        /// <summary>
        /// Unicodeת����
        /// </summary>
        /// <returns></returns>
        public static string UnicodeToChinese(string str)
        {
            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(
                  str, x => string.Empty + Convert.ToChar(Convert.ToInt32(x.Result("$1"), 16)));
        }

        public static bool IsChinese(string str)
        {
            return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
        }
    }
}

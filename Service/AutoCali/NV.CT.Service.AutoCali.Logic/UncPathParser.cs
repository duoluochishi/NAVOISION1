using System;
using System.Net;

namespace NV.CT.Service.AutoCali.Logic
{
    /// <summary>
    /// 共享路径解析
    /// 示例，string uncPath = @"\\192.168.1.1\共享文件夹";
    /// string ipAddress = UncPathParser.GetIPFromUncPath(uncPath);
    /// Console.WriteLine(ipAddress); // 输出: 192.168.1.1
    /// </summary>
    public class UncPathParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uncPath"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string GetIPFromUncPath(string uncPath)
        {
            if (string.IsNullOrWhiteSpace(uncPath))
                throw new ArgumentException("UNC路径不能为空或空白。", nameof(uncPath));

            if (!uncPath.StartsWith(@"\\"))
                throw new ArgumentException("提供的路径不是有效的UNC路径。", nameof(uncPath));

            // 提取服务器部分
            int startIndex = 2;
            int endIndex = uncPath.IndexOf('\\', startIndex);
            string serverPart = endIndex == -1
                ? uncPath.Substring(startIndex)
                : uncPath.Substring(startIndex, endIndex - startIndex);

            // 验证是否为有效的IP地址
            if (IPAddress.TryParse(serverPart, out IPAddress ipAddress))
            {
                return ipAddress.ToString();
            }
            else
            {
                throw new ArgumentException("服务器部分不是IP地址。", nameof(uncPath));
            }
        }
    }
}

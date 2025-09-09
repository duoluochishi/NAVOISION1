using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NV.CT.Service.Upgrade.Utilities
{
    public static class MD5Utility
    {
        public static string GetMD5FromFile(string path)
        {
            try
            {
                var sb = new StringBuilder();
                using var file = new FileStream(path, FileMode.Open);
                using var md5 = MD5.Create();
                var retVal = md5.ComputeHash(file);

                foreach (var ret in retVal)
                {
                    sb.Append(ret.ToString("x2"));
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }

        public static bool IsMatchFromFile(string path, string md5)
        {
            var fileMD5 = GetMD5FromFile(path);
            return fileMD5.Equals(md5);
        }
    }
}
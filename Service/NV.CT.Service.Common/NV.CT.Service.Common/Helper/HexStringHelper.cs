using System;
using System.Text;

namespace NV.CT.Service.Common.Helper
{
    public static class HexStringHelper
    {
        public static byte[] HexStringToBytes(string hexString)
        {
            var bytes = new byte[hexString.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return bytes;
        }

        public static string BytesToHexString(byte[] bytes)
        {
            var builder = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("X2"));
                builder.Append(" ");
            }

            return builder.ToString();
        }
    }
}
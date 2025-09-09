using System.Text;

namespace NV.CT.Service.HardwareTest.Share.Utils
{
    public static class StringBytesUtils
    {
        public static string ToHexStringFromBytes(this byte[] bytes) 
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++) 
            {
                builder.Append(string.Format("{0:X2}", bytes[i]));
            }

            return builder.ToString().Trim();
        }

        public static string ToFormatString(this IEnumerable<int> array)
        {
            return string.Join(",", array);
        }

        public static string ToFormatString(this double[] doubles) 
        {
            return string.Join(",", doubles.Select(t => Math.Round((double)t, 2)));
        }

    }
}

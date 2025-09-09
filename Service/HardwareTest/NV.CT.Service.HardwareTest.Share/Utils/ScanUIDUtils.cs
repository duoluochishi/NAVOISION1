using System.Text;

namespace NV.CT.Service.HardwareTest.Share.Utils
{
    public class ScanUIDUtils
    {
        public static byte[] UintToGuid(uint[] numbers)
        {
            if (numbers.Length != 4)
            {
                throw new ArgumentException("guid need 4 numbers");
            }

            byte[] result = new byte[16];
            for (int i = 0; i < result.Length; i++)
            {
                uint tmp = numbers[i / 4];
                result[i] = (byte)((tmp >> (3 - i % 4) * 8) & 0xff);
            }

            return result;
        }

        public static uint[] GuidToUint(byte[] num)
        {
            if (num.Length != 16)
            {
                throw new ArgumentException("length of guid must be 16");
            }

            uint[] result = new uint[4];
            for (int i = 0; i < num.Length; i++)
            {
                int j = i / 4;
                int k = 3 - i % 4;
                result[j] += (uint)(((int)num[i]) << k * 8);
            }

            return result;
        }

        public static uint[] GuidToProtoBytes(Guid id)
        {
            /** 转byte数组 **/
            byte[] bytes = id.ToByteArray();
            /** 初始化uint数组 **/
            uint[] uints = new uint[bytes.Length / sizeof(uint)];
            /** 复制 **/
            Buffer.BlockCopy(bytes, 0, uints, 0, bytes.Length);

            return uints;
        }

        public static Guid ProtoBytesToGuid(uint[] protoBytes)
        {
            if (protoBytes is null || protoBytes.Length < 4)
            {
                throw new ArgumentException($"Invalid input, method: {nameof(ProtoBytesToGuid)}.");
            }
            /** 初始化byte数组 **/
            byte[] bytes = new byte[protoBytes.Length * sizeof(uint)];
            /** 复制 **/
            Buffer.BlockCopy(protoBytes, 0, bytes, 0, bytes.Length);

            return new Guid(bytes);
        }

        public static uint[] ScanUIDToProtoBytes(string scanUID) 
        {
            /** UID长度 **/
            const int ScanUIDLength = 16;
            /** UID转bytes **/
            byte[] protoBytes = Encoding.ASCII.GetBytes(scanUID);
            /** 临时空间 **/
            byte[] temp = new byte[ScanUIDLength];
            /** 16位 **/
            Buffer.BlockCopy(protoBytes, 0, temp, 0, ScanUIDLength);
            
            return ScanUIDUtils.GuidToUint(temp);
        }

    }
}

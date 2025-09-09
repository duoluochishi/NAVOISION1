using System.Runtime.InteropServices;

namespace NV.CT.Service.HardwareTest.Share.Utils
{
    public static class ByteUtils
    {
        public static byte GetBit(uint value, int index) 
        {
            return (byte)((value >> index) & 0x01);
        }

        public static byte Uint32ToByte1(uint type)
        {
            return (byte)((type & 0xFF000000) >> 8 >> 8 >> 8);
        }

        public static byte Uint32ToByte2(uint type)
        {
            return (byte)((type & 0x00FF0000) >> 8 >> 8);
        }

        public static byte Uint32ToByte3(uint type)
        {
            return (byte)((type & 0xFF00) >> 8);
        }

        public static byte Uint32ToByte4(uint type)
        {
            return (byte)(type & 0xFF);
        }

        public static byte[] FloatsToBytes(float[] floatArray) 
        {
            /** 初始化byte数组 **/
            byte[] byteArray = new byte[floatArray.Length * sizeof(float)];
            /** 复制 **/
            Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);

            return byteArray;
        }

        public static T? BytesToStruct<T>(byte[] bytes) where T : struct
        {
            /** Struct Size **/
            int size = Marshal.SizeOf(typeof(T));
            /** 长度校验 **/
            if (bytes.Length < size)
            {
                return default;
            }
            //分配结构体大小的内存空间
            IntPtr structPointer = Marshal.AllocHGlobal(size);
            // 将byte数组拷到分配好的内存空间
            Marshal.Copy(bytes, 0, structPointer, size);
            //将内存空间转换为目标结构体
            T? obj = Marshal.PtrToStructure<T>(structPointer);
            //释放内存空间
            Marshal.FreeHGlobal(structPointer);
            // 返回结构体
            return obj;
        }

    }
}

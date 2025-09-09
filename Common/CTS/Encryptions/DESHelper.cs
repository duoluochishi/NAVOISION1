//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/2/5 14:46:39    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using System.Security.Cryptography;
using System.Text;

namespace NV.CT.CTS.Encryptions;

public static class DESHelper
{
	private static readonly string Password = "&*@@#Y@jfeji$$$$(()))";
	/// <summary>
	/// 从密码派生出合法的 DES Key 和 IV（均为8字节）
	/// </summary>
	public static (byte[] key, byte[] iv) GetKeyAndIV(string password)
	{
		using var md5 = MD5.Create();
		var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(password));

		byte[] key = hash.Take(8).ToArray();
		byte[] iv = hash.Skip(8).Take(8).ToArray();

		if (iv.Length < 8)
			iv = key.Reverse().Take(8).ToArray();

		return (key, iv);
	}

	/// <summary>
	/// 字节数组转十六进制字符串
	/// </summary>
	public static string ToHexString(byte[] data) =>
		BitConverter.ToString(data).Replace("-", "");

	/// <summary>
	/// 十六进制字符串转字节数组
	/// </summary>
	public static byte[] FromHexString(string hex)
	{
		int len = hex.Length / 2;
		byte[] result = new byte[len];
		for (int i = 0; i < len; i++)
			result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
		return result;
	}

	/// <summary>
	/// 加密字符串，返回十六进制密文
	/// </summary>
	public static string EncryptString(string plainText, CipherMode mode = CipherMode.ECB, PaddingMode padding = PaddingMode.PKCS7)
	{
		var plainBytes = Encoding.UTF8.GetBytes(plainText);
		var (key, iv) = GetKeyAndIV(Password);
		var encrypted = Encrypt(plainBytes, key, iv, mode, padding);
		return ToHexString(encrypted);
	}

	/// <summary>
	/// 解密十六进制密文，返回明文字符串
	/// </summary>
	public static string DecryptString(string hexCipher, CipherMode mode = CipherMode.ECB, PaddingMode padding = PaddingMode.PKCS7)
	{
		var cipherBytes = FromHexString(hexCipher);
		var (key, iv) = GetKeyAndIV(Password);
		var decrypted = Decrypt(cipherBytes, key, iv, mode, padding);
		return Encoding.UTF8.GetString(decrypted);
	}

	/// <summary>
	/// DES 加密（原始字节）
	/// </summary>
	public static byte[] Encrypt(byte[] plainBytes, byte[] key, byte[] iv, CipherMode mode, PaddingMode padding)
	{
		using var des = DES.Create();
		des.Mode = mode;
		des.Padding = padding;
		des.Key = key;
		if (mode == CipherMode.CBC)
			des.IV = iv;

		using var ms = new MemoryStream();
		using var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
		cs.Write(plainBytes, 0, plainBytes.Length);
		cs.FlushFinalBlock();
		return ms.ToArray();
	}

	/// <summary>
	/// DES 解密（原始字节）
	/// </summary>
	public static byte[] Decrypt(byte[] cipherBytes, byte[] key, byte[] iv, CipherMode mode, PaddingMode padding)
	{
		using var des = DES.Create();
		des.Mode = mode;
		des.Padding = padding;
		des.Key = key;
		if (mode == CipherMode.CBC)
			des.IV = iv;

		using var msInput = new MemoryStream(cipherBytes);
		using var cs = new CryptoStream(msInput, des.CreateDecryptor(), CryptoStreamMode.Read);
		using var msOutput = new MemoryStream();
		cs.CopyTo(msOutput);
		return msOutput.ToArray();
	}
}

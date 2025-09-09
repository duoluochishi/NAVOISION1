﻿//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/2/5 14:43:22    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using System.Security.Cryptography;
using System.Text;

namespace NV.CT.CTS.Encryptions;

/// <summary>
/// 类名：RSAFromPKCS1(OpenSSL)
/// 功能：RSA加密、解密、签名、验签
/// 详细：该类对Java生成的密钥进行解密和签名以及验签专用类，不需要修改
/// </summary>
public static class RSAFromPKCS1
{
    private static Encoding defaultCharset = Encoding.UTF8;

    #region 使用私钥签名
    /// <summary>
    /// 使用私钥签名
    /// </summary>
    /// <param name="data">原始数据</param>
    /// <returns></returns>
    public static string Sign(string data, string privateKey, string algorithmName = "SHA1")
    {
        if (string.IsNullOrEmpty(privateKey))
        {
            throw new ArgumentException("The private key is null.");
        }

        if (algorithmName != "SHA1" && algorithmName != "SHA256")
        {
            throw new NotSupportedException("The algorithm name is not supported.");
        }

        byte[] dataBytes = defaultCharset.GetBytes(data);
        var provider = CreateRsaProviderFromPrivateKey(privateKey);
        var hashAlgorithmName = algorithmName == "SHA1" ? HashAlgorithmName.SHA1 : HashAlgorithmName.SHA256;
        var signatureBytes = provider.SignData(dataBytes, hashAlgorithmName, RSASignaturePadding.Pkcs1);

        return Convert.ToBase64String(signatureBytes);
    }

    #endregion

    #region 使用公钥验证签名
    /// <summary>
    /// 使用公钥验证签名
    /// </summary>
    /// <param name="data">原始数据</param>
    /// <param name="sign">签名</param>
    /// <returns></returns>
    public static bool Verify(string data, string sign, string publicKey, string algorithmName = "SHA1")
    {
        if (string.IsNullOrEmpty(publicKey))
        {
            throw new ArgumentException("The public key is null.");
        }

        if (algorithmName != "SHA1" && algorithmName != "SHA256")
        {
            throw new NotSupportedException("The algorithm name is not supported.");
        }

        byte[] dataBytes = defaultCharset.GetBytes(data);
        byte[] signBytes = Convert.FromBase64String(sign);
        var provider = CreateRsaProviderFromPublicKey(publicKey);
        var hashAlgorithmName = algorithmName == "SHA1" ? HashAlgorithmName.SHA1 : HashAlgorithmName.SHA256;

        return provider.VerifyData(dataBytes, signBytes, hashAlgorithmName, RSASignaturePadding.Pkcs1);
    }

    #endregion

    #region 解密
    /// <summary>
    /// 使用私钥解密
    /// </summary>
    /// <param name="cipherText"></param>
    /// <param name="privateKey"></param>
    /// <returns></returns>
    public static string Decrypt(string cipherText, string privateKey)
    {
        if (string.IsNullOrEmpty(privateKey))
        {
            throw new ArgumentException("The private key is null.");
        }

        var provider = CreateRsaProviderFromPrivateKey(privateKey);

        return defaultCharset.GetString(provider.Decrypt(Convert.FromBase64String(cipherText), RSAEncryptionPadding.Pkcs1));
    }

    #endregion

    #region 加密
    /// <summary>
    /// 使用公钥加密
    /// </summary>
    /// <param name="plainText"></param>
    /// <param name="publicKey"></param>
    /// <returns></returns>
    public static string Encrypt(string plainText, string publicKey)
    {
        if (string.IsNullOrEmpty(publicKey))
        {
            throw new ArgumentException("The public key is null.");
        }

        var provider = CreateRsaProviderFromPublicKey(publicKey);

        return Convert.ToBase64String(provider.Encrypt(defaultCharset.GetBytes(plainText), RSAEncryptionPadding.Pkcs1));
    }
    #endregion

    #region 使用私钥创建RSA实例
    private static RSA CreateRsaProviderFromPrivateKey(string privateKey)
    {
        var privateKeyBits = Convert.FromBase64String(privateKey);

        var rsa = RSA.Create();
        var rsaParameters = new RSAParameters();

        using (BinaryReader binr = new BinaryReader(new MemoryStream(privateKeyBits)))
        {
            byte bt = 0;
            ushort twobytes = 0;
            twobytes = binr.ReadUInt16();
            if (twobytes == 0x8130)
                binr.ReadByte();
            else if (twobytes == 0x8230)
                binr.ReadInt16();
            else
                throw new Exception("Unexpected value read binr.ReadUInt16()");

            twobytes = binr.ReadUInt16();
            if (twobytes != 0x0102)
                throw new Exception("Unexpected version");

            bt = binr.ReadByte();
            if (bt != 0x00)
                throw new Exception("Unexpected value read binr.ReadByte()");

            rsaParameters.Modulus = binr.ReadBytes(GetIntegerSize(binr));
            rsaParameters.Exponent = binr.ReadBytes(GetIntegerSize(binr));
            rsaParameters.D = binr.ReadBytes(GetIntegerSize(binr));
            rsaParameters.P = binr.ReadBytes(GetIntegerSize(binr));
            rsaParameters.Q = binr.ReadBytes(GetIntegerSize(binr));
            rsaParameters.DP = binr.ReadBytes(GetIntegerSize(binr));
            rsaParameters.DQ = binr.ReadBytes(GetIntegerSize(binr));
            rsaParameters.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
        }

        rsa.ImportParameters(rsaParameters);
        return rsa;
    }
    #endregion

    #region 使用公钥创建RSA实例
    private static RSA CreateRsaProviderFromPublicKey(string publicKeyString)
    {
        // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
        byte[] seqOid = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
        byte[] seq = new byte[15];

        var x509Key = Convert.FromBase64String(publicKeyString);

        // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
        using (MemoryStream mem = new MemoryStream(x509Key))
        {
            using (BinaryReader binr = new BinaryReader(mem))  //wrap Memory Stream with BinaryReader for easy reading
            {
                byte bt = 0;
                ushort twobytes = 0;

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();    //advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();   //advance 2 bytes
                else
                    return null;

                seq = binr.ReadBytes(15);       //read the Sequence OID
                if (!CompareBytearrays(seq, seqOid))    //make sure Sequence for OID is correct
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
                    binr.ReadByte();    //advance 1 byte
                else if (twobytes == 0x8203)
                    binr.ReadInt16();   //advance 2 bytes
                else
                    return null;

                bt = binr.ReadByte();
                if (bt != 0x00)     //expect null byte next
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();    //advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();   //advance 2 bytes
                else
                    return null;

                twobytes = binr.ReadUInt16();
                byte lowbyte = 0x00;
                byte highbyte = 0x00;

                if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
                    lowbyte = binr.ReadByte();  // read next bytes which is bytes in modulus
                else if (twobytes == 0x8202)
                {
                    highbyte = binr.ReadByte(); //advance 2 bytes
                    lowbyte = binr.ReadByte();
                }
                else
                    return null;
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
                int modsize = BitConverter.ToInt32(modint, 0);

                int firstbyte = binr.PeekChar();
                if (firstbyte == 0x00)
                {   //if first byte (highest order) of modulus is zero, don't include it
                    binr.ReadByte();    //skip this null byte
                    modsize -= 1;   //reduce modulus buffer size by 1
                }

                byte[] modulus = binr.ReadBytes(modsize);   //read the modulus bytes

                if (binr.ReadByte() != 0x02)            //expect an Integer for the exponent data
                    return null;
                int expbytes = (int)binr.ReadByte();        // should only need one byte for actual exponent data (for all useful values)
                byte[] exponent = binr.ReadBytes(expbytes);

                // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                var rsa = RSA.Create();
                RSAParameters rsaKeyInfo = new RSAParameters
                {
                    Modulus = modulus,
                    Exponent = exponent
                };
                rsa.ImportParameters(rsaKeyInfo);

                return rsa;
            }

        }
    }
    #endregion

    #region 导入密钥算法
    private static int GetIntegerSize(BinaryReader binr)
    {
        byte bt = 0;
        int count = 0;
        bt = binr.ReadByte();
        if (bt != 0x02)
            return 0;
        bt = binr.ReadByte();

        if (bt == 0x81)
            count = binr.ReadByte();
        else
        if (bt == 0x82)
        {
            var highbyte = binr.ReadByte();
            var lowbyte = binr.ReadByte();
            byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
            count = BitConverter.ToInt32(modint, 0);
        }
        else
        {
            count = bt;
        }

        while (binr.ReadByte() == 0x00)
        {
            count -= 1;
        }
        binr.BaseStream.Seek(-1, SeekOrigin.Current);
        return count;
    }

    private static bool CompareBytearrays(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;
        int i = 0;
        foreach (byte c in a)
        {
            if (c != b[i])
                return false;
            i++;
        }
        return true;
    }
    #endregion
}

/// <summary>
/// 类名：RSAFromPKCS8
/// 功能：RSA加密、解密、签名、验签
/// 详细：该类对Java生成的密钥进行解密和签名以及验签专用类，不需要修改
/// 版本：3.0
/// </summary>
public static class RSAFromPKCS8
{
    private static Encoding defaultChartset = Encoding.UTF8;

    /// <summary>
    /// 签名
    /// </summary>
    /// <param name="content">待签名字符串</param>
    /// <param name="privateKey">私钥</param>
    /// <returns>签名后字符串</returns>
    public static string Sign(string content, string privateKey)
    {
        byte[] Data = defaultChartset.GetBytes(content);
        RSACryptoServiceProvider rsa = DecodePemPrivateKey(privateKey);
        var sh = SHA1.Create();
        byte[] signData = rsa.SignData(Data, sh);
        return Convert.ToBase64String(signData);
    }

    /// <summary>
    /// 验签
    /// </summary>
    /// <param name="content">待验签字符串</param>
    /// <param name="signedString">签名</param>
    /// <param name="publicKey">公钥</param>
    /// <returns>true(通过)，false(不通过)</returns>
    public static bool Verify(string content, string signedString, string publicKey)
    {
        bool result = false;
        byte[] Data = defaultChartset.GetBytes(content);
        byte[] data = Convert.FromBase64String(signedString);
        RSAParameters paraPub = ConvertFromPublicKey(publicKey);
        RSACryptoServiceProvider rsaPub = new RSACryptoServiceProvider();
        rsaPub.ImportParameters(paraPub);
        var sh = SHA1.Create();
        result = rsaPub.VerifyData(Data, sh, data);
        return result;
    }

    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="resData">需要加密的字符串</param>
    /// <param name="publicKey">公钥</param>
    /// <returns>明文</returns>
    public static string Encrypt(string resData, string publicKey)
    {
        byte[] DataToEncrypt = defaultChartset.GetBytes(resData);
        string result = encrypt(DataToEncrypt, publicKey);
        return result;
    }


    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="resData">加密字符串</param>
    /// <param name="privateKey">私钥</param>
    /// <returns>明文</returns>
    public static string Decrypt(string resData, string privateKey)
    {
        byte[] DataToDecrypt = Convert.FromBase64String(resData);
        string result = "";
        for (int j = 0; j < DataToDecrypt.Length / 128; j++)
        {
            byte[] buf = new byte[128];
            for (int i = 0; i < 128; i++)
            {

                buf[i] = DataToDecrypt[i + 128 * j];
            }
            result += decrypt(buf, privateKey);
        }
        return result;
    }

    #region 内部方法

    private static string encrypt(byte[] data, string publicKey)
    {
        RSACryptoServiceProvider rsa = DecodePemPublicKey(publicKey);
        var sh = SHA1.Create();
        byte[] result = rsa.Encrypt(data, false);

        return Convert.ToBase64String(result);
    }

    private static string decrypt(byte[] data, string privateKey)
    {
        string result = "";
        RSACryptoServiceProvider rsa = DecodePemPrivateKey(privateKey);
        var sh = SHA1.Create();
        byte[] source = rsa.Decrypt(data, false);
        char[] asciiChars = new char[defaultChartset.GetCharCount(source, 0, source.Length)];
        defaultChartset.GetChars(source, 0, source.Length, asciiChars, 0);
        result = new string(asciiChars);
        return result;
    }

    private static RSACryptoServiceProvider DecodePemPublicKey(String pemstr)
    {
        byte[] pkcs8publickkey;
        pkcs8publickkey = Convert.FromBase64String(pemstr);
        if (pkcs8publickkey is not null)
        {
            RSACryptoServiceProvider rsa = DecodeRSAPublicKey(pkcs8publickkey);
            return rsa;
        }
        else
            return null;
    }

    private static RSACryptoServiceProvider DecodePemPrivateKey(String pemstr)
    {
        byte[] pkcs8privatekey;
        pkcs8privatekey = Convert.FromBase64String(pemstr);
        if (pkcs8privatekey is not null)
        {
            RSACryptoServiceProvider rsa = DecodePrivateKeyInfo(pkcs8privatekey);
            return rsa;
        }
        else
            return null;
    }

    private static RSACryptoServiceProvider DecodePrivateKeyInfo(byte[] pkcs8)
    {
        byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
        byte[] seq = new byte[15];

        MemoryStream mem = new MemoryStream(pkcs8);
        int lenstream = (int)mem.Length;
        BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
        byte bt = 0;
        ushort twobytes = 0;

        try
        {
            twobytes = binr.ReadUInt16();
            if (twobytes == 0x8130)    //data read as little endian order (actual data order for Sequence is 30 81)
                binr.ReadByte();    //advance 1 byte
            else if (twobytes == 0x8230)
                binr.ReadInt16();    //advance 2 bytes
            else
                return null;

            bt = binr.ReadByte();
            if (bt != 0x02)
                return null;

            twobytes = binr.ReadUInt16();

            if (twobytes != 0x0001)
                return null;

            seq = binr.ReadBytes(15);        //read the Sequence OID
            if (!CompareBytearrays(seq, SeqOID))    //make sure Sequence for OID is correct
                return null;

            bt = binr.ReadByte();
            if (bt != 0x04)    //expect an Octet string
                return null;

            bt = binr.ReadByte();        //read next byte, or next 2 bytes is  0x81 or 0x82; otherwise bt is the byte count
            if (bt == 0x81)
                binr.ReadByte();
            else
                if (bt == 0x82)
                binr.ReadUInt16();
            //------ at this stage, the remaining sequence should be the RSA private key

            byte[] rsaprivkey = binr.ReadBytes((int)(lenstream - mem.Position));
            RSACryptoServiceProvider rsacsp = DecodeRSAPrivateKey(rsaprivkey);
            return rsacsp;
        }

        catch (Exception)
        {
            return null;
        }

        finally { binr.Close(); }

    }

    private static bool CompareBytearrays(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;
        int i = 0;
        foreach (byte c in a)
        {
            if (c != b[i])
                return false;
            i++;
        }
        return true;
    }

    private static RSACryptoServiceProvider DecodeRSAPublicKey(byte[] publickey)
    {
        // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
        byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
        byte[] seq = new byte[15];
        // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
        MemoryStream mem = new MemoryStream(publickey);
        BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
        byte bt = 0;
        ushort twobytes = 0;

        try
        {

            twobytes = binr.ReadUInt16();
            if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                binr.ReadByte();    //advance 1 byte
            else if (twobytes == 0x8230)
                binr.ReadInt16();   //advance 2 bytes
            else
                return null;

            seq = binr.ReadBytes(15);       //read the Sequence OID
            if (!CompareBytearrays(seq, SeqOID))    //make sure Sequence for OID is correct
                return null;

            twobytes = binr.ReadUInt16();
            if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
                binr.ReadByte();    //advance 1 byte
            else if (twobytes == 0x8203)
                binr.ReadInt16();   //advance 2 bytes
            else
                return null;

            bt = binr.ReadByte();
            if (bt != 0x00)     //expect null byte next
                return null;

            twobytes = binr.ReadUInt16();
            if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                binr.ReadByte();    //advance 1 byte
            else if (twobytes == 0x8230)
                binr.ReadInt16();   //advance 2 bytes
            else
                return null;

            twobytes = binr.ReadUInt16();
            byte lowbyte = 0x00;
            byte highbyte = 0x00;

            if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
                lowbyte = binr.ReadByte();  // read next bytes which is bytes in modulus
            else if (twobytes == 0x8202)
            {
                highbyte = binr.ReadByte(); //advance 2 bytes
                lowbyte = binr.ReadByte();
            }
            else
                return null;
            byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
            int modsize = BitConverter.ToInt32(modint, 0);

            byte firstbyte = binr.ReadByte();
            binr.BaseStream.Seek(-1, SeekOrigin.Current);

            if (firstbyte == 0x00)
            {   //if first byte (highest order) of modulus is zero, don't include it
                binr.ReadByte();    //skip this null byte
                modsize -= 1;   //reduce modulus buffer size by 1
            }

            byte[] modulus = binr.ReadBytes(modsize);   //read the modulus bytes

            if (binr.ReadByte() != 0x02)            //expect an Integer for the exponent data
                return null;
            int expbytes = (int)binr.ReadByte();        // should only need one byte for actual exponent data (for all useful values)
            byte[] exponent = binr.ReadBytes(expbytes);

            // ------- create RSACryptoServiceProvider instance and initialize with public key -----
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            RSAParameters RSAKeyInfo = new RSAParameters();
            RSAKeyInfo.Modulus = modulus;
            RSAKeyInfo.Exponent = exponent;
            RSA.ImportParameters(RSAKeyInfo);
            return RSA;
        }
        catch (Exception)
        {
            return null;
        }

        finally { binr.Close(); }

    }

    private static RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey)
    {
        byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

        // ---------  Set up stream to decode the asn.1 encoded RSA private key  ------
        MemoryStream mem = new MemoryStream(privkey);
        BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
        byte bt = 0;
        ushort twobytes = 0;
        int elems = 0;
        try
        {
            twobytes = binr.ReadUInt16();
            if (twobytes == 0x8130)    //data read as little endian order (actual data order for Sequence is 30 81)
                binr.ReadByte();    //advance 1 byte
            else if (twobytes == 0x8230)
                binr.ReadInt16();    //advance 2 bytes
            else
                return null;

            twobytes = binr.ReadUInt16();
            if (twobytes != 0x0102)    //version number
                return null;
            bt = binr.ReadByte();
            if (bt != 0x00)
                return null;


            //------  all private key components are Integer sequences ----
            elems = GetIntegerSize(binr);
            MODULUS = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            E = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            D = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            P = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            Q = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            DP = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            DQ = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            IQ = binr.ReadBytes(elems);

            // ------- create RSACryptoServiceProvider instance and initialize with public key -----
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            RSAParameters RSAparams = new RSAParameters();
            RSAparams.Modulus = MODULUS;
            RSAparams.Exponent = E;
            RSAparams.D = D;
            RSAparams.P = P;
            RSAparams.Q = Q;
            RSAparams.DP = DP;
            RSAparams.DQ = DQ;
            RSAparams.InverseQ = IQ;
            RSA.ImportParameters(RSAparams);
            return RSA;
        }
        catch (Exception)
        {
            return null;
        }
        finally { binr.Close(); }
    }

    private static int GetIntegerSize(BinaryReader binr)
    {
        byte bt = 0;
        byte lowbyte = 0x00;
        byte highbyte = 0x00;
        int count = 0;
        bt = binr.ReadByte();
        if (bt != 0x02)        //expect integer
            return 0;
        bt = binr.ReadByte();

        if (bt == 0x81)
            count = binr.ReadByte();    // data size in next byte
        else
            if (bt == 0x82)
        {
            highbyte = binr.ReadByte();    // data size in next 2 bytes
            lowbyte = binr.ReadByte();
            byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
            count = BitConverter.ToInt32(modint, 0);
        }
        else
        {
            count = bt;        // we already have the data size
        }



        while (binr.ReadByte() == 0x00)
        {    //remove high order zeros in data
            count -= 1;
        }
        binr.BaseStream.Seek(-1, SeekOrigin.Current);        //last ReadByte wasn't a removed zero, so back up a byte
        return count;
    }

    #endregion

    #region 解析.net 生成的Pem
    private static RSAParameters ConvertFromPublicKey(string pemFileConent)
    {

        byte[] keyData = Convert.FromBase64String(pemFileConent);
        if (keyData.Length < 162)
        {
            throw new ArgumentException("pem file content is incorrect.");
        }
        byte[] pemModulus = new byte[128];
        byte[] pemPublicExponent = new byte[3];
        Array.Copy(keyData, 29, pemModulus, 0, 128);
        Array.Copy(keyData, 159, pemPublicExponent, 0, 3);
        RSAParameters para = new RSAParameters();
        para.Modulus = pemModulus;
        para.Exponent = pemPublicExponent;
        return para;
    }

    private static RSAParameters ConvertFromPrivateKey(string pemFileConent)
    {
        byte[] keyData = Convert.FromBase64String(pemFileConent);
        if (keyData.Length < 609)
        {
            throw new ArgumentException("pem file content is incorrect.");
        }

        int index = 11;
        byte[] pemModulus = new byte[128];
        Array.Copy(keyData, index, pemModulus, 0, 128);

        index += 128;
        index += 2;//141
        byte[] pemPublicExponent = new byte[3];
        Array.Copy(keyData, index, pemPublicExponent, 0, 3);

        index += 3;
        index += 4;//148
        byte[] pemPrivateExponent = new byte[128];
        Array.Copy(keyData, index, pemPrivateExponent, 0, 128);

        index += 128;
        index += ((int)keyData[index + 1] == 64 ? 2 : 3);//279
        byte[] pemPrime1 = new byte[64];
        Array.Copy(keyData, index, pemPrime1, 0, 64);

        index += 64;
        index += ((int)keyData[index + 1] == 64 ? 2 : 3);//346
        byte[] pemPrime2 = new byte[64];
        Array.Copy(keyData, index, pemPrime2, 0, 64);

        index += 64;
        index += ((int)keyData[index + 1] == 64 ? 2 : 3);//412/413
        byte[] pemExponent1 = new byte[64];
        Array.Copy(keyData, index, pemExponent1, 0, 64);

        index += 64;
        index += ((int)keyData[index + 1] == 64 ? 2 : 3);//479/480
        byte[] pemExponent2 = new byte[64];
        Array.Copy(keyData, index, pemExponent2, 0, 64);

        index += 64;
        index += ((int)keyData[index + 1] == 64 ? 2 : 3);//545/546
        byte[] pemCoefficient = new byte[64];
        Array.Copy(keyData, index, pemCoefficient, 0, 64);

        RSAParameters para = new RSAParameters();
        para.Modulus = pemModulus;
        para.Exponent = pemPublicExponent;
        para.D = pemPrivateExponent;
        para.P = pemPrime1;
        para.Q = pemPrime2;
        para.DP = pemExponent1;
        para.DQ = pemExponent2;
        para.InverseQ = pemCoefficient;
        return para;
    }
    #endregion
}
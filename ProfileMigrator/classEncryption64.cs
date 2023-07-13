using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Security.Cryptography;

public class Encryption64
{

    private byte[] key = { };
    private byte[] IV = { 10, 20, 30, 40, 50, 60, 70, 80 };

    public string Decrypt(string stringToDecrypt, string sEncryptionKey)
    {
        byte[] inputByteArray = new byte[stringToDecrypt.Length];
        try
        {
            key = Encoding.UTF8.GetBytes(sEncryptionKey.Substring(0, 8));
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            inputByteArray = Convert.FromBase64String(stringToDecrypt);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(key, IV), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            Encoding encoding = Encoding.UTF8;
            return encoding.GetString(ms.ToArray());
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
    public string Encrypt(string stringToEncrypt, string sEncryptionKey)
    {
        try
        {
            key = Encoding.UTF8.GetBytes(sEncryptionKey.Substring(0, 8));
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            Byte[] inputByteArray = Encoding.UTF8.GetBytes(stringToEncrypt);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(key, IV), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray());
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}
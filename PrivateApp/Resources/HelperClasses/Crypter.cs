using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using PrivateApp.Resources.Entity;
using PrivateApp.Resources.Models;
namespace PrivateApp.Resources.HelperClasses
{
    public class Crypter : Converter
    {
        public string Encrypt(string data, byte[] key, byte[] initVector)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key; 
                aes.IV = initVector;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream msEncrypt = new())
                {
                    using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new(csEncrypt))
                        {
                            swEncrypt.Write(data);
                        }
                        return ByteArrayToIntArrayToStr(msEncrypt.ToArray());
                    }
                }
            }
        }
        public byte[] Decrypt(byte[] inputBytes, RSAParameters privateKey)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportParameters(privateKey);
                byte[] decryptedBytes = rsa.Decrypt(inputBytes, RSAEncryptionPadding.OaepSHA1);
                return decryptedBytes;
            }
        }

        public int Decrypt(NewDeviceID newDeviceID, byte[] key, byte[] initVector)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key; 
                aes.IV = initVector;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream msDecrypt = new(StrToIntArrayToByteArray(newDeviceID.DeviceIdStr)))
                {
                    using (CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return Int16.Parse(srDecrypt.ReadToEnd());
                        }
                    }
                }
            }
        }

    }
     
}

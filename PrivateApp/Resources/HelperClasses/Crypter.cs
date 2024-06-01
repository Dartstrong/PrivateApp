using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using PrivateApp.Resources.Entities;
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
        private string DecryptAES(string data, ICryptoTransform decryptor)
        {
            using (MemoryStream msDecrypt = new(StrToIntArrayToByteArray(data)))
            {
                using (CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new(csDecrypt))
                    {
                        string result = srDecrypt.ReadToEnd();
                        srDecrypt.Close();
                        csDecrypt.Close();
                        msDecrypt.Close();
                        return result;
                    }
                }
            }
        }
        public int Decrypt(NewDeviceID newDeviceID, byte[] key, byte[] initVector)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key; 
                aes.IV = initVector;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                return Int16.Parse(DecryptAES(newDeviceID.DeviceIdStr, decryptor));
            }
        }
        public int Decrypt(DialogueRequest dialogueRequest, byte[] key, byte[] initVector)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = initVector;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                return Int16.Parse(DecryptAES(dialogueRequest.IdStr, decryptor));
            }
        }
        public List<DialogueRequest> Decrypt(List<DialogueRequest> dialogueRequests, byte[] key, byte[] initVector)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = initVector;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                List<DialogueRequest> decryptedDialogues = new List<DialogueRequest>();
                for(int i = 0; i < dialogueRequests.Count; i++)
                {
                    decryptedDialogues.Add(new DialogueRequest
                    {
                        IdStr = DecryptAES( dialogueRequests[i].IdStr,decryptor),
                        Sender = DecryptAES(dialogueRequests[i].Sender,decryptor),
                        Receiver = DecryptAES(dialogueRequests[i].Receiver,decryptor)
                    });
                }
                return decryptedDialogues;
            }
        }
    }
     
}

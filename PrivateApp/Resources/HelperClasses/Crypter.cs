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
        public string Encrypt(string inputString, RSAParameters publicKey)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportParameters(publicKey);
                byte[] inputBytes = Encoding.UTF8.GetBytes(inputString);
                byte[] encryptedBytes = rsa.Encrypt(inputBytes, RSAEncryptionPadding.OaepSHA1);
                return ByteArrayToIntArrayToStr(encryptedBytes);
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
        public List<StartedDialogue> Decrypt(List<StartedDialogue> dialogues, byte[] key, byte[] initVector)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = initVector;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                List<StartedDialogue> decryptedDialogues = new List<StartedDialogue>();
                for (int i = 0; i < dialogues.Count; i++)
                {
                    decryptedDialogues.Add(new StartedDialogue
                    {
                        IdStr = DecryptAES(dialogues[i].IdStr, decryptor),
                        Receiver = DecryptAES(dialogues[i].Receiver, decryptor),
                        PublicKeyModulus = DecryptAES(dialogues[i].PublicKeyModulus, decryptor),
                        PublicKeyExponent = DecryptAES(dialogues[i].PublicKeyExponent, decryptor),
                    });
                }
                return decryptedDialogues;
            }
        }
        public List<CustomMessage> Decrypt(List<CustomMessage> messages, byte[] key, byte[] initVector, RSAParameters rsaParameters)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = initVector;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                List<CustomMessage> decryptedDialogues = new List<CustomMessage>();
                for (int i = 0; i < messages.Count; i++)
                {
                    decryptedDialogues.Add(new CustomMessage
                    {
                        My = messages[i].My,
                        Data = Encoding.UTF8.GetString(Decrypt(StrToIntArrayToByteArray(DecryptAES(messages[i].Data, decryptor)), rsaParameters)),
                        ReceivedServer = messages[i].ReceivedServer
                    });
                }
                return decryptedDialogues;
            }
        }
        public List<LoginEntry> Decrypt(List<LoginEntry> loginEntries, byte[] key, byte[] initVector)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = initVector;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                List<LoginEntry> decrypteLoginEntries = new List<LoginEntry>();
                for (int i = 0; i < loginEntries.Count; i++)
                {
                    decrypteLoginEntries.Add(new LoginEntry
                    {
                        DeviceIdStr = DecryptAES(loginEntries[i].DeviceIdStr, decryptor),
                        Date = loginEntries[i].Date
                    });
                }
                return decrypteLoginEntries;
            }
        }
    }  
}

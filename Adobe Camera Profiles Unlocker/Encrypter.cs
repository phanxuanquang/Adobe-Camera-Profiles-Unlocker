using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Adobe_Camera_Profiles_Unlocker
{
    public static class Encrypter
    {
        public static string GetEncrypted(string input)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.GenerateIV();

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(input);
                        }
                    }
                    byte[] iv = aesAlg.IV;
                    byte[] encryptedBytes = msEncrypt.ToArray();

                    byte[] result = new byte[iv.Length + encryptedBytes.Length];
                    Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                    Buffer.BlockCopy(encryptedBytes, 0, result, iv.Length, encryptedBytes.Length);

                    return Convert.ToBase64String(result);
                }
            }
        }

        public static string GetDecrypted(string input)
        {
            byte[] inputBytes = Convert.FromBase64String(input);

            using (Aes aesAlg = Aes.Create())
            {
                byte[] iv = new byte[aesAlg.IV.Length];
                Buffer.BlockCopy(inputBytes, 0, iv, 0, iv.Length);

                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream())
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                    {
                        csDecrypt.Write(inputBytes, iv.Length, inputBytes.Length - iv.Length);
                    }
                    return Encoding.UTF8.GetString(msDecrypt.ToArray());
                }
            }
        }

        public static void Execute(string filePath, bool chooseEncrypt)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var processedLine = chooseEncrypt ? GetEncrypted(line) : GetDecrypted(line);
                        writer.WriteLine(processedLine);
                    }
                }
            }
        }

    }
}

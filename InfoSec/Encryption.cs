using System;
using System.Security.Cryptography;
using System.Text;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.InfoSec
{
    public class Encryption
    {
        public static string GenerateKey() {
            string keyBase64 = "";

            using (Aes aes = Aes.Create()) {
                aes.KeySize = 256;
                aes.GenerateKey();
                keyBase64 = Convert.ToBase64String(aes.Key);    
            }
            return keyBase64;
        }

        public static string Encrpyt(string PlainText, string Key, out string IVKey) {
            using (Aes aes = Aes.Create()) {
                aes.Padding = PaddingMode.Zeros;
                aes.Key=Convert.FromBase64String(Key);
                aes.GenerateIV();
                IVKey=Convert.ToBase64String(aes.IV);

                ICryptoTransform encryptor = aes.CreateEncryptor();

                byte[] encryptedData;

                using (MemoryStream ms = new MemoryStream()) {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)) {
                        using (StreamWriter sw = new StreamWriter(cs)) { 
                            sw.Write(PlainText);
                        }
                        encryptedData = ms.ToArray();
                    }
                }
                return Convert.ToBase64String(encryptedData);
            }

        }

        public static string Decrypt(string CipherText, string Key, string IVKey)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Padding = PaddingMode.Zeros;
                aes.Key = Convert.FromBase64String(Key);
                aes.IV = Convert.FromBase64String(IVKey);

                ICryptoTransform decryptor = aes.CreateDecryptor();

                string PlainText = "";
                byte[] ciper = Convert.FromBase64String(CipherText);

                using (MemoryStream ms = new MemoryStream(ciper))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            PlainText = sr.ReadToEnd();
                        }
                       
                    }
                }
                return PlainText;
            }

        }
    }
}

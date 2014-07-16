using System;
using System.Security.Cryptography;
using System.Text;

namespace ToDoWebsite
{
    public class EncryptionService : IEncryptionService
    {
        public string GetStringSha1Hash(string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            using (var sha1 = new SHA1Managed())
            {
                byte[] textData = Encoding.UTF8.GetBytes(text);

                byte[] hash = sha1.ComputeHash(textData);

                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

        public string EncryptUsername(string txtPassword)
        {
            byte[] passBytes = Encoding.Unicode.GetBytes(txtPassword);
            string encryptPassword = Convert.ToBase64String(passBytes);

            return encryptPassword;
        }

        public string DecryptUsername(string encryptedPassword)
        {
            byte[] passByteData = Convert.FromBase64String(encryptedPassword);
            string originalPassword = Encoding.Unicode.GetString(passByteData);

            return originalPassword;
        }
    }

    public interface IEncryptionService
    {
        string GetStringSha1Hash(string text);
        string EncryptUsername(string txtPassword);
        string DecryptUsername(string encryptedPassword);
    }
}
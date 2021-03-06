﻿using inSyca.foundation.framework.diagnostics;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace inSyca.foundation.framework.security
{
    public static class Security
    {
        static readonly string PasswordHash = "P@@Sw0rd";
        static readonly string SaltKey = "S@LT&KEY";
        static readonly string VIKey = "@1B2c3D4e5F6g7H8";

        internal static string Encrypt(string plainText)
        {
            Log.DebugFormat("Encrypt(string plainText {0})", plainText);

            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));

            byte[] cipherTextBytes;

            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    cipherTextBytes = memoryStream.ToArray();
                    cryptoStream.Close();
                }
                memoryStream.Close();
            }
            return Convert.ToBase64String(cipherTextBytes);
        }

        internal static string Decrypt(string encryptedText)
        {
            Log.DebugFormat("Decrypt(string encryptedText {0})", encryptedText);

            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.None };

            var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));
            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];

            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
        }

        public static string ReplacePasswordCharacters(string strConnectionString)
        {
            Log.DebugFormat("ReplacePasswordCharacters(string strConnectionString {0})", strConnectionString);

            if (string.IsNullOrEmpty(strConnectionString))
                return strConnectionString;

            int iStart;
            int iStartPassword;
            int iEndPassword;

            StringBuilder strBuilder = new StringBuilder(strConnectionString.ToLower());

            iStart = strBuilder.ToString().IndexOf("password=");

            if (iStart < 0)
                return strConnectionString;

            iStartPassword = strBuilder.ToString().IndexOf("=", iStart) + 1;
            iEndPassword = strBuilder.ToString().IndexOf(";", iStart);

            try
            {
                strBuilder.Remove(iStartPassword, iEndPassword - iStartPassword);
                strBuilder.Insert(iStartPassword, "*****");
            }
            catch (Exception ex)
            {
                Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { "strConnectionString" }, ex));

                return strConnectionString;
            }

            return strBuilder.ToString();
        }
    }
}

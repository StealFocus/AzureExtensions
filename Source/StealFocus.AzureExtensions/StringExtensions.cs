// -----------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="Beazley">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace StealFocus.AzureExtensions
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public static class StringExtensions
    {
        public static string Base64Encode(this string text)
        {
            byte[] bytes = new UTF8Encoding().GetBytes(text);
            string base64EncodedText = Convert.ToBase64String(bytes);
            return base64EncodedText;
        }

        public static string Base64Decode(this string base64EncodedText)
        {
            byte[] bytes = Convert.FromBase64String(base64EncodedText);
            string text = new UTF8Encoding().GetString(bytes);
            return text;
        }

        public static string MD5Hash(this string text)
        {
            using (MD5CryptoServiceProvider md5CryptoServiceProvider = new MD5CryptoServiceProvider())
            {
                byte[] hashBytes = md5CryptoServiceProvider.ComputeHash(Encoding.Default.GetBytes(text));
                string hashText = Convert.ToBase64String(hashBytes);
                return hashText;
            }
        }
    }
}

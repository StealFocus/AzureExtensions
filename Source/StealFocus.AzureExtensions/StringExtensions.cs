namespace StealFocus.AzureExtensions
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public static class StringExtensions
    {
        /// <summary>
        /// Converts a UTF-8 string to a Base-64 version of the string.
        /// </summary>
        /// <param name="text">The string to convert to Base-64.</param>
        /// <returns>The Base-64 converted string.</returns>
        public static string Base64Encode(this string text)
        {
            byte[] bytes = new UTF8Encoding().GetBytes(text);
            string base64EncodedText = Convert.ToBase64String(bytes);
            return base64EncodedText;
        }

        /// <summary>
        /// Converts a Base-64 encoded string to UTF-8.
        /// </summary>
        /// <param name="base64EncodedText">The string to convert from Base-64.</param>
        /// <returns>The converted UTF-8 string.</returns>
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

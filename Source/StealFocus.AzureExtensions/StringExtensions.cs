// -----------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="Beazley">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace StealFocus.AzureExtensions
{
    using System;
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
    }
}

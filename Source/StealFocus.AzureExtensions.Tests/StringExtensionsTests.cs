namespace StealFocus.AzureExtensions.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void UnitTestEncodeAndDecode()
        {
            const string OrginalText = "originalText";
            string encodedText = OrginalText.Base64Encode();
            Assert.AreNotEqual(OrginalText, encodedText, "The encoded text was the same as the original plain text.");
            string decodedText = encodedText.Base64Decode();
            Assert.AreEqual(OrginalText, decodedText, "The decoded text was not the same as the original plain text.");
        }
    }
}

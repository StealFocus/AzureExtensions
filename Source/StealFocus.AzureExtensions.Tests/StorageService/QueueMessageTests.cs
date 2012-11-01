namespace StealFocus.AzureExtensions.Tests.StorageService
{
    using System.Xml.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using StealFocus.AzureExtensions.StorageService;

    [TestClass]
    public class QueueMessageTests
    {
        [TestMethod]
        public void UnitTestConstructorWithMessageTextAndMessageTextPropertyAccess()
        {
            QueueMessage queueMessage = new QueueMessage("<MyMessage />");
            Assert.AreEqual("<MyMessage />", queueMessage.MessageText);
        }

        [TestMethod]
        public void UnitTestConstructorWithQueueMessageElementAndMessageTextPropertyAccess()
        {
            // <QueueMessage>
            //   <MessageText>PFN0dWZmIC8+</MessageText>
            // </QueueMessage>
            // "PFN0dWZmIC8+" = "<Stuff />" base 64 encoded.
            XElement queueMessageElement = new XElement(
                "QueueMessage",
                new XElement("MessageText", "PFN0dWZmIC8+"));
            QueueMessage queueMessage = new QueueMessage(queueMessageElement);
            Assert.AreEqual("<Stuff />", queueMessage.MessageText);
        }
    }
}

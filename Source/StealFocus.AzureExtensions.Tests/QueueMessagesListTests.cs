namespace StealFocus.AzureExtensions.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class QueueMessagesListTests
    {
        [TestMethod]
        public void UnitTestConstructorWithQueueMessagesListXmlAndGetMessages()
        {
            // <QueueMessagesList>
            //   <QueueMessage>
            //     <MessageText>PFN0dWZmIC8+</MessageText>
            //   </QueueMessage>
            // </QueueMessagesList>
            // "PFN0dWZmIC8+" = "<Stuff />" base 64 encoded.
            const string QueueMessagesListXml = "<QueueMessagesList><QueueMessage><MessageText>PFN0dWZmIC8+</MessageText></QueueMessage></QueueMessagesList>";
            QueueMessagesList queueMessagesList = new QueueMessagesList(QueueMessagesListXml);
            QueueMessage[] queueMessages = queueMessagesList.GetMessages();
            Assert.AreEqual(1, queueMessages.Length, "The number of queue messages was not as expected.");
            Assert.AreEqual("<Stuff />", queueMessages[0].MessageText, "The message text was not as expected.");
        }
    }
}

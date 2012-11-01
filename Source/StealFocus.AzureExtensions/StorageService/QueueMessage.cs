namespace StealFocus.AzureExtensions.StorageService
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    using StealFocus.AzureExtensions.StorageService.Configuration;

    /// <remarks>
    /// An example XML fragment for a queue message (this is an Azure specific format).
    /// <code>
    ///   <QueueMessage>
    ///     <MessageId>d0d31ff9-4581-44e7-9472-a5d45937f59b</MessageId>
    ///     <InsertionTime>Mon, 29 Oct 2012 19:17:33 GMT</InsertionTime>
    ///     <ExpirationTime>Mon, 05 Nov 2012 19:17:33 GMT</ExpirationTime>
    ///     <DequeueCount>0</DequeueCount>
    ///     <PopReceipt>sF4mYECEzwgBAAAA</PopReceipt> 
    ///     <TimeNextVisible>Mon, 29 Oct 2012 23:00:50 GMT</TimeNextVisible>
    ///     <MessageText>PFN0dWZmIC8+</MessageText>
    ///   </QueueMessage>
    /// </code>
    /// </remarks>
    public class QueueMessage
    {
        private readonly XElement queueMessageElement;

        /// <param name="queueMessageElement">An <see cref="XElement"/>. The 'QueueMessage' element.</param>
        internal QueueMessage(XElement queueMessageElement)
        {
            if (queueMessageElement == null)
            {
                throw new ArgumentNullException("queueMessageElement");
            }

            this.queueMessageElement = queueMessageElement;
        }

        /// <param name="messageText">A <see cref="string"/>. The message text, unencoded.</param>
        internal QueueMessage(string messageText)
        {
            if (string.IsNullOrEmpty(messageText))
            {
                throw new ArgumentException("The Message Text may not be null or empty.", "messageText");
            }

            this.queueMessageElement = new XElement(
                "QueueMessage",
                new XElement("MessageText", EncodeMessageText(messageText)));
        }

        public Guid MessageId
        {
            get
            {
                return this.GetMessageId();
            }
        }

        public DateTime InsertionTime
        {
            get
            {
                return this.GetDateTime("InsertionTime");
            }
        }

        public DateTime ExpirationTime
        {
            get
            {
                return this.GetDateTime("ExpirationTime");
            }
        }

        public int DequeueCount
        {
            get
            {
                return this.GetDequeueCount();
            }
        }

        public string PopReceipt
        {
            get
            {
                return this.GetQueueMessageElementChildElementValue("PopReceipt");
            }
        }

        public DateTime TimeNextVisible
        {
            get
            {
                return this.GetDateTime("TimeNextVisible");
            }
        }

        public string MessageText
        {
            get
            {
                return this.GetBase64("MessageText");
            }
        }

        internal XElement QueueMessageElement
        {
            get { return this.queueMessageElement; }
        }

        public string GetRawXml()
        {
            using (XmlReader xmlReader = this.QueueMessageElement.CreateReader())
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlReader);
                return xmlDocument.OuterXml;
            }
        }

        private static string EncodeMessageText(string messageText)
        {
            byte[] messageBodyBytes = new UTF8Encoding().GetBytes(messageText);
            string messageBodyBase64 = Convert.ToBase64String(messageBodyBytes);
            return messageBodyBase64;
        }

        private Guid GetMessageId()
        {
            string rawMessageId = this.GetQueueMessageElementChildElementValue("MessageId");
            return Guid.Parse(rawMessageId);
        }

        private DateTime GetDateTime(string childElementName)
        {
            string rawDateTime = this.GetQueueMessageElementChildElementValue(childElementName);
            return DateTime.ParseExact(rawDateTime, DateFormat.Rfc1123Pattern, CultureInfo.CurrentCulture);
        }

        private int GetDequeueCount()
        {
            string rawDequeueCount = this.GetQueueMessageElementChildElementValue("DequeueCount");
            return int.Parse(rawDequeueCount, CultureInfo.CurrentCulture);
        }

        private string GetBase64(string childElementName)
        {
            string encodedValue = this.GetQueueMessageElementChildElementValue(childElementName);
            byte[] fromBase64String = Convert.FromBase64String(encodedValue);
            string unencodedValue = new UTF8Encoding().GetString(fromBase64String);
            return unencodedValue;
        }

        private string GetQueueMessageElementChildElementValue(string elementName)
        {
            XElement childElement = this.queueMessageElement.Element(elementName);
            if (childElement == null)
            {
                string exceptionMessage = string.Format(CultureInfo.CurrentCulture, "The 'QueueMessage' element did not contain a '{0}' child element.", elementName);
                throw new AzureExtensionsException(exceptionMessage);
            }

            return childElement.Value;
        }
    }
}
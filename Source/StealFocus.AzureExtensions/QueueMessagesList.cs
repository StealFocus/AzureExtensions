namespace StealFocus.AzureExtensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Linq;

    /// <remarks>
    /// An example XML fragment for a queue message (this is an Azure specific format).
    /// <code>
    ///   <QueueMessagesList>
    ///     <QueueMessage>
    ///       <MessageId>d0d31ff9-4581-44e7-9472-a5d45937f59b</MessageId>
    ///       <InsertionTime>Mon, 29 Oct 2012 19:17:33 GMT</InsertionTime>
    ///       <ExpirationTime>Mon, 05 Nov 2012 19:17:33 GMT</ExpirationTime>
    ///       <DequeueCount>0</DequeueCount>
    ///       <MessageText>PFN0dWZmIC8+</MessageText>
    ///     </QueueMessage>
    ///   </QueueMessagesList>
    /// </code>
    /// </remarks>
    public class QueueMessagesList
    {
        private readonly XElement queueMessagesListElement;

        internal QueueMessagesList(string queueMessagesListXml)
        {
            if (string.IsNullOrEmpty(queueMessagesListXml))
            {
                throw new ArgumentException("The Queue Message List XML may not be null or empty.", "queueMessagesListXml");
            }

            this.queueMessagesListElement = GetXElement(queueMessagesListXml);
        }

        internal XElement QueueMessagesListElement
        {
            get { return this.queueMessagesListElement; }
        }

        public QueueMessage[] GetMessages()
        {
            IEnumerable<XElement> queueMessageElements = this.queueMessagesListElement.Elements("QueueMessage");
            ArrayList list = new ArrayList();
            foreach (XElement queueMessageElement in queueMessageElements)
            {
                QueueMessage queueMessage = new QueueMessage(queueMessageElement);
                list.Add(queueMessage);
            }

            return (QueueMessage[])list.ToArray(typeof(QueueMessage));
        }

        private static XElement GetXElement(string queueMessagesListXml)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(queueMessagesListXml);
            XDocument xdocument = new XDocument();
            using (XmlWriter xmlWriter = xdocument.CreateWriter())
            {
                xmlDocument.WriteTo(xmlWriter);
            }

            return xdocument.Root;
        }
    }
}

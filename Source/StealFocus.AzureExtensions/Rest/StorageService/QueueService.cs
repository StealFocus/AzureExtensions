namespace StealFocus.AzureExtensions.Rest.StorageService
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Xml.Linq;

    public class QueueService : IQueueService
    {
        private readonly IStorageServiceRequest storageServiceRequest;

        /// <summary>
        /// Creates a new instance of <see cref="QueueService" />.
        /// </summary>
        /// <param name="storageAccountName">A <see cref="string"/>. The Storage account name, for the Storage Emulator this is 'devstoreaccount1'.</param>
        /// <param name="storageAccountKey">A <see cref="string"/>. The Storage account key, for the Storage Emulator this is 'Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=='.</param>
        public QueueService(string storageAccountName, string storageAccountKey)
        {
            if (string.IsNullOrEmpty(storageAccountName))
            {
                throw new ArgumentException("The Storage Account Name may not be null or empty.", "storageAccountName");
            }

            if (string.IsNullOrEmpty(storageAccountKey))
            {
                throw new ArgumentException("The Storage Account Key may not be null or empty.", "storageAccountKey");
            }

            this.storageServiceRequest = new StorageServiceRequest(storageAccountName, storageAccountKey, new QueueStorageEndpoint(storageAccountName));
        }

        public Queue[] ListQueues()
        {
            List<Queue> queues = new List<Queue>();
            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("GET", "?comp=list");
                HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();
                    if (responseStream == null)
                    {
                        throw new AzureExtensionsException("The response did not provide a stream as expected.");
                    }

                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        // <?xml version="1.0" encoding="utf-8" ?> 
                        // <EnumerationResults AccountName="http://127.0.0.1:10001/devstoreaccount1/">
                        //   <Queues>
                        //     <Queue>
                        //       <Name>some-queue</Name>
                        //       <Url>http://127.0.0.1:10001/devstoreaccount1/some-queue</Url>
                        //     </Queue>
                        //     <Queue>
                        //       <Name>another-queue</Name> 
                        //       <Url>http://127.0.0.1:10001/devstoreaccount1/another-queue</Url> 
                        //     </Queue>
                        //   </Queues>
                        //   <NextMarker />
                        // </EnumerationResults>
                        string enumerationResultsXml = reader.ReadToEnd();
                        XElement enumerationResultsElement = XElement.Parse(enumerationResultsXml);
                        XElement queuesElement = enumerationResultsElement.Element("Queues");
                        if (queuesElement == null)
                        {
                            throw new AzureExtensionsException("The root element of the XML did not contain any child elements of the name 'Queues'.");
                        }

                        foreach (XElement queueElement in queuesElement.Elements("Queue"))
                        {
                            XElement nameElement = queueElement.Element("Name");
                            if (nameElement == null)
                            {
                                throw new AzureExtensionsException("The 'Queue' element of the XML did not contain any child elements of the name 'Name'.");
                            }

                            XElement urlElement = queueElement.Element("Url");
                            if (urlElement == null)
                            {
                                throw new AzureExtensionsException("The 'Queue' element of the XML did not contain any child elements of the name 'Url'.");
                            }

                            Queue queue = new Queue(nameElement.Value, new Uri(urlElement.Value));
                            queues.Add(queue);
                        }
                    }
                }

                response.Close();
                return queues.ToArray();
            }
            catch (WebException ex)
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response;
                if (ex.Status == WebExceptionStatus.ProtocolError &&
                    ex.Response != null &&
                    httpWebResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }
        }

        public Queue[] ListQueues(int numberOfAttempts)
        {
            return this.ListQueues(numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public Queue[] ListQueues(int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(this.ListQueues, numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public bool CreateQueue(string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentException("The Queue Name may not be null or empty.", "queueName");
            }

            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("PUT", queueName);
                HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
                response.Close();
                return true;
            }
            catch (WebException ex)
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response;
                if (ex.Status == WebExceptionStatus.ProtocolError &&
                    ex.Response != null &&
                    httpWebResponse.StatusCode == HttpStatusCode.Conflict)
                {
                    return false;
                }

                if (ex.Status == WebExceptionStatus.ProtocolError &&
                    ex.Response != null &&
                    httpWebResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    string exceptionMessage = string.Format(CultureInfo.CurrentCulture, "Windows Azure indicated the request was bad (HTTP Status Code {0}). Did your queue name contain invalid characters like capital letters, periods or spaces? The supplied queue name was '{1}'.", (int)httpWebResponse.StatusCode, queueName);
                    throw new AzureExtensionsException(exceptionMessage);
                }

                throw;
            }
        }

        public bool CreateQueue(string queueName, int numberOfAttempts)
        {
            return this.CreateQueue(queueName, numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public bool CreateQueue(string queueName, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(() => this.CreateQueue(queueName), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public bool DeleteQueue(string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentException("The Queue Name may not be null or empty.", "queueName");
            }

            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("DELETE", queueName);
                HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
                response.Close();
                return true;
            }
            catch (WebException ex)
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response;
                if (ex.Status == WebExceptionStatus.ProtocolError &&
                    ex.Response != null &&
                    httpWebResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }

                throw;
            }
        }

        public bool DeleteQueue(string queueName, int numberOfAttempts)
        {
            return this.DeleteQueue(queueName, numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public bool DeleteQueue(string queueName, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(() => this.DeleteQueue(queueName), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public SortedList<string, string> GetQueueMetadata(string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentException("The Queue Name may not be null or empty.", "queueName");
            }

            SortedList<string, string> metadataList = new SortedList<string, string>();
            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("HEAD", queueName + "?comp=metadata", string.Empty, metadataList);
                HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
                response.Close();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    if (response.Headers != null)
                    {
                        for (int i = 0; i < response.Headers.Count; i++)
                        {
                            if (response.Headers.Keys[i].StartsWith("x-ms-meta-", StringComparison.OrdinalIgnoreCase))
                            {
                                metadataList.Add(response.Headers.Keys[i], response.Headers[i]);
                            }
                        }
                    }
                }

                return metadataList;
            }
            catch (WebException ex)
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response;
                if (ex.Status == WebExceptionStatus.ProtocolError &&
                    ex.Response != null &&
                    httpWebResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }
        }

        public SortedList<string, string> GetQueueMetadata(string queueName, int numberOfAttempts)
        {
            return this.GetQueueMetadata(queueName, numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public SortedList<string, string> GetQueueMetadata(string queueName, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(() => this.GetQueueMetadata(queueName), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public bool SetQueueMetadata(string queueName, SortedList<string, string> metadataList)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentException("The Queue Name may not be null or empty.", "queueName");
            }

            if (metadataList == null)
            {
                throw new ArgumentNullException("metadataList");
            }

            try
            {
                SortedList<string, string> headers = new SortedList<string, string>();
                foreach (KeyValuePair<string, string> value in metadataList)
                {
                    headers.Add("x-ms-meta-" + value.Key, value.Value);
                }

                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("PUT", queueName + "?comp=metadata", string.Empty, headers);
                HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
                response.Close();
                return true;
            }
            catch (WebException ex)
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response;
                if (ex.Status == WebExceptionStatus.ProtocolError &&
                    ex.Response != null &&
                    httpWebResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }

                throw;
            }
        }
        
        public bool SetQueueMetadata(string queueName, SortedList<string, string> metadataList, int numberOfAttempts)
        {
            return this.SetQueueMetadata(queueName, metadataList, numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public bool SetQueueMetadata(string queueName, SortedList<string, string> metadataList, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(() => this.SetQueueMetadata(queueName, metadataList), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public bool PutMessage(string queueName, string messageBody)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentException("The Queue Name may not be null or empty.", "queueName");
            }

            if (string.IsNullOrEmpty(messageBody))
            {
                throw new ArgumentException("The Message Body may not be null or empty.", "messageBody");
            }

            try
            {
                string message = new QueueMessage(messageBody).GetRawXml();
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("POST", queueName + "/messages", message);
                HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
                response.Close();
                return true;
            }
            catch (WebException ex)
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response;
                if (ex.Status == WebExceptionStatus.ProtocolError &&
                    ex.Response != null &&
                    httpWebResponse.StatusCode == HttpStatusCode.Conflict)
                {
                    return false;
                }

                throw;
            }
        }

        public bool PutMessage(string queueName, string messageBody, int numberOfAttempts)
        {
            return this.PutMessage(queueName, messageBody, numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public bool PutMessage(string queueName, string messageBody, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(() => this.PutMessage(queueName, messageBody), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public QueueMessage PeekMessage(string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentException("The Queue Name may not be null or empty.", "queueName");
            }

            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("GET", queueName + "/messages?peekonly=true");
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                if (responseStream == null)
                {
                    throw new AzureExtensionsException("The response did not provide a stream as expected.");
                }

                string message;
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    message = reader.ReadToEnd();
                }

                httpWebResponse.Close();
                QueueMessagesList queueMessagesList = new QueueMessagesList(message);
                QueueMessage[] queueMessages = queueMessagesList.GetMessages();
                if (queueMessages.Length == 0)
                {
                    return null;
                }

                return queueMessages[0];
            }
            catch (WebException ex)
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response;
                if (ex.Status == WebExceptionStatus.ProtocolError &&
                    ex.Response != null &&
                    httpWebResponse.StatusCode == HttpStatusCode.Conflict)
                {
                    return null;
                }

                throw;
            }
        }
        
        public QueueMessage PeekMessage(string queueName, int numberOfAttempts)
        {
            return this.PeekMessage(queueName, numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public QueueMessage PeekMessage(string queueName, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(() => this.PeekMessage(queueName), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public QueueMessage GetMessage(string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentException("The Queue Name may not be null or empty.", "queueName");
            }

            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("GET", queueName + "/messages");
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                if (responseStream == null)
                {
                    throw new AzureExtensionsException("The response did not provide a stream as expected.");
                }

                string queueMessagesListXml;
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    queueMessagesListXml = reader.ReadToEnd();
                    httpWebResponse.Close();
                }

                QueueMessagesList queueMessagesList = new QueueMessagesList(queueMessagesListXml);
                QueueMessage[] queueMessages = queueMessagesList.GetMessages();
                if (queueMessages.Length == 0)
                {
                    return null;
                }

                return queueMessages[0];
            }
            catch (WebException ex)
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response;
                if (ex.Status == WebExceptionStatus.ProtocolError &&
                    ex.Response != null &&
                    httpWebResponse.StatusCode == HttpStatusCode.Conflict)
                {
                    return null;
                }

                throw;
            }
        }
        
        public QueueMessage GetMessage(string queueName, int numberOfAttempts)
        {
            return this.GetMessage(queueName, numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public QueueMessage GetMessage(string queueName, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(() => this.GetMessage(queueName), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public bool ClearMessages(string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentException("The Queue Name may not be null or empty.", "queueName");
            }

            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("DELETE", queueName + "/messages");
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                httpWebResponse.Close();
                return true;
            }
            catch (WebException ex)
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response;
                if (ex.Status == WebExceptionStatus.ProtocolError &&
                    ex.Response != null &&
                    httpWebResponse.StatusCode == HttpStatusCode.Conflict)
                {
                    return false;
                }

                throw;
            }
        }

        public bool ClearMessages(string queueName, int numberOfAttempts)
        {
            return this.ClearMessages(queueName, numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public bool ClearMessages(string queueName, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(() => this.ClearMessages(queueName), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public bool DeleteMessage(string queueName, Guid messageId, string popReceipt)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentException("The Queue Name may not be null or empty.", "queueName");
            }

            if (messageId == Guid.Empty)
            {
                throw new ArgumentException("The Message ID may not be an empty GUID.", "messageId");
            }

            if (string.IsNullOrEmpty(popReceipt))
            {
                throw new ArgumentException("The Pop Receipt may not be null or empty.", "popReceipt");
            }

            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("DELETE", queueName + "/messages/" + messageId + "?popreceipt=" + Uri.EscapeDataString(popReceipt));
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                httpWebResponse.Close();
                return true;
            }
            catch (WebException ex)
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response;
                if (ex.Status == WebExceptionStatus.ProtocolError &&
                    ex.Response != null &&
                    httpWebResponse.StatusCode == HttpStatusCode.Conflict)
                {
                    return false;
                }

                if (ex.Status == WebExceptionStatus.ProtocolError &&
                    ex.Response != null &&
                    httpWebResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    string exceptionMessage = string.Format(CultureInfo.CurrentCulture, "Windows Azure indicated the request was bad (HTTP Status Code {0}). Did you provide a valid Pop Receipt? The supplied Pop Receipt was '{1}'.", (int)httpWebResponse.StatusCode, popReceipt);
                    throw new AzureExtensionsException(exceptionMessage, ex);
                }

                throw;
            }
        }

        public bool DeleteMessage(string queueName, Guid messageId, string popReceipt, int numberOfAttempts)
        {
            return this.DeleteMessage(queueName, messageId, popReceipt, numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public bool DeleteMessage(string queueName, Guid messageId, string popReceipt, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(() => this.DeleteMessage(queueName, messageId, popReceipt), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }
    }
}

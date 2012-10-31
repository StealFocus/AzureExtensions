namespace StealFocus.AzureExtensions.Rest
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Xml.Linq;

    public class BlobStorage : IBlobStorage
    {
        private readonly IStorageApiRequest storageApiRequest;

        /// <summary>
        /// Creates a new instance of <see cref="QueueStorage" />.
        /// </summary>
        /// <param name="storageAccountName">A <see cref="string"/>. The Storage account name, for the Storage Emulator this is 'devstoreaccount1'.</param>
        /// <param name="storageAccountKey">A <see cref="string"/>. The Storage account key, for the Storage Emulator this is 'Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=='.</param>
        public BlobStorage(string storageAccountName, string storageAccountKey)
        {
            if (string.IsNullOrEmpty(storageAccountName))
            {
                throw new ArgumentException("The Storage Account Name may not be null or empty.", "storageAccountName");
            }

            if (string.IsNullOrEmpty(storageAccountKey))
            {
                throw new ArgumentException("The Storage Account Key may not be null or empty.", "storageAccountKey");
            }

            this.storageApiRequest = new StorageApiRequest(storageAccountName, storageAccountKey, new BlobStorageEndpoint(storageAccountName));
        }

        public BlobContainer[] ListContainers()
        {
            List<BlobContainer> blobContainers = new List<BlobContainer>();
            try
            {
                HttpWebRequest httpWebRequest = this.storageApiRequest.Create("GET", "?comp=list");
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = httpWebResponse.GetResponseStream();
                    if (responseStream == null)
                    {
                        throw new AzureExtensionsException("The response did not provide a stream as expected.");
                    }

                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        // <?xml version="1.0" encoding="utf-8" ?> 
                        // <EnumerationResults AccountName="http://127.0.0.1:10000/devstoreaccount1/">
                        //   <Containers>
                        //     <Container>
                        //       <Name>container1</Name> 
                        //       <Url>http://127.0.0.1:10000/devstoreaccount1/container1</Url> 
                        //       <Properties>
                        //         <Last-Modified>Mon, 29 Oct 2012 12:52:22 GMT</Last-Modified> 
                        //         <Etag>0x8CF83EB5F8E16F0</Etag> 
                        //       </Properties>
                        //     </Container>
                        //     <Container>
                        //       <Name>container2</Name> 
                        //       <Url>http://127.0.0.1:10000/devstoreaccount1/container2</Url> 
                        //       <Properties>
                        //         <Last-Modified>Mon, 29 Oct 2012 12:52:22 GMT</Last-Modified> 
                        //         <Etag>0x8CF83EB5F319F10</Etag> 
                        //       </Properties>
                        //     </Container>
                        //   </Containers>
                        //   <NextMarker /> 
                        // </EnumerationResults>
                        string enumerationResultsXml = reader.ReadToEnd();
                        XElement enumerationResultsElement = XElement.Parse(enumerationResultsXml);
                        XElement containersElement = enumerationResultsElement.Element("Containers");
                        if (containersElement == null)
                        {
                            throw new AzureExtensionsException("The 'EnumerationResults' element did not have a 'Containers' child element.");
                        }

                        foreach (XElement containerElement in containersElement.Elements("Container"))
                        {
                            XElement nameElement = containerElement.Element("Name");
                            if (nameElement == null)
                            {
                                throw new AzureExtensionsException("The 'Container' element did not have a 'Name' child element.");
                            }

                            XElement urlElement = containerElement.Element("Url");
                            if (urlElement == null)
                            {
                                throw new AzureExtensionsException("The 'Container' element did not have a 'Url' child element.");
                            }

                            XElement propertiesElement = containerElement.Element("Properties");
                            if (propertiesElement == null)
                            {
                                throw new AzureExtensionsException("The 'Container' element did not have a 'Properties' child element.");
                            }

                            XElement lastModifiedElement = propertiesElement.Element("Last-Modified");
                            if (lastModifiedElement == null)
                            {
                                throw new AzureExtensionsException("The 'propertiesElement' element did not have a 'Last-Modified' child element.");
                            }

                            XElement etagElement = propertiesElement.Element("Etag");
                            if (etagElement == null)
                            {
                                throw new AzureExtensionsException("The 'propertiesElement' element did not have an 'Etag' child element.");
                            }

                            BlobContainer blobContainer = new BlobContainer(nameElement.Value, new Uri(urlElement.Value), DateTime.Parse(lastModifiedElement.Value, CultureInfo.CurrentCulture), etagElement.Value);
                            blobContainers.Add(blobContainer);
                        }
                    }
                }

                httpWebResponse.Close();
                return blobContainers.ToArray();
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

        public BlobContainer[] ListContainers(int numberOfAttempts)
        {
            return this.ListContainers(numberOfAttempts, StorageApiRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public BlobContainer[] ListContainers(int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageApiRequest.Attempt(this.ListContainers, numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public bool CreateContainer(string container)
        {
            try
            {
                HttpWebRequest httpWebRequest = this.storageApiRequest.Create("PUT", container + "?restype=container");
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

        public bool CreateContainer(string container, int numberOfAttempts)
        {
            return this.CreateContainer(container, numberOfAttempts, StorageApiRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public bool CreateContainer(string container, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageApiRequest.Attempt(() => this.CreateContainer(container), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public bool DeleteContainer(string container)
        {
            try
            {
                HttpWebRequest httpWebRequest = this.storageApiRequest.Create("DELETE", container + "?restype=container");
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                httpWebResponse.Close();
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

        public bool DeleteContainer(string container, int numberOfAttempts)
        {
            return this.DeleteContainer(container, numberOfAttempts, StorageApiRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public bool DeleteContainer(string container, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageApiRequest.Attempt(() => this.DeleteContainer(container), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public SortedList<string, string> GetContainerProperties(string container)
        {
            try
            {
                HttpWebRequest httpWebRequest = this.storageApiRequest.Create("HEAD", container + "?restype=container");
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                httpWebResponse.Close();
                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    if (httpWebResponse.Headers != null)
                    {
                        SortedList<string, string> properties = new SortedList<string, string>();
                        for (int i = 0; i < httpWebResponse.Headers.Count; i++)
                        {
                            properties.Add(httpWebResponse.Headers.Keys[i], httpWebResponse.Headers[i]);
                        }

                        return properties;
                    }
                }

                return null;
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

        public SortedList<string, string> GetContainerMetadata(string container)
        {
            SortedList<string, string> metadataList = new SortedList<string, string>();
            try
            {
                HttpWebRequest httpWebRequest = this.storageApiRequest.Create("HEAD", container + "?restype=container&comp=metadata", string.Empty, metadataList);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                httpWebResponse.Close();
                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    if (httpWebResponse.Headers != null)
                    {
                        SortedList<string, string> properties = new SortedList<string, string>();
                        for (int i = 0; i < httpWebResponse.Headers.Count; i++)
                        {
                            if (httpWebResponse.Headers.Keys[i].StartsWith("x-ms-meta-", StringComparison.OrdinalIgnoreCase))
                            {
                                properties.Add(httpWebResponse.Headers.Keys[i], httpWebResponse.Headers[i]);
                            }
                        }

                        return properties;
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

        public bool SetContainerMetadata(string container, SortedList<string, string> metadataList)
        {
            try
            {
                SortedList<string, string> headers = new SortedList<string, string>();
                if (metadataList != null)
                {
                    foreach (KeyValuePair<string, string> value in metadataList)
                    {
                        if (value.Key.StartsWith("x-ms-meta-", StringComparison.OrdinalIgnoreCase))
                        {
                            headers.Add(value.Key.ToLowerInvariant(), value.Value);
                        }
                        else
                        {
                            headers.Add("x-ms-meta-" + value.Key.ToLowerInvariant(), value.Value);
                        }
                    }
                }

                HttpWebRequest httpWebRequest = this.storageApiRequest.Create("PUT", container + "?restype=container&comp=metadata", string.Empty, headers);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                httpWebResponse.Close();
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

        public string GetContainerAcl(string container)
        {
            string accessLevel = string.Empty;
            try
            {
                HttpWebRequest httpWebRequest = this.storageApiRequest.Create("GET", container + "?restype=container&comp=acl");
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                httpWebResponse.Close();
                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    if (httpWebResponse.Headers != null)
                    {
                        string access = httpWebResponse.Headers["x-ms-blob-public-access"];
                        if (access != null)
                        {
                            switch (access)
                            {
                                case "container":
                                case "blob":
                                    accessLevel = access;
                                    break;
                                case "true":
                                    accessLevel = "container";
                                    break;
                                default:
                                    accessLevel = "private";
                                    break;
                            }
                        }
                        else
                        {
                            accessLevel = "private";
                        }
                    }
                }

                return accessLevel;
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
    }
}

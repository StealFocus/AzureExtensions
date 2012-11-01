namespace StealFocus.AzureExtensions.Rest.StorageService
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Xml.Linq;

    using StealFocus.AzureExtensions.Endpoints.StorageService;

    public class BlobService : IBlobService
    {
        private readonly IStorageServiceRequest storageServiceRequest;

        /// <summary>
        /// Creates a new instance of <see cref="QueueService" />.
        /// </summary>
        /// <param name="storageAccountName">A <see cref="string"/>. The Storage account name, for the Storage Emulator this is 'devstoreaccount1'.</param>
        /// <param name="storageAccountKey">A <see cref="string"/>. The Storage account key, for the Storage Emulator this is 'Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=='.</param>
        public BlobService(string storageAccountName, string storageAccountKey)
        {
            if (string.IsNullOrEmpty(storageAccountName))
            {
                throw new ArgumentException("The Storage Account Name may not be null or empty.", "storageAccountName");
            }

            if (string.IsNullOrEmpty(storageAccountKey))
            {
                throw new ArgumentException("The Storage Account Key may not be null or empty.", "storageAccountKey");
            }

            this.storageServiceRequest = new StorageServiceRequest(storageAccountName, storageAccountKey, new BlobStorageEndpoint(storageAccountName));
        }

        public BlobContainer[] ListContainers()
        {
            List<BlobContainer> blobContainers = new List<BlobContainer>();
            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("GET", "?comp=list");
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
            return this.ListContainers(numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public BlobContainer[] ListContainers(int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(this.ListContainers, numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public bool CreateContainer(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException("The Container Name may not be null or empty.", "containerName");
            }

            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("PUT", containerName + "?restype=container");
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

        public bool CreateContainer(string containerName, int numberOfAttempts)
        {
            return this.CreateContainer(containerName, numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public bool CreateContainer(string containerName, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(() => this.CreateContainer(containerName), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public bool DeleteContainer(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException("The Container Name may not be null or empty.", "containerName");
            }

            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("DELETE", containerName + "?restype=container");
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

        public bool DeleteContainer(string containerName, int numberOfAttempts)
        {
            return this.DeleteContainer(containerName, numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public bool DeleteContainer(string containerName, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(() => this.DeleteContainer(containerName), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public SortedList<string, string> GetContainerProperties(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException("The Container Name may not be null or empty.", "containerName");
            }

            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("HEAD", containerName + "?restype=container");
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

        public SortedList<string, string> GetContainerMetadata(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException("The Container Name may not be null or empty.", "containerName");
            }

            SortedList<string, string> metadataList = new SortedList<string, string>();
            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("HEAD", containerName + "?restype=container&comp=metadata", string.Empty, metadataList);
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

        public bool SetContainerMetadata(string containerName, SortedList<string, string> metadataList)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException("The Container Name may not be null or empty.", "containerName");
            }

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

                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("PUT", containerName + "?restype=container&comp=metadata", string.Empty, headers);
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

        public ContainerAcl GetContainerAcl(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException("The Container Name may not be null or empty.", "containerName");
            }

            string containerAccessLevel = null;
            string signedIdentifiersXml = null;
            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("GET", containerName + "?restype=container&comp=acl");
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
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
                                    containerAccessLevel = "container";
                                    break;
                                case "blob":
                                    containerAccessLevel = "blob";
                                    break;
                                case "true":
                                    // Case for legacy compatibility.
                                    containerAccessLevel = "container";
                                    break;
                                default:
                                    containerAccessLevel = access;
                                    break;
                            }
                        }
                        else
                        {
                            containerAccessLevel = "private";
                        }
                    }

                    Stream responseStream = httpWebResponse.GetResponseStream();
                    if (responseStream == null)
                    {
                        throw new AzureExtensionsException("The response did not provide a stream as expected.");
                    }

                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        // <?xml version="1.0" encoding="utf-8"?>
                        // <SignedIdentifiers>
                        //   <SignedIdentifier> 
                        //     <Id>MTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTI=</Id>
                        //     <AccessPolicy>
                        //      <Start>2009-09-28T08:49:37.0000000Z</Start>
                        //       <Expiry>2009-09-29T08:49:37.0000000Z</Expiry>
                        //       <Permission>rwd</Permission>
                        //     </AccessPolicy>
                        //   </SignedIdentifier>
                        // </SignedIdentifiers>
                        signedIdentifiersXml = reader.ReadToEnd();
                    }
                }
                
                httpWebResponse.Close();
                return new ContainerAcl(containerAccessLevel, signedIdentifiersXml);
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

        public bool SetContainerAcl(string containerName, ContainerAcl containerAcl)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException("The Container Name may not be null or empty.", "containerName");
            }

            if (containerAcl == null)
            {
                throw new ArgumentNullException("containerAcl");
            }

            try
            {
                SortedList<string, string> headers = new SortedList<string, string>();
                switch (containerAcl.AccessLevel)
                {
                    case "container":
                    case "blob":
                        headers.Add("x-ms-blob-public-access", containerAcl.AccessLevel.ToLowerInvariant());
                        break;
                }

                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("PUT", containerName + "?restype=container&comp=acl", containerAcl.SignedIdentifiersXml, headers);
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
    }
}

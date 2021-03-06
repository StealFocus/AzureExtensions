﻿namespace StealFocus.AzureExtensions.StorageService
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Xml.Linq;

    using StealFocus.AzureExtensions.Configuration;
    using StealFocus.AzureExtensions.StorageService.Endpoints;

    public class BlobService : IBlobService
    {
        private readonly IStorageServiceRequest storageServiceRequest;

        /// <summary>
        /// Creates a new instance of <see cref="QueueService" />.
        /// </summary>
        /// <param name="storageAccountName">A <see cref="string"/>. The Storage account name, for the Storage Emulator this is '<![CDATA[devstoreaccount1]]>'.</param>
        /// <param name="storageAccountKey">A <see cref="string"/>. The Storage account key, for the Storage Emulator this is '<![CDATA[Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==]]>'.</param>
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

            this.storageServiceRequest = new StorageServiceRequest(storageAccountName, storageAccountKey, new BlobServiceEndpoint(storageAccountName));
        }

        public BlobContainer[] ListContainers()
        {
            List<BlobContainer> blobContainers = new List<BlobContainer>();
            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create(RequestMethod.Get, "?comp=list");
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
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create(RequestMethod.Delete, containerName + "?restype=container");
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
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create(RequestMethod.Get, containerName + "?restype=container&comp=acl");
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

        public string[] ListBlobs(string containerName)
        {
            List<string> blobs = new List<string>();
            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create(RequestMethod.Get, containerName + "?restype=container&comp=list&include=snapshots&include=metadata");
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
                        string result = reader.ReadToEnd();
                        XElement enumerationResultsXml = XElement.Parse(result);
                        XElement blobsElement = enumerationResultsXml.Element("Blobs");
                        if (blobsElement == null)
                        {
                            throw new AzureExtensionsException("The 'EnumerationResults' element did not contain a child element 'Blobs'.");
                        }

                        foreach (XElement blob in blobsElement.Elements("Blob"))
                        {
                            XElement nameElement = blob.Element("Name");
                            if (nameElement == null)
                            {
                                throw new AzureExtensionsException("The 'Blob' element did not contain a child element 'Name'.");
                            }

                            blobs.Add(nameElement.Value);
                        }
                    }
                }

                httpWebResponse.Close();
                return blobs.ToArray();
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

        public bool PutBlob(string containerName, string blobName, string blobContent)
        {
            try
            {
                SortedList<string, string> headers = new SortedList<string, string>();
                headers.Add("x-ms-blob-type", "BlockBlob");
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("PUT", containerName + "/" + blobName, blobContent, headers);
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

        public bool PutPageBlob(string containerName, string blobName, int pageSize)
        {
            if ((pageSize % 512) != 0)
            {
                throw new ArgumentException("The page size must be aligned to a 512-byte boundary.", "pageSize");
            }

            try
            {
                SortedList<string, string> headers = new SortedList<string, string>();
                headers.Add("x-ms-blob-type", "PageBlob");
                headers.Add("x-ms-blob-content-length", pageSize.ToString(CultureInfo.CurrentCulture));
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("PUT", containerName + "/" + blobName, null, headers);
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

        public string GetBlob(string containerName, string blobName)
        {
            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create(RequestMethod.Get, containerName + "/" + blobName);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                if (responseStream == null)
                {
                    throw new AzureExtensionsException("The response did not provide a stream as expected.");
                }

                string content;
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    content = reader.ReadToEnd();
                }

                httpWebResponse.Close();
                return content;
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

        public bool PutBlobIfUnchanged(string containerName, string blobName, string content, string expectedETagValue)
        {
            try
            {
                SortedList<string, string> headers = new SortedList<string, string>();
                headers.Add("x-ms-blob-type", "BlockBlob");
                headers.Add("If-Match", expectedETagValue);
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("PUT", containerName + "/" + blobName, content, headers, expectedETagValue);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                httpWebResponse.Close();
                return true;
            }
            catch (WebException ex)
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response;
                if (ex.Status == WebExceptionStatus.ProtocolError &&
                    ex.Response != null &&
                    (httpWebResponse.StatusCode == HttpStatusCode.Conflict || httpWebResponse.StatusCode == HttpStatusCode.PreconditionFailed))
                {
                    return false;
                }

                throw;
            }
        }

        public SortedList<string, string> GetBlobMetadata(string containerName, string blobName)
        {
            SortedList<string, string> metadata = new SortedList<string, string>();
            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("HEAD", containerName + "/" + blobName + "?comp=metadata");
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                httpWebResponse.Close();
                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    if (httpWebResponse.Headers != null)
                    {
                        for (int i = 0; i < httpWebResponse.Headers.Count; i++)
                        {
                            if (httpWebResponse.Headers.Keys[i].StartsWith("x-ms-meta-", StringComparison.OrdinalIgnoreCase))
                            {
                                metadata.Add(httpWebResponse.Headers.Keys[i], httpWebResponse.Headers[i]);
                            }
                        }
                    }
                }

                return metadata;
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

        public SortedList<string, string> GetBlobProperties(string containerName, string blobName)
        {
            SortedList<string, string> propertiesList = new SortedList<string, string>();
            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("HEAD", containerName + "/" + blobName);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                httpWebResponse.Close();
                if ((int)httpWebResponse.StatusCode == 200)
                {
                    if (httpWebResponse.Headers != null)
                    {
                        for (int i = 0; i < httpWebResponse.Headers.Count; i++)
                        {
                            propertiesList.Add(httpWebResponse.Headers.Keys[i], httpWebResponse.Headers[i]);
                        }
                    }
                }

                return propertiesList;
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

        public bool PutBlobAsMD5Hash(string container, string blob, string content)
        {
            try
            {
                string md5Hash = content.MD5Hash();
                SortedList<string, string> headers = new SortedList<string, string>();
                headers.Add("x-ms-blob-type", "BlockBlob");
                headers.Add("Content-MD5", md5Hash);
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("PUT", container + "/" + blob, content, headers, string.Empty, md5Hash);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                httpWebResponse.Close();
                return true;
            }
            catch (WebException ex)
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response;
                if (ex.Status == WebExceptionStatus.ProtocolError &&
                    ex.Response != null &&
                    (httpWebResponse.StatusCode == HttpStatusCode.Conflict ||
                    httpWebResponse.StatusCode == HttpStatusCode.BadRequest))
                {
                    return false;
                }

                throw;
            }
        }
    }
}

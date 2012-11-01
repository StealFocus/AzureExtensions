namespace StealFocus.AzureExtensions.Rest.StorageService
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Xml.Linq;

    using StealFocus.AzureExtensions.Endpoints.StorageService;

    public class TableService : ITableService
    {
        private readonly TableServiceEndpoint endpoint;

        private readonly IStorageServiceRequest storageServiceRequest;

        /// <summary>
        /// Creates a new instance of <see cref="TableService" />.
        /// </summary>
        /// <param name="storageAccountName">A <see cref="string"/>. The Storage account name, for the Storage Emulator this is 'devstoreaccount1'.</param>
        /// <param name="storageAccountKey">A <see cref="string"/>. The Storage account key, for the Storage Emulator this is 'Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=='.</param>
        public TableService(string storageAccountName, string storageAccountKey)
        {
            if (string.IsNullOrEmpty(storageAccountName))
            {
                throw new ArgumentException("The Storage Account Name may not be null or empty.", "storageAccountName");
            }

            if (string.IsNullOrEmpty(storageAccountKey))
            {
                throw new ArgumentException("The Storage Account Key may not be null or empty.", "storageAccountKey");
            }

            this.endpoint = new TableServiceEndpoint(storageAccountName);
            this.storageServiceRequest = new StorageServiceRequest(storageAccountName, storageAccountKey, this.endpoint);
        }

        public Table[] ListTables()
        {
            List<Table> tables = new List<Table>();
            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("GET", "Tables");
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
                        // <?xml version="1.0" encoding="utf-8" standalone="yes" ?>
                        // <feed 
                        //   xml:base="http://127.0.0.1:10002/devstoreaccount1/" 
                        //   xmlns:d="http://schemas.microsoft.com/ado/2007/08/dataservices" 
                        //   xmlns:m="http://schemas.microsoft.com/ado/2007/08/dataservices/metadata" 
                        //   xmlns="http://www.w3.org/2005/Atom">
                        //   <title type="text">Tables</title>
                        //   <id>http://127.0.0.1:10002/devstoreaccount1/Tables</id>
                        //   <updated>2012-10-30T10:27:55Z</updated>
                        //   <link rel="self" title="Tables" href="Tables" />
                        //   <entry>
                        //     <id>http://127.0.0.1:10002/devstoreaccount1/Tables('MyTable')</id>
                        //     <title type="text"></title>
                        //     <updated>2012-10-30T10:27:55Z</updated>
                        //     <author>
                        //       <name />
                        //     </author>
                        //     <link rel="edit" title="Table" href="Tables('MyTable')" />
                        //     <category 
                        //       term="Microsoft.WindowsAzure.DevelopmentStorage.Store.Table" 
                        //       scheme="http://schemas.microsoft.com/ado/2007/08/dataservices/scheme" />
                        //     <content type="application/xml">
                        //       <m:properties>
                        //         <d:TableName>MyTable</d:TableName>
                        //       </m:properties>
                        //     </content>
                        //   </entry>
                        //   <entry>
                        //     ...
                        //   <entry>
                        // </feed>
                        string feedXmlWithDocDeclaration = reader.ReadToEnd();
                        XNamespace atom = "http://www.w3.org/2005/Atom";
                        XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
                        XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
                        XElement feedXml = XElement.Parse(feedXmlWithDocDeclaration, LoadOptions.SetBaseUri);
                        foreach (XElement entryElement in feedXml.Elements(atom + "entry"))
                        {
                            XElement idElement = entryElement.Element(atom + "id");
                            if (idElement == null)
                            {
                                throw new AzureExtensionsException("The 'entry' element of the XML did not contain any child elements of the name 'id'.");
                            }

                            XElement updatedElement = entryElement.Element(atom + "updated");
                            if (updatedElement == null)
                            {
                                throw new AzureExtensionsException("The 'entry' element of the XML did not contain any child elements of the name 'updatedElement'.");
                            }

                            XElement contentElement = entryElement.Element(atom + "content");
                            if (contentElement == null)
                            {
                                throw new AzureExtensionsException("The 'entry' element of the XML did not contain any child elements of the name 'content'.");
                            }

                            XElement propertiesElement = contentElement.Element(m + "properties");
                            if (propertiesElement == null)
                            {
                                throw new AzureExtensionsException("The 'content' element of the XML did not contain any child elements of the name 'properties'.");
                            }

                            XElement tableNameElement = propertiesElement.Element(d + "TableName");
                            if (tableNameElement == null)
                            {
                                throw new AzureExtensionsException("The 'properties' element of the XML did not contain any child elements of the name 'TableName'.");
                            }

                            Table table = new Table(tableNameElement.Value, idElement.Value, new Uri(idElement.Value), DateTime.Parse(updatedElement.Value, CultureInfo.CurrentCulture));
                            tables.Add(table);
                        }
                    }
                }

                response.Close();
                return tables.ToArray();
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

        public Table[] ListTables(int numberOfAttempts)
        {
            return this.ListTables(numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public Table[] ListTables(int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(this.ListTables, numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public bool CreateTable(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("The Table Name may not be null or empty.", "tableName");
            }

            try
            {
                string now = DateTime.UtcNow.ToString("o", CultureInfo.CurrentCulture);
                const string RequestBodyFormat = 
                    "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>" +
                    "<entry" +
                    "  xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\"" +
                    "  xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\"" +
                    "  xmlns=\"http://www.w3.org/2005/Atom\"> " +
                    "  <title /> " +
                    "  <updated>{0}</updated> " +
                    "  <author>" +
                    "    <name/> " +
                    "  </author> " +
                    "  <id/> " +
                    "  <content type=\"application/xml\">" +
                    "    <m:properties>" +
                    "      <d:TableName>{1}</d:TableName>" +
                    "    </m:properties>" +
                    "  </content> " +
                    "</entry>";
                string requestBody = string.Format(CultureInfo.CurrentCulture, RequestBodyFormat, now, tableName);
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("POST", "Tables", requestBody);
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

        public bool CreateTable(string tableName, int numberOfAttempts)
        {
            return this.CreateTable(tableName, numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public bool CreateTable(string tableName, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(() => this.CreateTable(tableName), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public bool DeleteTable(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("The Table Name may not be null or empty.", "tableName");
            }

            try
            {
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("DELETE", "Tables('" + tableName + "')");
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

        public bool DeleteTable(string tableName, int numberOfAttempts)
        {
            return this.DeleteTable(tableName, numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public bool DeleteTable(string tableName, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(() => this.DeleteTable(tableName), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public bool InsertEntity(string tableName, string partitionKey, string rowKey, object entity)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("The Table Name may not be null or empty.", "tableName");
            }

            if (string.IsNullOrEmpty(partitionKey))
            {
                throw new ArgumentException("The Partition Key may not be null or empty.", "partitionKey");
            }

            if (string.IsNullOrEmpty(rowKey))
            {
                throw new ArgumentException("The Row Key may not be null or empty.", "rowKey");
            }

            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            try
            {
                StringBuilder properties = new StringBuilder();
                properties.Append(string.Format(CultureInfo.CurrentCulture, "<d:{0}>{1}</d:{0}>\n", "PartitionKey", partitionKey));
                properties.Append(string.Format(CultureInfo.CurrentCulture, "<d:{0}>{1}</d:{0}>\n", "RowKey", rowKey));
                Type t = entity.GetType();
                PropertyInfo[] pi = t.GetProperties();
                foreach (PropertyInfo p in pi)
                {
                    MethodInfo mi = p.GetGetMethod();
                    properties.Append(string.Format(CultureInfo.CurrentCulture, "<d:{0}>{1}</d:{0}>\n", p.Name, mi.Invoke(entity, null)));
                }

                string now = DateTime.UtcNow.ToString("o", CultureInfo.CurrentCulture);
                const string RequestBodyFormat = 
                    "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>" +
                    "<entry" +
                    "  xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\"" +
                    "  xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\"" +
                    "  xmlns=\"http://www.w3.org/2005/Atom\"> " +
                    "  <title /> " +
                    "  <updated>{0}</updated> " +
                    "  <author>" +
                    "    <name/> " +
                    "  </author> " +
                    "  <id/> " +
                    "  <content type=\"application/xml\">" +
                    "  <m:properties>" +
                    "{1}" +
                    "  </m:properties>" +
                    "  </content> " +
                    "</entry>";
                string requestBody = string.Format(CultureInfo.CurrentCulture, RequestBodyFormat, now, properties);
                HttpWebRequest httpWebRequest = this.storageServiceRequest.Create("POST", tableName, requestBody);
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

        public bool InsertEntity(string tableName, string partitionKey, string rowKey, object entity, int numberOfAttempts)
        {
            return this.InsertEntity(tableName, partitionKey, rowKey, entity, numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public bool InsertEntity(string tableName, string partitionKey, string rowKey, object entity, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(() => this.InsertEntity(tableName, partitionKey, rowKey, entity), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public string GetEntity(string tableName, string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("The Table Name may not be null or empty.", "tableName");
            }

            if (string.IsNullOrEmpty(partitionKey))
            {
                throw new ArgumentException("The Partition Key may not be null or empty.", "partitionKey");
            }

            if (string.IsNullOrEmpty(rowKey))
            {
                throw new ArgumentException("The Row Key may not be null or empty.", "rowKey");
            }

            string entityXml = null;
            try
            {
                string resource = string.Format(CultureInfo.CurrentCulture, tableName + "(PartitionKey='{0}',RowKey='{1}')", partitionKey, rowKey);
                SortedList<string, string> headers = new SortedList<string, string>();
                headers.Add("If-Match", "*");
                HttpWebRequest request = this.storageServiceRequest.Create("GET", resource, null, headers);
                request.Accept = "application/atom+xml";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();
                    if (responseStream == null)
                    {
                        throw new AzureExtensionsException("The response did not provide a stream as expected.");
                    }

                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        string result = reader.ReadToEnd();
                        XElement entry = XElement.Parse(result);
                        entityXml = entry.ToString();
                    }
                }

                response.Close();
                return entityXml;
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

        public string GetEntity(string tableName, string partitionKey, string rowKey, int numberOfAttempts)
        {
            return this.GetEntity(tableName, partitionKey, rowKey, numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public string GetEntity(string tableName, string partitionKey, string rowKey, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(() => this.GetEntity(tableName, partitionKey, rowKey), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public string QueryEntities(string tableName, string filter)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("The Table Name may not be null or empty.", "tableName");
            }

            if (string.IsNullOrEmpty(filter))
            {
                throw new ArgumentException("The Partition Key may not be null or empty.", "filter");
            }

            string entityXml = null;
            try
            {
                string resource = string.Format(CultureInfo.CurrentCulture, tableName + "()?$filter=" + Uri.EscapeDataString(filter));
                HttpWebRequest request = this.storageServiceRequest.Create("GET", resource);
                request.Accept = "application/atom+xml,application/xml";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();
                    if (responseStream == null)
                    {
                        throw new AzureExtensionsException("The response did not provide a stream as expected.");
                    }

                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        string result = reader.ReadToEnd();
                        XElement entry = XElement.Parse(result);
                        entityXml = entry.ToString();
                    }
                }

                response.Close();
                return entityXml;
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

        public string QueryEntities(string tableName, string filter, int numberOfAttempts)
        {
            return this.QueryEntities(tableName, filter, numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public string QueryEntities(string tableName, string filter, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(() => this.QueryEntities(tableName, filter), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public bool ReplaceUpdateEntity(string tableName, string partitionKey, string rowKey, object entity)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("The Table Name may not be null or empty.", "tableName");
            }

            if (string.IsNullOrEmpty(partitionKey))
            {
                throw new ArgumentException("The Partition Key may not be null or empty.", "partitionKey");
            }

            if (string.IsNullOrEmpty(rowKey))
            {
                throw new ArgumentException("The Row Key may not be null or empty.", "rowKey");
            }

            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            try
            {
                string now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
                StringBuilder properties = new StringBuilder();
                properties.Append(string.Format(CultureInfo.CurrentCulture, "<d:{0}>{1}</d:{0}>\n", "PartitionKey", partitionKey));
                properties.Append(string.Format(CultureInfo.CurrentCulture, "<d:{0}>{1}</d:{0}>\n", "RowKey", rowKey));
                properties.Append(string.Format(CultureInfo.CurrentCulture, "<d:{0} m:type=\"Edm.DateTime\">{1}</d:{0}>\n", "Timestamp", now));
                Type t = entity.GetType();
                PropertyInfo[] pi = t.GetProperties();
                foreach (PropertyInfo p in pi)
                {
                    MethodInfo mi = p.GetGetMethod();
                    properties.Append(string.Format(CultureInfo.CurrentCulture, "<d:{0}>{1}</d:{0}>\n", p.Name, mi.Invoke(entity, null)));
                }
                
                const string RequestBodyFormat = 
                    "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>" +
                    "<entry" +
                    "  xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\"" +
                    "  xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\"" +
                    "  xmlns=\"http://www.w3.org/2005/Atom\"> " +
                    "  <title /> " +
                    "  <updated>{0}</updated> " +
                    "  <author>" +
                    "    <name/> " +
                    "  </author> " +
                    "  <id>{1}</id> " +
                    "  <content type=\"application/xml\">" +
                    "  <m:properties>" +
                    "{2}" +
                    "  </m:properties>" +
                    "  </content> " +
                    "</entry>";
                string id = string.Format(CultureInfo.CurrentCulture, "{0}{1}(PartitionKey='{2}',RowKey='{3}')", this.endpoint.Address, tableName, partitionKey, rowKey);
                string requestBody = string.Format(CultureInfo.CurrentCulture, RequestBodyFormat, now, id, properties);
                string resource = string.Format(CultureInfo.CurrentCulture, tableName + "(PartitionKey='{0}',RowKey='{1}')", partitionKey, rowKey);
                SortedList<string, string> headers = new SortedList<string, string>();
                headers.Add("If-Match", "*");
                HttpWebRequest request = this.storageServiceRequest.Create("PUT", resource, requestBody, headers);
                request.Accept = "application/atom+xml";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
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

        public bool ReplaceUpdateEntity(string tableName, string partitionKey, string rowKey, object entity, int numberOfAttempts)
        {
            return this.ReplaceUpdateEntity(tableName, partitionKey, rowKey, entity, numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public bool ReplaceUpdateEntity(string tableName, string partitionKey, string rowKey, object entity, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(() => this.ReplaceUpdateEntity(tableName, partitionKey, rowKey, entity), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public bool MergeUpdateEntity(string tableName, string partitionKey, string rowKey, object entity)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("The Table Name may not be null or empty.", "tableName");
            }

            if (string.IsNullOrEmpty(partitionKey))
            {
                throw new ArgumentException("The Partition Key may not be null or empty.", "partitionKey");
            }

            if (string.IsNullOrEmpty(rowKey))
            {
                throw new ArgumentException("The Row Key may not be null or empty.", "rowKey");
            }

            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            try
            {
                string now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
                StringBuilder properties = new StringBuilder();
                properties.Append(string.Format(CultureInfo.CurrentCulture, "<d:{0}>{1}</d:{0}>\n", "PartitionKey", partitionKey));
                properties.Append(string.Format(CultureInfo.CurrentCulture, "<d:{0}>{1}</d:{0}>\n", "RowKey", rowKey));
                properties.Append(string.Format(CultureInfo.CurrentCulture, "<d:{0} m:type=\"Edm.DateTime\">{1}</d:{0}>\n", "Timestamp", now));
                Type t = entity.GetType();
                PropertyInfo[] pi = t.GetProperties();
                foreach (PropertyInfo p in pi)
                {
                    MethodInfo mi = p.GetGetMethod();
                    properties.Append(string.Format(CultureInfo.CurrentCulture, "<d:{0}>{1}</d:{0}>\n", p.Name, mi.Invoke(entity, null)));
                }

                const string RequestBodyFormat =
                    "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>" +
                    "<entry" +
                    "  xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\"" +
                    "  xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\"" +
                    "  xmlns=\"http://www.w3.org/2005/Atom\"> " +
                    "  <title /> " +
                    "  <updated>{0}</updated> " +
                    "  <author>" +
                    "    <name/> " +
                    "  </author> " +
                    "  <id>{1}</id> " +
                    "  <content type=\"application/xml\">" +
                    "  <m:properties>" +
                    "{2}" +
                    "  </m:properties>" +
                    "  </content> " +
                    "</entry>";
                string id = string.Format(CultureInfo.CurrentCulture, "{0}{1}(PartitionKey='{2}',RowKey='{3}')", this.endpoint.Address, tableName, partitionKey, rowKey);
                string requestBody = string.Format(CultureInfo.CurrentCulture, RequestBodyFormat, now, id, properties);
                string resource = string.Format(CultureInfo.CurrentCulture, tableName + "(PartitionKey='{0}',RowKey='{1}')", partitionKey, rowKey);
                SortedList<string, string> headers = new SortedList<string, string>();
                headers.Add("If-Match", "*");
                HttpWebRequest request = this.storageServiceRequest.Create("MERGE", resource, requestBody, headers);
                request.Accept = "application/atom+xml";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
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

        public bool MergeUpdateEntity(string tableName, string partitionKey, string rowKey, object entity, int numberOfAttempts)
        {
            return this.MergeUpdateEntity(tableName, partitionKey, rowKey, entity, numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public bool MergeUpdateEntity(string tableName, string partitionKey, string rowKey, object entity, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(() => this.MergeUpdateEntity(tableName, partitionKey, rowKey, entity), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }

        public bool DeleteEntity(string tableName, string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("The Table Name may not be null or empty.", "tableName");
            }

            if (string.IsNullOrEmpty(partitionKey))
            {
                throw new ArgumentException("The Partition Key may not be null or empty.", "partitionKey");
            }

            if (string.IsNullOrEmpty(rowKey))
            {
                throw new ArgumentException("The Row Key may not be null or empty.", "rowKey");
            }

            try
            {
                string resource = string.Format(CultureInfo.CurrentCulture, tableName + "(PartitionKey='{0}',RowKey='{1}')", partitionKey, rowKey);
                SortedList<string, string> headers = new SortedList<string, string>();
                headers.Add("If-Match", "*");
                HttpWebRequest request = this.storageServiceRequest.Create("DELETE", resource, null, headers);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
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

        public bool DeleteEntity(string tableName, string partitionKey, string rowKey, int numberOfAttempts)
        {
            return this.DeleteEntity(tableName, partitionKey, rowKey, numberOfAttempts, StorageServiceRequest.DefaultAttemptIntervalInMilliseconds);
        }

        public bool DeleteEntity(string tableName, string partitionKey, string rowKey, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds)
        {
            return StorageServiceRequest.Attempt(() => this.DeleteEntity(tableName, partitionKey, rowKey), numberOfAttempts, timeBetweenAttemptsInMilliseconds);
        }
    }
}

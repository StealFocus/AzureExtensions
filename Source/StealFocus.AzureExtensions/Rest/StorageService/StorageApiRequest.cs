namespace StealFocus.AzureExtensions.Rest.StorageService
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Web;

    using StealFocus.AzureExtensions.Rest.Configuration;

    internal class StorageApiRequest : IStorageApiRequest
    {
        internal const int DefaultAttemptIntervalInMilliseconds = 200;

        private const int DefaultAttemptCount = 3;

        private readonly string storageAccountName;

        private readonly string storageAccountKey;

        private readonly IStorageEndpoint storageEndpoint;

        internal StorageApiRequest(string storageAccountName, string storageAccountKey, IStorageEndpoint storageEndpoint)
        {
            this.storageAccountName = storageAccountName;
            this.storageAccountKey = storageAccountKey;
            this.storageEndpoint = storageEndpoint;
        }

        public HttpWebRequest Create(
            string method,
            string resource,
            string requestBody = null,
            SortedList<string, string> headers = null,
            string ifMatch = "",
            string md5 = "")
        {
            byte[] byteArray = new byte[0];
            DateTime now = DateTime.UtcNow;

            // Do we need to guard against trailing "/" here?
            string uri = this.storageEndpoint.Address + resource;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            request.ContentLength = 0;
            request.Headers.Add("x-ms-date", now.ToString(DateFormat.Rfc1123Pattern, CultureInfo.InvariantCulture));
            request.Headers.Add("x-ms-version", "2009-09-19");
            if (this.storageEndpoint.IsTableStorage)
            {
                request.ContentType = "application/atom+xml";
                request.Headers.Add("DataServiceVersion", "1.0;NetFx");
                request.Headers.Add("MaxDataServiceVersion", "1.0;NetFx");
            }

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (!string.IsNullOrEmpty(requestBody))
            {
                request.Headers.Add("Accept-Charset", "UTF-8");
                byteArray = Encoding.UTF8.GetBytes(requestBody);
                request.ContentLength = byteArray.Length;
            }

            request.Headers.Add("Authorization", CreateAuthorizationHeader(this.storageAccountName, this.storageAccountKey, method, now, request, this.storageEndpoint.IsTableStorage, ifMatch, md5));
            if (!string.IsNullOrEmpty(requestBody))
            {
                request.GetRequestStream().Write(byteArray, 0, byteArray.Length);
            }

            return request;
        }

        internal static T Attempt<T>(StorageApiOperationAttempt<T> attemptOperation, int numberOfAttempts = DefaultAttemptCount, int attemptIntervalInMilliseconds = DefaultAttemptIntervalInMilliseconds)
        {
            if (numberOfAttempts < 1)
            {
                throw new ArgumentException("The number of retries must be more than zero.", "numberOfAttempts");
            }

            for (int i = 1; i <= numberOfAttempts; i++)
            {
                try
                {
                    return attemptOperation.Invoke();
                }
                catch (Exception)
                {
                    if (i == numberOfAttempts)
                    {
                        throw;
                    }
                }

                Thread.Sleep(attemptIntervalInMilliseconds);
            }

            // Should never happen - logic should above should return or throw.
            throw new AzureExtensionsException("Logical error.");
        }

        private static string CreateAuthorizationHeader(
            string storageAccountName, 
            string storageKey, 
            string method, 
            DateTime now, 
            HttpWebRequest request, 
            bool tableStorage, 
            string ifMatch = "", 
            string md5 = "")
        {
            string messageSignature;
            if (tableStorage)
            {
                messageSignature = string.Format(
                    CultureInfo.CurrentCulture,
                    "{0}\n\n{1}\n{2}\n{3}",
                    method,
                    "application/atom+xml",
                    now.ToString(DateFormat.Rfc1123Pattern, CultureInfo.InvariantCulture),
                    GetCanonicalizedResource(request.RequestUri, storageAccountName, true));
            }
            else
            {
                messageSignature = string.Format(
                    CultureInfo.CurrentCulture,
                    "{0}\n\n\n{1}\n{5}\n\n\n\n{2}\n\n\n\n{3}{4}",
                    method,
                    (method == "GET" || method == "HEAD") ? string.Empty : request.ContentLength.ToString(CultureInfo.CurrentCulture),
                    ifMatch,
                    GetCanonicalizedHeaders(request),
                    GetCanonicalizedResource(request.RequestUri, storageAccountName, false),
                    md5);
            }

            byte[] signatureBytes = Encoding.UTF8.GetBytes(messageSignature);
            string authorizationHeader;
            using (HMACSHA256 hmacsha256 = new HMACSHA256(Convert.FromBase64String(storageKey)))
            {
                authorizationHeader = "SharedKey " + storageAccountName + ":" + Convert.ToBase64String(hmacsha256.ComputeHash(signatureBytes));
            }

            return authorizationHeader;
        }

        private static string GetCanonicalizedHeaders(HttpWebRequest request)
        {
            ArrayList headerNameList = new ArrayList();
            StringBuilder sb = new StringBuilder();
            foreach (string headerName in request.Headers.Keys)
            {
                if (headerName.ToLowerInvariant().StartsWith("x-ms-", StringComparison.Ordinal))
                {
                    headerNameList.Add(headerName.ToLowerInvariant());
                }
            }

            headerNameList.Sort();
            foreach (string headerName in headerNameList)
            {
                StringBuilder builder = new StringBuilder(headerName);
                string separator = ":";
                foreach (string headerValue in GetHeaderValues(request.Headers, headerName))
                {
                    string trimmedValue = headerValue.Replace("\r\n", string.Empty);
                    builder.Append(separator);
                    builder.Append(trimmedValue);
                    separator = ",";
                }

                sb.Append(builder);
                sb.Append("\n");
            }

            return sb.ToString();
        }

        private static ArrayList GetHeaderValues(NameValueCollection headers, string headerName)
        {
            ArrayList list = new ArrayList();
            string[] values = headers.GetValues(headerName);
            if (values != null)
            {
                foreach (string str in values)
                {
                    list.Add(str.TrimStart(null));
                }
            }

            return list;
        }

        private static string GetCanonicalizedResource(Uri address, string accountName, bool tableStorage)
        {
            StringBuilder str = new StringBuilder();
            StringBuilder builder = new StringBuilder("/");
            builder.Append(accountName);
            builder.Append(address.AbsolutePath);
            str.Append(builder);
            NameValueCollection values2 = new NameValueCollection();
            if (!tableStorage)
            {
                NameValueCollection values = HttpUtility.ParseQueryString(address.Query);
                foreach (string str2 in values.Keys)
                {
                    ArrayList list = new ArrayList(values.GetValues(str2));
                    list.Sort();
                    StringBuilder builder2 = new StringBuilder();
                    foreach (object obj2 in list)
                    {
                        if (builder2.Length > 0)
                        {
                            builder2.Append(",");
                        }

                        builder2.Append(obj2);
                    }

                    values2.Add((str2 == null) ? str2 : str2.ToLowerInvariant(), builder2.ToString());
                }
            }

            ArrayList list2 = new ArrayList(values2.AllKeys);
            list2.Sort();
            foreach (string str3 in list2)
            {
                StringBuilder builder3 = new StringBuilder(string.Empty);
                builder3.Append(str3);
                builder3.Append(":");
                builder3.Append(values2[str3]);
                str.Append("\n");
                str.Append(builder3);
            }

            return str.ToString();
        }
    }
}

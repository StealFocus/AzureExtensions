﻿namespace StealFocus.AzureExtensions.HostedService
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Xml.Linq;
    using Net;
    using Security.Cryptography;

    using StealFocus.AzureExtensions.Configuration;
    using StealFocus.AzureExtensions.HostedService.Configuration;

    public class Operation : IOperation
    {
        /// <param name="subscriptionId">The Subscription ID.</param>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <param name="requestId">The request ID, returned in the headers of the response to the original request.</param>
        public OperationResult StatusCheck(Guid subscriptionId, string certificateThumbprint, string requestId)
        {
            HttpWebRequest httpWebRequest = GetRequestForStatusCheck(subscriptionId, certificateThumbprint, requestId);
            HttpWebResponse httpWebResponse = null;
            OperationResult operationResult;
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponseThrottled();
                operationResult = ExtractOperationResultFromResponse(httpWebResponse);
            }
            catch (WebException e)
            {
                string exceptionMessage = string.Format(CultureInfo.CurrentCulture, "There was an error querying the operation status of Request ID '{0}'.", requestId);
                throw new AzureExtensionsException(exceptionMessage, e);
            }
            finally
            {
                httpWebRequest.Abort();
                if (httpWebResponse != null)
                {
                    httpWebResponse.Close();
                }
            }

            return operationResult;
        }

        private static HttpWebRequest GetRequestForStatusCheck(Guid subscriptionId, string certificateThumbprint, string requestId)
        {
            string statusCheckUrl = GetStatusCheckUrl(subscriptionId.AzureRestFormat(), requestId);
            Uri requestUri = new Uri(statusCheckUrl);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUri);
            httpWebRequest.Headers.Add(RequestHeaderName.MSVersion, RequestMSVersion.December2011);
            httpWebRequest.Method = RequestMethod.Get;
            httpWebRequest.ContentType = RequestContentType.ApplicationXml;
            X509Certificate2 certificate = CertificateStore.GetCertificateFromCurrentUserStore(certificateThumbprint);
            httpWebRequest.ClientCertificates.Add(certificate);
            return httpWebRequest;
        }

        private static string GetStatusCheckUrl(string subscriptionId, string requestId)
        {
            // https://management.core.windows.net/<subscriptionId>/operations/<requestId>
            return string.Format(CultureInfo.CurrentCulture, "https://management.core.windows.net/{0}/operations/{1}", subscriptionId, requestId);
        }

        private static OperationResult ExtractOperationResultFromResponse(HttpWebResponse httpWebResponse)
        {
            XDocument responseBody = httpWebResponse.GetResponseBody();
            /*
             *  <Operation xmlns="http://schemas.microsoft.com/windowsazure" xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
             *      <ID>[GUID]</ID>
             *      <Status>InProgress|Succeeded|Failed</Status>
             *      <HttpStatusCode>200|etc</HttpStatusCode>
             *      <Code>code</Code>
             *      <Message>text</Message>
             *  </Operation>
             */
            OperationResult operationResult = new OperationResult();
            if (responseBody.Root == null)
            {
                throw new AzureExtensionsException("The response did not contain a root element.");
            }

            string idValue = GetChild(responseBody.Root, "ID").Value;
            operationResult.Id = Guid.Parse(idValue);
            string statusValue = GetChild(responseBody.Root, "Status").Value;
            operationResult.Status = (OperationStatus)Enum.Parse(typeof(OperationStatus), statusValue);
            if (operationResult.Status != OperationStatus.InProgress)
            {
                operationResult.HttpStatusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), GetChild(responseBody.Root, "HttpStatusCode").Value);
            }

            if (operationResult.Status == OperationStatus.Failed)
            {
                XElement error = GetChild(responseBody.Root, "Error");
                operationResult.Code = GetChild(error, "Code").Value;
                operationResult.Message = GetChild(error, "Message").Value;
            }
            
            return operationResult;
        }

        private static XElement GetChild(XElement parentElement, string childElementName)
        {
            XNamespace windowsAzureNamespace = XmlNamespace.MicrosoftWindowsAzure;
            XElement element = parentElement.Element(windowsAzureNamespace + childElementName);
            if (element == null)
            {
                string exceptionMessage = string.Format(CultureInfo.CurrentCulture, "The response did not contain a '{0}' element under the root as expected.", childElementName);
                throw new AzureExtensionsException(exceptionMessage);
            }

            return element;
        }
    }
}

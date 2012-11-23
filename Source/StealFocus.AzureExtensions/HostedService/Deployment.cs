﻿namespace StealFocus.AzureExtensions.HostedService
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Xml.Linq;
    using Net;
    using Security.Cryptography;

    using StealFocus.AzureExtensions.Configuration;
    using StealFocus.AzureExtensions.HostedService.Configuration;

    public class Deployment : IDeployment
    {
        /// <param name="subscriptionId">The Subscription ID.</param>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="deploymentSlot">Either "Production" or "Staging".</param>
        public bool CheckExists(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot)
        {
            HttpWebRequest httpWebRequest = GetRequestForGet(subscriptionId, certificateThumbprint, serviceName, deploymentSlot);
            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebRequest.GetResponseThrottled();
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    httpWebResponse = (HttpWebResponse)e.Response;
                    if (httpWebResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        return false;
                    }
                }

                string exceptionMessage = string.Format(
                    CultureInfo.CurrentCulture,
                    "There was a problem getting the deployment for Subscription ID '{0}', Service Name '{1}' and Deployment '{2}'.",
                    subscriptionId,
                    serviceName,
                    deploymentSlot);
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

            return true;
        }

        /// <param name="subscriptionId">The Subscription ID.</param>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="deploymentSlot">Either "Production" or "Staging".</param>
        public string DeleteRequest(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot)
        {
            HttpWebRequest httpWebRequest = GetRequestForDelete(subscriptionId, certificateThumbprint, serviceName, deploymentSlot);
            HttpWebResponse httpWebResponse = null;
            string requestId;
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponseThrottled();
                if (httpWebResponse.StatusCode != HttpStatusCode.Accepted)
                {
                    string exceptionMessage = string.Format(CultureInfo.CurrentCulture, "The HTTP Status Code returned in the response was '{0}', expected was '{1}'.", httpWebResponse.StatusCode, HttpStatusCode.Accepted);
                    throw new AzureExtensionsException(exceptionMessage);
                }

                requestId = httpWebResponse.Headers[ResponseHeaderName.MSRequestId];
            }
            catch (WebException e)
            {
                string exceptionMessage = null;
                if (e.Response != null)
                {
                    httpWebResponse = (HttpWebResponse)e.Response;
                    if (httpWebResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        exceptionMessage = string.Format(CultureInfo.CurrentCulture, "There was en error deleting deployment for service '{0}' in deployment slot '{1}', the service and deployment slot combination was not found.", serviceName, deploymentSlot);
                    }
                }

                if (string.IsNullOrEmpty(exceptionMessage))
                {
                    exceptionMessage = string.Format(CultureInfo.CurrentCulture, "There was en error deleting deployment for service '{0}' in deployment slot '{1}'.", serviceName, deploymentSlot);
                }

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

            return requestId;
        }

        /// <param name="subscriptionId">The Subscription ID.</param>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="deploymentSlot">Either "Production" or "Staging".</param>
        /// <param name="deploymentName">Should not contain spaces.</param>
        /// <param name="packageUrl">The URL to the <![CDATA[.cspkg]]> in blob storage.</param>
        /// <param name="label">Limited to 100 characters.</param>
        /// <param name="configurationFilePath">The path to the <![CDATA[.cscfg]]> file.</param>
        /// <param name="startDeployment">Whether to start after deployment.</param>
        /// <param name="treatWarningsAsError">Whether to treat warnings as errors.</param>
        public string CreateRequest(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot, string deploymentName, Uri packageUrl, string label, string configurationFilePath, bool startDeployment, bool treatWarningsAsError)
        {
            HttpWebRequest httpWebRequest = GetRequestForCreate(subscriptionId, certificateThumbprint, serviceName, deploymentSlot, deploymentName, packageUrl, label, configurationFilePath, startDeployment, treatWarningsAsError);
            HttpWebResponse httpWebResponse = null;
            string requestId;
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponseThrottled();
                if (httpWebResponse.StatusCode != HttpStatusCode.Accepted)
                {
                    string exceptionMessage = string.Format(CultureInfo.CurrentCulture, "The HTTP Status Code returned in the response was '{0}', expected was '{1}'.", httpWebResponse.StatusCode, HttpStatusCode.Accepted);
                    throw new AzureExtensionsException(exceptionMessage);
                }

                requestId = httpWebResponse.Headers[ResponseHeaderName.MSRequestId];
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    httpWebResponse = (HttpWebResponse)e.Response;
                    AzureExtensionsOperationException azureExtensionsOperationException = new AzureExtensionsOperationException();
                    azureExtensionsOperationException.ResponseBody = httpWebResponse.GetResponseBody();
                    throw azureExtensionsOperationException;
                }
                
                string exceptionMessage = string.Format(CultureInfo.CurrentCulture, "There was en error creating deployment for service '{0}' in deployment slot '{1}'.", serviceName, deploymentSlot);
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

            return requestId;
        }

        private static HttpWebRequest GetRequestForDelete(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot)
        {
            string deleteDeploymentUrl = GetDeleteDeploymentUrl(subscriptionId.AzureRestFormat(), serviceName, deploymentSlot);
            Uri requestUri = new Uri(deleteDeploymentUrl);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUri);
            httpWebRequest.Headers.Add(RequestHeaderName.MSVersion, RequestMSVersion.December2011);
            httpWebRequest.Method = RequestMethod.Delete;
            httpWebRequest.ContentType = RequestContentType.ApplicationXml;
            X509Certificate2 certificate = CertificateStore.GetCertificateFromCurrentUserStore(certificateThumbprint);
            httpWebRequest.ClientCertificates.Add(certificate);
            return httpWebRequest;
        }

        private static HttpWebRequest GetRequestForCreate(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot, string deploymentName, Uri packageUrl, string label, string configurationFilePath, bool startDeployment, bool treatWarningsAsError)
        {
            string deleteDeploymentUrl = GetCreateDeploymentUrl(subscriptionId.AzureRestFormat(), serviceName, deploymentSlot);
            Uri requestUri = new Uri(deleteDeploymentUrl);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUri);
            httpWebRequest.Headers.Add(RequestHeaderName.MSVersion, RequestMSVersion.December2011);
            httpWebRequest.Method = RequestMethod.Post;
            httpWebRequest.ContentType = RequestContentType.ApplicationXml;
            X509Certificate2 certificate = CertificateStore.GetCertificateFromCurrentUserStore(certificateThumbprint);
            httpWebRequest.ClientCertificates.Add(certificate);
            XDocument requestBody = GetCreateDeploymentRequestBody(deploymentName, packageUrl, label, configurationFilePath, startDeployment, treatWarningsAsError);
            Stream requestStream = null;
            try
            {
                requestStream = httpWebRequest.GetRequestStream();
                using (StreamWriter streamWriter = new StreamWriter(requestStream, Encoding.UTF8))
                {
                    requestStream = null;
                    requestBody.Save(streamWriter, SaveOptions.DisableFormatting);
                }
            }
            finally
            {
                if (requestStream != null)
                {
                    requestStream.Dispose();
                }
            }

            return httpWebRequest;
        }

        private static HttpWebRequest GetRequestForGet(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot)
        {
            string getDeploymentUrl = GetGetDeploymentUrl(subscriptionId.AzureRestFormat(), serviceName, deploymentSlot);
            Uri requestUri = new Uri(getDeploymentUrl);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUri);
            httpWebRequest.Headers.Add(RequestHeaderName.MSVersion, RequestMSVersion.December2011);
            httpWebRequest.Method = RequestMethod.Get;
            httpWebRequest.ContentType = RequestContentType.ApplicationXml;
            X509Certificate2 certificate = CertificateStore.GetCertificateFromCurrentUserStore(certificateThumbprint);
            httpWebRequest.ClientCertificates.Add(certificate);
            return httpWebRequest;
        }

        private static string GetDeleteDeploymentUrl(string subscriptionId, string serviceName, string deploymentSlot)
        {
            return GetDeploymentUrl(subscriptionId, serviceName, deploymentSlot);
        }

        private static string GetCreateDeploymentUrl(string subscriptionId, string serviceName, string deploymentSlot)
        {
            return GetDeploymentUrl(subscriptionId, serviceName, deploymentSlot);
        }

        private static string GetGetDeploymentUrl(string subscriptionId, string serviceName, string deploymentSlot)
        {
            return GetDeploymentUrl(subscriptionId, serviceName, deploymentSlot);
        }

        private static string GetDeploymentUrl(string subscriptionId, string serviceName, string deploymentSlot)
        {
            // https://management.core.windows.net/<subscriptionId>/services/hostedservices/<serviceName>/deploymentslots/<deploymentSlot>
            return string.Format(CultureInfo.CurrentCulture, "https://management.core.windows.net/{0}/services/hostedservices/{1}/deploymentslots/{2}", subscriptionId, serviceName, deploymentSlot);
        }

        private static XDocument GetCreateDeploymentRequestBody(string name, Uri packageUrl, string label, string configurationFilePath, bool startDeployment, bool treatWarningsAsError)
        {
            /*
             *  <?xml version="1.0" encoding="utf-8"?>
             *  <CreateDeployment xmlns="http://schemas.microsoft.com/windowsazure">
             *      <Name>deployment-name</Name>
             *      <PackageUrl>package-url-in-blob-storage</PackageUrl>
             *      <Label>base64-encoded-deployment-label</Label>
             *      <Configuration>base64-encoded-configuration-file</Configuration>
             *      <StartDeployment>true|false</StartDeployment>
             *      <TreatWarningsAsError>true|false</TreatWarningsAsError>
             *  </CreateDeployment>
            */
            XNamespace windowsAzureNamespace = XmlNamespace.MicrosoftWindowsAzure;
            XDocument requestBody = new XDocument(
                new XDeclaration("1.0", "UTF-8", "no"),
                new XElement(
                    windowsAzureNamespace + "CreateDeployment",
                    new XElement(windowsAzureNamespace + "Name", name),
                    new XElement(windowsAzureNamespace + "PackageUrl", packageUrl.AbsoluteUri),
                    new XElement(windowsAzureNamespace + "Label", label.Base64Encode()),
                    new XElement(windowsAzureNamespace + "Configuration", GetConfigurationFileAsSingleString(configurationFilePath).Base64Encode()),
                    new XElement(windowsAzureNamespace + "StartDeployment", startDeployment.AzureRestFormat()),
                    new XElement(windowsAzureNamespace + "TreatWarningsAsError", treatWarningsAsError.AzureRestFormat())));
            return requestBody;
        }

        private static string GetConfigurationFileAsSingleString(string configurationFilePath)
        {
            return string.Join(string.Empty, File.ReadAllLines(configurationFilePath));
        }
    }
}

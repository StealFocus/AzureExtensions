namespace StealFocus.AzureExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Xml.Linq;

    using StealFocus.AzureExtensions.Configuration;
    using StealFocus.AzureExtensions.HostedService.Configuration;
    using StealFocus.AzureExtensions.Net;
    using StealFocus.AzureExtensions.Security.Cryptography;

    public class Subscription : ISubscription
    {
        private readonly Guid subscriptionId;

        private readonly string certificateThumbprint;

        /// <param name="subscriptionId">The Subscription ID.</param>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        public Subscription(Guid subscriptionId, string certificateThumbprint)
        {
            this.subscriptionId = subscriptionId;
            this.certificateThumbprint = certificateThumbprint;
        }

        public string[] ListHostedServices()
        {
            HttpWebRequest httpWebRequest = GetRequestForListHostedServices(this.subscriptionId, this.certificateThumbprint);
            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponseThrottled();
                Stream responseStream = httpWebResponse.GetResponseStream();
                if (responseStream == null)
                {
                    throw new AzureExtensionsException("The response did not provide a stream as expected.");
                }

                string responseBody;
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    responseBody = reader.ReadToEnd();
                }

                XDocument hostedServicesXml = XDocument.Parse(responseBody);
                string[] hostedServices = GetHostedServiceNamesFromHostedServicesXml(hostedServicesXml);
                return hostedServices;
            }
            catch (WebException e)
            {
                string exceptionMessage = string.Format(CultureInfo.CurrentCulture, "There was a problem getting the list of Hosted Services for Subscription ID '{0}'.", this.subscriptionId);
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
        }

        private static HttpWebRequest GetRequestForListHostedServices(Guid subscriptionId, string certificateThumbprint)
        {
            string listHostedServicesUrl = GetListHostedServicesUrl(subscriptionId.AzureRestFormat());
            Uri requestUri = new Uri(listHostedServicesUrl);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUri);
            httpWebRequest.Headers.Add(RequestHeaderName.MSVersion, RequestMSVersion.December2011);
            httpWebRequest.Method = RequestMethod.Get;
            httpWebRequest.ContentType = RequestContentType.ApplicationXml;
            X509Certificate2 certificate = CertificateStore.GetCertificateFromCurrentUserStore(certificateThumbprint);
            httpWebRequest.ClientCertificates.Add(certificate);
            return httpWebRequest;
        }

        private static string GetListHostedServicesUrl(string subscriptionId)
        {
            // https://management.core.windows.net/<subscriptionId>/services/hostedservices
            return string.Format(CultureInfo.CurrentCulture, "https://management.core.windows.net/{0}/services/hostedservices", subscriptionId);
        }

        private static string[] GetHostedServiceNamesFromHostedServicesXml(XDocument hostedServicesXml)
        {
            /*
            <?xml version="1.0" encoding="utf-8"?>
            <HostedServices 
                xmlns="http://schemas.microsoft.com/windowsazure" 
                xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
                <HostedService>
                    <Url>url</Url> 
                    <ServiceName>serviceName</ServiceName> 
                    <HostedServiceProperties>
                        <Description i:nil="true" /> 
                        <AffinityGroup>affinityGroup</AffinityGroup> 
                        <Label>label</Label> 
                    </HostedServiceProperties>
                </HostedService>
                ...
            </HostedServices>
             */
            XElement hostedServicesXmlRootElement = hostedServicesXml.Root;
            if (hostedServicesXmlRootElement == null)
            {
                string exceptionMessage = string.Format(CultureInfo.CurrentCulture, "The Hosted Services XML returned from the management API did not contain a root element.");
                throw new AzureExtensionsException(exceptionMessage);
            }

            XNamespace ns = XmlNamespace.MicrosoftWindowsAzure;
            IEnumerable<string> serviceNames = (from hostedServiceElement 
                                               in hostedServicesXmlRootElement.Elements(ns + "HostedService")
                                               select hostedServiceElement.Element(ns + "ServiceName").Value).AsEnumerable();
            return serviceNames.ToArray();
        }
    }
}

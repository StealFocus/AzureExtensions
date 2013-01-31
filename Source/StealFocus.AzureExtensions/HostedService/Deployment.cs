namespace StealFocus.AzureExtensions.HostedService
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
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

        /// <param name="subscriptionId">The Subscription ID.</param>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="deploymentSlot">Either "Production" or "Staging".</param>
        /// <returns>
        /// The following XML:
        /// <![CDATA[
        /// <?xml version="1.0" encoding="utf-8"?>
        /// <Deployment 
        ///   xmlns="http://schemas.microsoft.com/windowsazure" 
        ///   xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
        ///   <Name>[GUID]</Name>
        ///   <DeploymentSlot>Production</DeploymentSlot>
        ///   <PrivateID>21d9affd19e540398323bc1106f5d708</PrivateID>
        ///   <Status>Running</Status>
        ///   <Label>[Base64EncodedString]</Label>
        ///   <Url>http://[serviceName].cloudapp.net/</Url>
        ///   <Configuration>[Base64EncodedString]</Configuration>
        ///   <RoleInstanceList>
        ///     <RoleInstance>
        ///       <RoleName>[roleName]</RoleName>
        ///       <InstanceName>[roleName]_IN_0</InstanceName>
        ///       <InstanceStatus>ReadyRole</InstanceStatus>
        ///       <InstanceUpgradeDomain>0</InstanceUpgradeDomain>
        ///       <InstanceFaultDomain>0</InstanceFaultDomain>
        ///       <InstanceSize>ExtraSmall</InstanceSize>
        ///       <InstanceStateDetails />
        ///     </RoleInstance>
        ///     <RoleInstance>
        ///       ...
        ///     </RoleInstance>
        ///   </RoleInstanceList>
        ///   <UpgradeDomainCount>1</UpgradeDomainCount>
        ///   <RoleList>
        ///     <Role>
        ///       <RoleName>[roleName]</RoleName>
        ///       <OsVersion>WA-GUEST-OS-1.21_201210-01</OsVersion>
        ///     </Role>
        ///     <Role>
        ///       ...
        ///     </Role>
        ///   </RoleList>
        ///   <SdkVersion>1.7.30602.1703</SdkVersion>
        ///   <InputEndpointList>
        ///     <InputEndpoint>
        ///       <RoleName>[roleName]</RoleName>
        ///       <Vip>[ipAddress]</Vip>
        ///       <Port>8080</Port>
        ///     </InputEndpoint>
        ///     <InputEndpoint>
        ///       ...
        ///     </InputEndpoint>
        ///   </InputEndpointList>
        ///   <Locked>false</Locked>
        ///   <RollbackAllowed>false</RollbackAllowed>
        /// </Deployment>
        /// ]]>
        /// </returns>
        public XDocument GetInformation(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot)
        {
            return Get(subscriptionId, certificateThumbprint, serviceName, deploymentSlot);
        }

        /// <param name="subscriptionId">The Subscription ID.</param>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="deploymentSlot">Either "Production" or "Staging".</param>
        public XDocument GetConfiguration(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot)
        {
            XDocument deploymentXml = Get(subscriptionId, certificateThumbprint, serviceName, deploymentSlot);
            XNamespace mwans = XmlNamespace.MicrosoftWindowsAzure;
            XElement deploymentElement = deploymentXml.Root;
            if (deploymentElement == null)
            {
                string exceptionMessage = string.Format(CultureInfo.CurrentCulture, "The deployment XML returned from the management API did not contain a root element (expected to be named 'Deployment').");
                throw new AzureExtensionsException(exceptionMessage);
            }

            XElement configurationElement = deploymentElement.Element(mwans + "Configuration");
            if (configurationElement == null)
            {
                string exceptionMessage = string.Format(CultureInfo.CurrentCulture, "The Deployment element did not have a child element named 'Configuration' as expected.");
                throw new AzureExtensionsException(exceptionMessage);
            }

            string decodedConfigurationXml = configurationElement.Value.Base64Decode();
            return XDocument.Parse(decodedConfigurationXml);
        }

        /// <param name="subscriptionId">The Subscription ID.</param>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="deploymentSlot">Either "Production" or "Staging".</param>
        /// <param name="configuration">The XML representing the new configuration i.e. the contents of a <![CDATA[.cscfg]]> file.</param>
        /// <param name="treatWarningsAsError">A <see cref="bool"/>.</param>
        /// <param name="mode">Either "Auto" or "Manual".</param>
        public string ChangeConfiguration(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot, XDocument configuration, bool treatWarningsAsError, string mode)
        {
            HttpWebRequest httpWebRequest = GetRequestForChangeConfiguration(subscriptionId, certificateThumbprint, serviceName, deploymentSlot, configuration, treatWarningsAsError, mode);
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

        /// <param name="subscriptionId">The Subscription ID.</param>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="deploymentSlot">Either "Production" or "Staging".</param>
        /// <param name="horizontalScales">Role names and required instance counts.</param>
        /// <param name="treatWarningsAsError">Whether to treat any warnings as errors.</param>
        /// <param name="mode">Auto|Manual</param>
        /// <returns>The ID of the request to update the configuration of the deployment. Null if no update was made (if the deployment was already scaled to the specification).</returns>
        public string HorizontallyScale(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot, HorizontalScale[] horizontalScales, bool treatWarningsAsError, string mode)
        {
            if (horizontalScales == null)
            {
                throw new ArgumentNullException("horizontalScales");
            }

            XDocument configuration = this.GetConfiguration(subscriptionId, certificateThumbprint, serviceName, deploymentSlot);
            bool configurationHasBeenUpdated = false;
            foreach (HorizontalScale horizontalScale in horizontalScales)
            {
                if (ConfigurationShouldBeUpdated(configuration, horizontalScale.RoleName, horizontalScale.InstanceCount))
                {
                    configuration = UpdateConfiguration(configuration, horizontalScale.RoleName, horizontalScale.InstanceCount);
                    configurationHasBeenUpdated = true;
                }
            }

            if (configurationHasBeenUpdated)
            {
                string changeConfigurationRequestId = this.ChangeConfiguration(subscriptionId, certificateThumbprint, serviceName, deploymentSlot, configuration, treatWarningsAsError, mode);
                return changeConfigurationRequestId;
            }

            return null;
        }

        /// <param name="subscriptionId">The Subscription ID.</param>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="deploymentSlot">Either "Production" or "Staging".</param>
        /// <param name="roleName">The name of the role.</param>
        public string GetInstanceSize(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot, string roleName)
        {
            XDocument deploymentXml = this.GetInformation(subscriptionId, certificateThumbprint, serviceName, deploymentSlot);
            IDeploymentXmlParser deploymentXmlParser = new DeploymentXmlParser(deploymentXml);
            return deploymentXmlParser.GetInstanceSize(roleName);
        }

        private static XDocument Get(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot)
        {
            HttpWebRequest httpWebRequest = GetRequestForGet(subscriptionId, certificateThumbprint, serviceName, deploymentSlot);
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

                return XDocument.Parse(responseBody);
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    httpWebResponse = (HttpWebResponse)e.Response;
                    if (httpWebResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        string notFoundExceptionMessage = string.Format(
                        CultureInfo.CurrentCulture,
                        "The deployment for Subscription ID '{0}', Service Name '{1}' and Deployment '{2}' did not exist.",
                        subscriptionId,
                        serviceName,
                        deploymentSlot);
                        throw new AzureExtensionsException(notFoundExceptionMessage, e);
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

        private static HttpWebRequest GetRequestForChangeConfiguration(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot, XDocument configuration, bool treatWarningsAsError, string mode)
        {
            string deleteDeploymentUrl = GetChangeConfigurationUrl(subscriptionId.AzureRestFormat(), serviceName, deploymentSlot);
            Uri requestUri = new Uri(deleteDeploymentUrl);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUri);
            httpWebRequest.Headers.Add(RequestHeaderName.MSVersion, RequestMSVersion.December2011);
            httpWebRequest.Method = RequestMethod.Post;
            httpWebRequest.ContentType = RequestContentType.ApplicationXml;
            X509Certificate2 certificate = CertificateStore.GetCertificateFromCurrentUserStore(certificateThumbprint);
            httpWebRequest.ClientCertificates.Add(certificate);
            XDocument requestBody = GetChangeConfigurationRequestBody(configuration, treatWarningsAsError, mode);
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

        private static string GetChangeConfigurationUrl(string subscriptionId, string serviceName, string deploymentSlot)
        {
            // https://management.core.windows.net/<subscriptionId>/services/hostedservices/<serviceName>/deploymentslots/<deploymentSlot>/?comp=config
            return string.Format(CultureInfo.CurrentCulture, "https://management.core.windows.net/{0}/services/hostedservices/{1}/deploymentslots/{2}/?comp=config", subscriptionId, serviceName, deploymentSlot);
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

        private static XDocument GetChangeConfigurationRequestBody(XDocument configuration, bool treatWarningsAsError, string mode)
        {
            /*
             *  <?xml version="1.0" encoding="utf-8"?>
             *  <ChangeConfiguration xmlns="http://schemas.microsoft.com/windowsazure">
             *     <Configuration>base-64-encoded-configuration-file</Configuration>
             *     <TreatWarningsAsError>true|false</TreatWarningsAsError>
             *     <Mode>Auto|Manual</Mode>
             *     <ExtendedProperties>
             *        <ExtendedProperty>
             *           <Name>property-name</Name>
             *           <Value>property-value</Value>
             *        </ExtendedProperty>
             *     </ExtendedProperties>
             *  </ChangeConfiguration>
             */
            XNamespace windowsAzureNamespace = XmlNamespace.MicrosoftWindowsAzure;
            XDocument requestBody = new XDocument(
                new XDeclaration("1.0", "UTF-8", "no"),
                new XElement(
                    windowsAzureNamespace + "ChangeConfiguration",
                    new XElement(windowsAzureNamespace + "Configuration", configuration.ToString(SaveOptions.DisableFormatting).Base64Encode()),
                    new XElement(windowsAzureNamespace + "TreatWarningsAsError", treatWarningsAsError.AzureRestFormat()),
                    new XElement(windowsAzureNamespace + "Mode", mode)));
            return requestBody;
        }

        private static string GetConfigurationFileAsSingleString(string configurationFilePath)
        {
            return string.Join(string.Empty, File.ReadAllLines(configurationFilePath));
        }

        private static XElement GetInstancesElement(XDocument configuration, string roleName)
        {
            /*
            <ServiceConfiguration
             serviceName=""
             osFamily="1"
             osVersion="*"
             xmlns="http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration">
              <Role name="[roleName]">
                <ConfigurationSettings>
                  <Setting ... />
                </ConfigurationSettings>
                <Instances count="1" />
                <Certificates />
              </Role>
            </ServiceConfiguration>
             */
            XNamespace ns = XmlNamespace.ServiceHosting200810ServiceConfiguration;
            XElement configurationRootElement = configuration.Root;
            if (configurationRootElement == null)
            {
                string exceptionMessage = string.Format(CultureInfo.CurrentCulture, "The configuration XML returned from the management API did not contain a root element.");
                throw new AzureExtensionsException(exceptionMessage);
            }

            XElement instancesElement = (from role 
                                         in configurationRootElement.Elements(ns + "Role")
                                         where role.Attribute("name").Value == roleName
                                         select role)
                                         .Single()
                                         .Element(ns + "Instances");
            if (instancesElement == null)
            {
                string exceptionMessage = string.Format(CultureInfo.CurrentCulture, "The configuration XML returned from the management API did not contain a 'Role' element matching name '{0}'.", roleName);
                throw new AzureExtensionsException(exceptionMessage);
            }

            return instancesElement;
        }

        private static bool ConfigurationShouldBeUpdated(XDocument configuration, string roleName, int requiredInstanceCount)
        {
            XElement instancesElement = GetInstancesElement(configuration, roleName);
            return requiredInstanceCount.ToString(CultureInfo.CurrentCulture) != instancesElement.Attribute("count").Value;
        }

        private static XDocument UpdateConfiguration(XDocument configuration, string roleName, int newInstanceCount)
        {
            XElement instancesElement = GetInstancesElement(configuration, roleName);
            instancesElement.Attribute("count").SetValue(newInstanceCount);
            return configuration;
        }
    }
}

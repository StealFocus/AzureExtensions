namespace StealFocus.AzureExtensions.HostedService
{
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;

    using StealFocus.AzureExtensions.Configuration;

    internal class DeploymentXmlParser : IDeploymentXmlParser
    {
        private readonly XDocument deploymentXml;

        public DeploymentXmlParser(XDocument deploymentXml)
        {
            this.deploymentXml = deploymentXml;
        }

        public string GetInstanceSize(string roleName)
        {
            XElement roleInstanceListElement = this.GetRoleInstanceListElement();
            XNamespace wans = XmlNamespace.MicrosoftWindowsAzure;
            string instanceSize = (from roleInstanceElement
                                        in roleInstanceListElement.Elements(wans + "RoleInstance")
                                   where roleInstanceElement.Elements(wans + "RoleName").FirstOrDefault().Value == roleName
                                   select roleInstanceElement)
                                   .Single()
                                   .Element(wans + "InstanceSize").Value;
            return instanceSize;
        }

        public int GetInstanceCount(string roleName)
        {
            XElement roleInstanceListElement = this.GetRoleInstanceListElement();
            XNamespace wans = XmlNamespace.MicrosoftWindowsAzure;
            int instanceCount = (from roleInstanceElement
                                     in roleInstanceListElement.Elements(wans + "RoleInstance")
                                 where roleInstanceElement.Elements(wans + "RoleName").FirstOrDefault().Value == roleName
                                 select roleInstanceElement)
                                 .Count();
            return instanceCount;
        }

        private XElement GetRoleInstanceListElement()
        {
            XNamespace wans = XmlNamespace.MicrosoftWindowsAzure;
            XElement deploymentElement = this.deploymentXml.Root;
            if (deploymentElement == null)
            {
                string exceptionMessage = string.Format(CultureInfo.CurrentCulture, "The deployment XML returned from the management API did not contain a root element (expected to be named 'Deployment').");
                throw new AzureExtensionsException(exceptionMessage);
            }

            XElement roleInstanceListElement = deploymentElement.Element(wans + "RoleInstanceList");
            if (roleInstanceListElement == null)
            {
                string exceptionMessage = string.Format(CultureInfo.CurrentCulture, "The Deployment element did not have a child element named 'RoleInstanceList' as expected.");
                throw new AzureExtensionsException(exceptionMessage);
            }

            return roleInstanceListElement;
        }
    }
}

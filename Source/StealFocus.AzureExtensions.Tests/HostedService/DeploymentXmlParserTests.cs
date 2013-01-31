namespace StealFocus.AzureExtensions.Tests.HostedService
{
    using System.Xml.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using StealFocus.AzureExtensions.HostedService;

    [TestClass]
    public class DeploymentXmlParserTests
    {
        private const string DeploymentXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<Deployment xmlns=\"http://schemas.microsoft.com/windowsazure\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            "<Name>[GUID]</Name>" +
            "<DeploymentSlot>Production</DeploymentSlot>" +
            "<PrivateID>21d9affd19e540398323bc1106f5d708</PrivateID>" +
            "<Status>Running</Status>" +
            "<Label>[Base64EncodedString]</Label>" +
            "<Url>http://[serviceName].cloudapp.net/</Url>" +
            "<Configuration>[Base64EncodedString]</Configuration>" +
            "<RoleInstanceList>" +
            "<RoleInstance>" +
            "<RoleName>[roleName]</RoleName>" +
            "<InstanceName>[roleName]_IN_0</InstanceName>" +
            "<InstanceStatus>ReadyRole</InstanceStatus>" +
            "<InstanceUpgradeDomain>0</InstanceUpgradeDomain>" +
            "<InstanceFaultDomain>0</InstanceFaultDomain>" +
            "<InstanceSize>ExtraSmall</InstanceSize>" +
            "<InstanceStateDetails />" +
            "</RoleInstance>" +
            "</RoleInstanceList>" +
            "<UpgradeDomainCount>1</UpgradeDomainCount>" +
            "<RoleList>" +
            "<Role>" +
            "<RoleName>[roleName]</RoleName>" +
            "<OsVersion>WA-GUEST-OS-1.21_201210-01</OsVersion>" +
            "</Role>" +
            "</RoleList>" +
            "<SdkVersion>1.7.30602.1703</SdkVersion>" +
            "<InputEndpointList>" +
            "<InputEndpoint>" +
            "<RoleName>[roleName]</RoleName>" +
            "<Vip>[ipAddress]</Vip>" +
            "<Port>8080</Port>" +
            "</InputEndpoint>" +
            "</InputEndpointList>" +
            "<Locked>false</Locked>" +
            "<RollbackAllowed>false</RollbackAllowed>" +
            "</Deployment>";

        [TestMethod]
        public void UnitTest()
        {
            XDocument deploymentXml = XDocument.Parse(DeploymentXml);
            IDeploymentXmlParser deploymentXmlParser = new DeploymentXmlParser(deploymentXml);
            string instanceSize = deploymentXmlParser.GetInstanceSize("[roleName]");
            Assert.AreEqual("ExtraSmall", instanceSize);
        }
    }
}

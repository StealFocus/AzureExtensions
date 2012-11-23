namespace StealFocus.AzureExtensions.Tests.HostedService
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using StealFocus.AzureExtensions.HostedService;
    using StealFocus.AzureExtensions.HostedService.Configuration;
    using StealFocus.AzureExtensions.StorageService;
    using StealFocus.AzureExtensions.Tests.Configuration;

    [TestClass]
    [Ignore]
    public class DeploymentTests
    {
        [TestMethod]
        public void TestCheckExists()
        {
            IDeployment deployment = new Deployment();
            bool result = deployment.CheckExists(
                WindowsAzureAccount.SubscriptionId,
                WindowsAzureAccount.CertificateThumbprint,
                "BeazleyTasks-WEuro-Sys",
                DeploymentSlot.Production);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestDeleteRequest()
        {
            IDeployment deployment = new Deployment();
            deployment.DeleteRequest(
                WindowsAzureAccount.SubscriptionId, 
                WindowsAzureAccount.CertificateThumbprint, 
                "BeazleyTasks-WEuro-Sys", 
                DeploymentSlot.Production);
        }

        [TestMethod]
        public void TestCreateRequest()
        {
            Uri packageUrl = Blob.GetUrl("bzytasksweurosys", "mydeployments", "20111207_015202_Beazley.Tasks.Azure.cspkg");
            IDeployment deployment = new Deployment();
            deployment.CreateRequest(
                WindowsAzureAccount.SubscriptionId, 
                WindowsAzureAccount.CertificateThumbprint, 
                "BeazleyTasks-WEuro-Sys", 
                DeploymentSlot.Production, 
                "Bad Deployment Name", 
                packageUrl,
                "DeploymentLabel",
                "ServiceConfiguration.Cloud-SysTest.cscfg", 
                true, 
                true);
        }
    }
}

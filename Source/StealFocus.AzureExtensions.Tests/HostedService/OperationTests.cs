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
    public class OperationTests
    {
        [TestMethod]
        public void TestCheckStatus1()
        {
            IDeployment deployment = new Deployment();
            string deleteRequestId = deployment.DeleteRequest(
                WindowsAzureAccount.SubscriptionId, 
                WindowsAzureAccount.CertificateThumbprint, 
                "BeazleyTasks-WEuro-Sys", 
                DeploymentSlot.Production);
            OperationResult operationResult = new Operation().StatusCheck(
                WindowsAzureAccount.SubscriptionId, 
                WindowsAzureAccount.CertificateThumbprint, 
                deleteRequestId);
            Assert.IsNotNull(operationResult);
        }

        [TestMethod]
        public void TestCheckStatus2()
        {
            Uri packageUrl = Blob.GetUrl("bzytasksweurosys", "mydeployments", "20111207_015202_Beazley.Tasks.Azure.cspkg");
            IDeployment deployment = new Deployment();
            string createRequestId = deployment.CreateRequest(
                WindowsAzureAccount.SubscriptionId,
                WindowsAzureAccount.CertificateThumbprint,
                "BeazleyTasks-WEuro-Sys",
                DeploymentSlot.Production,
                "DeploymentName",
                packageUrl,
                "DeploymentLabel",
                "ServiceConfiguration.Cloud-SysTest.cscfg",
                true,
                true);
            OperationResult operationResult1 = new Operation().StatusCheck(
                WindowsAzureAccount.SubscriptionId, 
                WindowsAzureAccount.CertificateThumbprint,
                createRequestId);
            Assert.IsNotNull(operationResult1);
            System.Threading.Thread.Sleep(5000);
            OperationResult operationResult2 = new Operation().StatusCheck(
                WindowsAzureAccount.SubscriptionId, 
                WindowsAzureAccount.CertificateThumbprint,
                createRequestId);
            Assert.IsNotNull(operationResult2);
        }
    }
}

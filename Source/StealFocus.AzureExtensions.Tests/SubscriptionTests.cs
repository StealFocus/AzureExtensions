namespace StealFocus.AzureExtensions.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using StealFocus.AzureExtensions.Tests.Configuration;

    [TestClass]
    [Ignore]
    public class SubscriptionTests
    {
        [TestMethod]
        public void IntegrationTestListHostedServicesWithDeployments()
        {
            ISubscription subscription = new Subscription(WindowsAzureAccount.SubscriptionId, WindowsAzureAccount.CertificateThumbprint);
            string[] hostedServicesWithDeployments = subscription.ListHostedServices();
            Assert.IsNotNull(hostedServicesWithDeployments);
        }
    }
}

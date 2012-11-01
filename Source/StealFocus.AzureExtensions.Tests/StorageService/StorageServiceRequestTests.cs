namespace StealFocus.AzureExtensions.Tests.StorageService
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using StealFocus.AzureExtensions.StorageService;

    [TestClass]
    public class StorageServiceRequestTests
    {
        private int attemptCount;

        private int errorUntilAttemptCount;

        [TestMethod]
        public void UnitTestAttemptWithSuccess()
        {
            this.errorUntilAttemptCount = 0;
            bool result = StorageServiceRequest.Attempt(this.DoSomething, 3, 0);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void UnitTestAttemptWithRetriesAndSuccess()
        {
            this.errorUntilAttemptCount = 1;
            bool result = StorageServiceRequest.Attempt(this.DoSomething, 3, 0);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [ExpectedException(typeof(AzureExtensionsException))]
        public void UnitTestAttemptWithFailure()
        {
            this.errorUntilAttemptCount = 0;
            try
            {
                StorageServiceRequest.Attempt(this.DoSomething, 1, 0);
            }
            catch (AzureExtensionsException e)
            {
                Assert.AreEqual("An error happened.", e.Message, "An exception was thrown but not the one expected.");
                throw;
            }
        }

        private bool DoSomething()
        {
            if (this.attemptCount <= this.errorUntilAttemptCount)
            {
                this.attemptCount++;
                throw new AzureExtensionsException("An error happened.");
            }

            return true;
        }
    }
}

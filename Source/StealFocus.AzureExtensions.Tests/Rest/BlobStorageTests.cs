namespace StealFocus.AzureExtensions.Tests.Rest
{
    using System;
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using StealFocus.AzureExtensions.Rest;
    using StealFocus.AzureExtensions.Tests.Configuration;

    [TestClass]
    public class BlobStorageTests
    {
        /// <remarks>
        /// Called once per test class.
        /// </remarks>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            TestAssembly.RunEmulatorIfRequired(StorageAccount.Name);
            TestAssembly.ThrottleTestClassesIfRunningUnderAzure(StorageAccount.Name);
            CleanupContainers();
        }

        /// <remarks>
        /// Called once per test class.
        /// </remarks>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            CleanupContainers();
        }

        /// <remarks>
        /// Called once per test method.
        /// </remarks>
        [TestInitialize]
        public void TestInitialize()
        {
            TestAssembly.ThrottleTestMethodsIfRunningUnderAzure(StorageAccount.Name);
        }

        /// <remarks>
        /// Called once per test method.
        /// </remarks>
        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void IntegrationTestListContainers()
        {
            IBlobStorage blobStorage = new BlobStorage(StorageAccount.Name, StorageAccount.Key);
            BlobContainer[] blobContainers = blobStorage.ListContainers();
            Assert.IsTrue(blobContainers.Length > 0, "The blob container list was empty when it was not expected to be so.");
            foreach (BlobContainer blobContainer in blobContainers)
            {
                Console.WriteLine("Found container name - " + blobContainer.Name);
            }
        }

        [TestMethod]
        public void IntegrationTestListContainersWithRetries()
        {
            IBlobStorage blobStorage = new BlobStorage(StorageAccount.Name, StorageAccount.Key);
            BlobContainer[] blobContainers = blobStorage.ListContainers(2);
            Assert.IsTrue(blobContainers.Length > 0, "The blob container list was empty when it was not expected to be so.");
            foreach (BlobContainer blobContainer in blobContainers)
            {
                Console.WriteLine("Found container name - " + blobContainer.Name);
            }
        }

        [TestMethod]
        public void IntegrationTestCreateContainerAndDeleteContainer()
        {
            IBlobStorage blobStorage = new BlobStorage(StorageAccount.Name, StorageAccount.Key);
            bool createSucess = blobStorage.CreateContainer("test1");
            Assert.IsTrue(createSucess, "The container was not created as expected.");
            bool deleteSuccess = blobStorage.DeleteContainer("test1");
            Assert.IsTrue(deleteSuccess, "The container was not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestGetContainerProperties()
        {
            IBlobStorage blobStorage = new BlobStorage(StorageAccount.Name, StorageAccount.Key);
            bool createSucess = blobStorage.CreateContainer("test2");
            Assert.IsTrue(createSucess, "The container was not created as expected.");
            SortedList<string, string> containerProperties = blobStorage.GetContainerProperties("test2");
            Assert.IsNotNull(containerProperties, "The properties were null when they were not expected to be.");
            Assert.IsTrue(containerProperties.Count > 6, "The number of container properties was not as expected.");
            bool deleteSuccess = blobStorage.DeleteContainer("test2");
            Assert.IsTrue(deleteSuccess, "The container was not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestGetContainerMetadata()
        {
            IBlobStorage blobStorage = new BlobStorage(StorageAccount.Name, StorageAccount.Key);
            bool createSucess = blobStorage.CreateContainer("test3");
            Assert.IsTrue(createSucess, "The container was not created as expected.");
            SortedList<string, string> containerMetadata = blobStorage.GetContainerMetadata("test3");
            Assert.IsNotNull(containerMetadata, "The metadata were null when they were not expected to be.");
            Assert.IsTrue(containerMetadata.Count > 6, "The number of container properties was not as expected.");
            bool deleteSuccess = blobStorage.DeleteContainer("test3");
            Assert.IsTrue(deleteSuccess, "The container was not deleted as expected.");
        }

        private static void CleanupContainers()
        {
            IBlobStorage blobStorage = new BlobStorage(StorageAccount.Name, StorageAccount.Key);
            BlobContainer[] blobContainers = blobStorage.ListContainers();
            bool failure = false;
            foreach (BlobContainer blobContainer in blobContainers)
            {
                if (blobContainer.Name.StartsWith("test", StringComparison.OrdinalIgnoreCase))
                {
                    bool deleteSuccess = blobStorage.DeleteContainer(blobContainer.Name);
                    if (!deleteSuccess)
                    {
                        Console.WriteLine("Failed to delete container named '{0}'.", blobContainer.Name);
                        failure = true;
                    }
                }
            }

            Assert.IsFalse(failure, "The clean-up failed to delete at least one container, see console output for more information.");
        }
    }
}

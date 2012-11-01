namespace StealFocus.AzureExtensions.Tests.Rest.StorageService
{
    using System;
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using StealFocus.AzureExtensions.Rest.StorageService;
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
        public void IntegrationTestGetAndSetContainerMetadata()
        {
            IBlobStorage blobStorage = new BlobStorage(StorageAccount.Name, StorageAccount.Key);

            // Create a container.
            bool createSucess = blobStorage.CreateContainer("test3");
            Assert.IsTrue(createSucess, "The container was not created as expected.");

            // Get container metadata and check there is no metadata
            SortedList<string, string> containerMetadata1 = blobStorage.GetContainerMetadata("test3");
            Assert.IsNotNull(containerMetadata1, "The metadata were null when they were not expected to be.");
            Assert.AreEqual(0, containerMetadata1.Count, "The number of metadata items for the container was not as expected.");
            
            // Set some metadata
            SortedList<string, string> containerMetadata2 = new SortedList<string, string>();
            containerMetadata2.Add("x-ms-meta-meta1", "value1"); // Add one with the correct prefix ("x-ms-meta-").
            containerMetadata2.Add("meta2", "value2"); // Add one without the prefix.
            bool setMetadataSuccess = blobStorage.SetContainerMetadata("test3", containerMetadata2);
            Assert.IsTrue(setMetadataSuccess, "The container metadata was not set as expected.");

            // Get container metadata again and check there is some metadata this time.
            SortedList<string, string> containerMetadata3 = blobStorage.GetContainerMetadata("test3");
            Assert.IsNotNull(containerMetadata3, "The metadata were null when they were not expected to be.");
            Assert.AreEqual(2, containerMetadata3.Count, "The number of metadata items for the container was not as expected.");
            Assert.IsTrue(containerMetadata3.ContainsKey("x-ms-meta-meta1"), "The key was not present in the metadata when it was expected to be so.");
            Assert.AreEqual("value1", containerMetadata3["x-ms-meta-meta1"], "The value in the metadata was not as expected.");
            Assert.IsTrue(containerMetadata3.ContainsKey("x-ms-meta-meta2"), "The key was not present in the metadata when it was expected to be so.");
            Assert.AreEqual("value2", containerMetadata3["x-ms-meta-meta2"], "The value in the metadata was not as expected.");

            // Now delete the container.
            bool deleteSuccess = blobStorage.DeleteContainer("test3");
            Assert.IsTrue(deleteSuccess, "The container was not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestGetAndSetContainerAcl()
        {
            IBlobStorage blobStorage = new BlobStorage(StorageAccount.Name, StorageAccount.Key);

            // Create a container.
            bool createSucess = blobStorage.CreateContainer("test4");
            Assert.IsTrue(createSucess, "The container was not created as expected.");

            // Check the ACL.
            ContainerAcl containerAcl1 = blobStorage.GetContainerAcl("test4");
            Assert.AreEqual("private", containerAcl1.AccessLevel, "The returned ACL was not as expected.");

            // Set ACL to "blob"
            bool aclSuccess1 = blobStorage.SetContainerAcl("test4", new ContainerAcl("blob"));
            Assert.IsTrue(aclSuccess1, "The ACL set was not a success.");

            // Check the ACL.
            ContainerAcl containerAcl2 = blobStorage.GetContainerAcl("test4");
            Assert.AreEqual("blob", containerAcl2.AccessLevel, "The returned ACL was not as expected.");

            // Set ACL to "container"
            bool aclSuccess2 = blobStorage.SetContainerAcl("test4", new ContainerAcl("container"));
            Assert.IsTrue(aclSuccess2, "The ACL set was not a success.");

            // Check the ACL.
            ContainerAcl containerAcl3 = blobStorage.GetContainerAcl("test4");
            Assert.AreEqual("container", containerAcl3.AccessLevel, "The returned ACL was not as expected.");

            // Set ACL to "private"
            bool aclSuccess3 = blobStorage.SetContainerAcl("test4", new ContainerAcl("private"));
            Assert.IsTrue(aclSuccess3, "The ACL set was not a success.");

            // Check the ACL.
            ContainerAcl containerAcl4 = blobStorage.GetContainerAcl("test4");
            Assert.AreEqual("private", containerAcl4.AccessLevel, "The returned ACL was not as expected.");

            // Now delete the container.
            bool deleteSuccess = blobStorage.DeleteContainer("test4");
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

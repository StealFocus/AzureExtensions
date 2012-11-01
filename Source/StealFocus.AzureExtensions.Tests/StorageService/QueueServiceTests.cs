namespace StealFocus.AzureExtensions.Tests.StorageService
{
    using System;
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using StealFocus.AzureExtensions.StorageService;
    using StealFocus.AzureExtensions.Tests.Configuration;

    [TestClass]
    public class QueueServiceTests
    {
        /// <remarks>
        /// Called once per test class.
        /// </remarks>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            TestAssembly.RunEmulatorIfRequired(StorageAccount.Name);
            TestAssembly.ThrottleTestClassesIfRunningUnderAzure(StorageAccount.Name);
            CleanupQueues();
        }

        /// <remarks>
        /// Called once per test class.
        /// </remarks>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            CleanupQueues();
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
        public void IntegrationTestListQueues()
        {
            IQueueService queueService = new QueueService(StorageAccount.Name, StorageAccount.Key);
            Queue[] queues = queueService.ListQueues();
            Assert.IsTrue(queues.Length > 0, "The queue list was empty when it was not expected to be so.");
            foreach (Queue queue in queues)
            {
                Console.WriteLine("Found queue name - " + queue.Name);
            }
        }

        [TestMethod]
        public void IntegrationTestListQueuesWithRetries()
        {
            IQueueService queueService = new QueueService(StorageAccount.Name, StorageAccount.Key);
            Queue[] queues = queueService.ListQueues(2);
            Assert.IsTrue(queues.Length > 0, "The queue list was empty when it was not expected to be so.");
            foreach (Queue queue in queues)
            {
                Console.WriteLine("Found queue name - " + queue.Name);
            }
        }

        [TestMethod]
        public void IntegrationTestCreateQueueAndDeleteQueue()
        {
            IQueueService queueService = new QueueService(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueService.CreateQueue("test1");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            bool deleteSuccess = queueService.DeleteQueue("test1");
            Assert.IsTrue(deleteSuccess, "The queue not deleted as expected.");
        }

        [TestMethod]
        [ExpectedException(typeof(AzureExtensionsException))]
        public void IntegrationTestCreateQueueWithBadName()
        {
            IQueueService queueService = new QueueService(StorageAccount.Name, StorageAccount.Key);

            // Can't use capital lettters.
            queueService.CreateQueue("BadQueueName");
        }

        [TestMethod]
        public void IntegrationTestGetQueueMetadata()
        {
            IQueueService queueService = new QueueService(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueService.CreateQueue("test2");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            SortedList<string, string> queueMetadata = queueService.GetQueueMetadata("test2");
            foreach (KeyValuePair<string, string> keyValuePair in queueMetadata)
            {
                Console.WriteLine("Queue metadata item '{0}' had value '{1}'", keyValuePair.Key, keyValuePair.Value);
            }

            bool deleteSuccess = queueService.DeleteQueue("test2");
            Assert.IsTrue(deleteSuccess, "The queue not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestSetQueueMetadata()
        {
            IQueueService queueService = new QueueService(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueService.CreateQueue("test3");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            SortedList<string, string> metadataList = new SortedList<string, string>();
            metadataList.Add("myMetaDataKey", "myMetaDataValue");
            bool setQueueMetadataSuccess = queueService.SetQueueMetadata("test3", metadataList);
            Assert.IsTrue(setQueueMetadataSuccess, "The metadata was not set for the queue as expected.");
            bool deleteSuccess = queueService.DeleteQueue("test3");
            Assert.IsTrue(deleteSuccess, "The queue not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestPutMessage()
        {
            IQueueService queueService = new QueueService(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueService.CreateQueue("test4");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            bool putSuccess = queueService.PutMessage("test4", "<Stuff />");
            Assert.IsTrue(putSuccess, "The message was not successfully put as expected.");
            bool deleteSuccess = queueService.DeleteQueue("test4");
            Assert.IsTrue(deleteSuccess, "The queue not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestPeekMessage()
        {
            IQueueService queueService = new QueueService(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueService.CreateQueue("test5");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            const string ExpectedMessageContent = "<Stuff />";
            bool putSuccess = queueService.PutMessage("test5", "<Stuff />");
            Assert.IsTrue(putSuccess, "The message was not successfully put as expected.");
            QueueMessage queueMessage = queueService.PeekMessage("test5");
            Assert.AreEqual(ExpectedMessageContent, queueMessage.MessageText, "The message was not as expected.");
            bool deleteSuccess = queueService.DeleteQueue("test5");
            Assert.IsTrue(deleteSuccess, "The queue not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestPeekMessageWhereNonExists()
        {
            IQueueService queueService = new QueueService(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueService.CreateQueue("test6");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            QueueMessage queueMessage = queueService.PeekMessage("test6");
            Assert.IsNull(queueMessage, "The queue message was not null when it was expected to be so.");
            bool deleteSuccess = queueService.DeleteQueue("test6");
            Assert.IsTrue(deleteSuccess, "The queue not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestGetMessage()
        {
            IQueueService queueService = new QueueService(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueService.CreateQueue("test7");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            const string ExpectedMessageContent = "<Stuff />";
            bool putSuccess = queueService.PutMessage("test7", "<Stuff />");
            Assert.IsTrue(putSuccess, "The message was not successfully put as expected.");
            QueueMessage queueMessage = queueService.GetMessage("test7");
            Assert.AreEqual(ExpectedMessageContent, queueMessage.MessageText, "The message was not as expected.");
            bool deleteSuccess = queueService.DeleteQueue("test7");
            Assert.IsTrue(deleteSuccess, "The queue not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestGetMessageWhereNonExists()
        {
            IQueueService queueService = new QueueService(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueService.CreateQueue("test8");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            QueueMessage queueMessage = queueService.GetMessage("test8");
            Assert.IsNull(queueMessage, "The queue message was not null when it was expected to be so.");
            bool deleteSuccess = queueService.DeleteQueue("test8");
            Assert.IsTrue(deleteSuccess, "The queue not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestClearMessages()
        {
            IQueueService queueService = new QueueService(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueService.CreateQueue("test9");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            bool putSuccess = queueService.PutMessage("test9", "<Stuff />");
            Assert.IsTrue(putSuccess, "The message was not successfully put as expected.");
            bool clearSuccess = queueService.ClearMessages("test9");
            Assert.IsTrue(clearSuccess, "The queue was not cleared as expected.");
            QueueMessage queueMessage = queueService.GetMessage("test9");
            Assert.IsNull(queueMessage, "The queue message was not null when it was expected to be so.");
            bool deleteSuccess = queueService.DeleteQueue("test9");
            Assert.IsTrue(deleteSuccess, "The queue not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestDeleteMessage()
        {
            IQueueService queueService = new QueueService(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueService.CreateQueue("test10");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            bool putSuccess = queueService.PutMessage("test10", "<Stuff />");
            Assert.IsTrue(putSuccess, "The message was not successfully put as expected.");
            QueueMessage queueMessageBeforeDelete = queueService.GetMessage("test10");
            Assert.IsNotNull(queueMessageBeforeDelete);
            bool deleteMessageSuccess = queueService.DeleteMessage("test10", queueMessageBeforeDelete.MessageId, queueMessageBeforeDelete.PopReceipt);
            Assert.IsTrue(deleteMessageSuccess, "The message was not indicated to be deleted when it was expected to be.");
            QueueMessage queueMessageAfterDelete = queueService.GetMessage("test10");
            Assert.IsNull(queueMessageAfterDelete, "The queue message was not null when it was expected to be so.");
            bool deleteQueueSuccess = queueService.DeleteQueue("test10");
            Assert.IsTrue(deleteQueueSuccess, "The queue not deleted as expected.");
        }

        [TestMethod]
        [ExpectedException(typeof(AzureExtensionsException))]
        public void IntegrationTestDeleteMessageWithBadPopReceipt()
        {
            IQueueService queueService = new QueueService(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueService.CreateQueue("test11");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            bool putSuccess = queueService.PutMessage("test11", "<Stuff />");
            Assert.IsTrue(putSuccess, "The message was not successfully put as expected.");
            QueueMessage queueMessageBeforeDelete = queueService.GetMessage("test11");
            Assert.IsNotNull(queueMessageBeforeDelete);
            try
            {
                queueService.DeleteMessage("test11", queueMessageBeforeDelete.MessageId, "badPopReceipt");
            }
            finally
            {
                bool deleteQueueSuccess = queueService.DeleteQueue("test11");
                Assert.IsTrue(deleteQueueSuccess, "The queue not deleted as expected.");   
            }
        }

        private static void CleanupQueues()
        {
            IQueueService queueService = new QueueService(StorageAccount.Name, StorageAccount.Key);
            Queue[] queues = queueService.ListQueues();
            bool failure = false;
            foreach (Queue queue in queues)
            {
                if (queue.Name.StartsWith("test", StringComparison.OrdinalIgnoreCase))
                {
                    bool deleteSuccess = queueService.DeleteQueue(queue.Name);
                    if (!deleteSuccess)
                    {
                        Console.WriteLine("Failed to delete queue named '{0}'.", queue.Name);
                        failure = true;
                    }
                }
            }

            Assert.IsFalse(failure, "The clean-up failed to delete at least one queue, see console output for more information.");
        }
    }
}

namespace StealFocus.AzureExtensions.Tests.Rest
{
    using System;
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using StealFocus.AzureExtensions.Rest;
    using StealFocus.AzureExtensions.Tests.Configuration;

    [TestClass]
    public class QueueStorageTests
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
            IQueueStorage queueStorage = new QueueStorage(StorageAccount.Name, StorageAccount.Key);
            Queue[] queues = queueStorage.ListQueues();
            Assert.IsTrue(queues.Length > 0, "The queue list was empty when it was not expected to be so.");
            foreach (Queue queue in queues)
            {
                Console.WriteLine("Found queue name - " + queue.Name);
            }
        }

        [TestMethod]
        public void IntegrationTestListQueuesWithRetries()
        {
            IQueueStorage queueStorage = new QueueStorage(StorageAccount.Name, StorageAccount.Key);
            Queue[] queues = queueStorage.ListQueues(2);
            Assert.IsTrue(queues.Length > 0, "The queue list was empty when it was not expected to be so.");
            foreach (Queue queue in queues)
            {
                Console.WriteLine("Found queue name - " + queue.Name);
            }
        }

        [TestMethod]
        public void IntegrationTestCreateQueueAndDeleteQueue()
        {
            IQueueStorage queueStorage = new QueueStorage(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueStorage.CreateQueue("test1");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            bool deleteSuccess = queueStorage.DeleteQueue("test1");
            Assert.IsTrue(deleteSuccess, "The queue not deleted as expected.");
        }

        [TestMethod]
        [ExpectedException(typeof(AzureExtensionsException))]
        public void IntegrationTestCreateQueueWithBadName()
        {
            IQueueStorage queueStorage = new QueueStorage(StorageAccount.Name, StorageAccount.Key);

            // Can't use capital lettters.
            queueStorage.CreateQueue("BadQueueName");
        }

        [TestMethod]
        public void IntegrationTestGetQueueMetadata()
        {
            IQueueStorage queueStorage = new QueueStorage(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueStorage.CreateQueue("test2");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            SortedList<string, string> queueMetadata = queueStorage.GetQueueMetadata("test2");
            foreach (KeyValuePair<string, string> keyValuePair in queueMetadata)
            {
                Console.WriteLine("Queue metadata item '{0}' had value '{1}'", keyValuePair.Key, keyValuePair.Value);
            }

            bool deleteSuccess = queueStorage.DeleteQueue("test2");
            Assert.IsTrue(deleteSuccess, "The queue not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestSetQueueMetadata()
        {
            IQueueStorage queueStorage = new QueueStorage(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueStorage.CreateQueue("test3");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            SortedList<string, string> metadataList = new SortedList<string, string>();
            metadataList.Add("myMetaDataKey", "myMetaDataValue");
            bool setQueueMetadataSuccess = queueStorage.SetQueueMetadata("test3", metadataList);
            Assert.IsTrue(setQueueMetadataSuccess, "The metadata was not set for the queue as expected.");
            bool deleteSuccess = queueStorage.DeleteQueue("test3");
            Assert.IsTrue(deleteSuccess, "The queue not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestPutMessage()
        {
            IQueueStorage queueStorage = new QueueStorage(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueStorage.CreateQueue("test4");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            bool putSuccess = queueStorage.PutMessage("test4", "<Stuff />");
            Assert.IsTrue(putSuccess, "The message was not successfully put as expected.");
            bool deleteSuccess = queueStorage.DeleteQueue("test4");
            Assert.IsTrue(deleteSuccess, "The queue not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestPeekMessage()
        {
            IQueueStorage queueStorage = new QueueStorage(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueStorage.CreateQueue("test5");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            const string ExpectedMessageContent = "<Stuff />";
            bool putSuccess = queueStorage.PutMessage("test5", "<Stuff />");
            Assert.IsTrue(putSuccess, "The message was not successfully put as expected.");
            QueueMessage queueMessage = queueStorage.PeekMessage("test5");
            Assert.AreEqual(ExpectedMessageContent, queueMessage.MessageText, "The message was not as expected.");
            bool deleteSuccess = queueStorage.DeleteQueue("test5");
            Assert.IsTrue(deleteSuccess, "The queue not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestPeekMessageWhereNonExists()
        {
            IQueueStorage queueStorage = new QueueStorage(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueStorage.CreateQueue("test6");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            QueueMessage queueMessage = queueStorage.PeekMessage("test6");
            Assert.IsNull(queueMessage, "The queue message was not null when it was expected to be so.");
            bool deleteSuccess = queueStorage.DeleteQueue("test6");
            Assert.IsTrue(deleteSuccess, "The queue not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestGetMessage()
        {
            IQueueStorage queueStorage = new QueueStorage(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueStorage.CreateQueue("test7");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            const string ExpectedMessageContent = "<Stuff />";
            bool putSuccess = queueStorage.PutMessage("test7", "<Stuff />");
            Assert.IsTrue(putSuccess, "The message was not successfully put as expected.");
            QueueMessage queueMessage = queueStorage.GetMessage("test7");
            Assert.AreEqual(ExpectedMessageContent, queueMessage.MessageText, "The message was not as expected.");
            bool deleteSuccess = queueStorage.DeleteQueue("test7");
            Assert.IsTrue(deleteSuccess, "The queue not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestGetMessageWhereNonExists()
        {
            IQueueStorage queueStorage = new QueueStorage(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueStorage.CreateQueue("test8");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            QueueMessage queueMessage = queueStorage.GetMessage("test8");
            Assert.IsNull(queueMessage, "The queue message was not null when it was expected to be so.");
            bool deleteSuccess = queueStorage.DeleteQueue("test8");
            Assert.IsTrue(deleteSuccess, "The queue not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestClearMessages()
        {
            IQueueStorage queueStorage = new QueueStorage(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueStorage.CreateQueue("test9");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            bool putSuccess = queueStorage.PutMessage("test9", "<Stuff />");
            Assert.IsTrue(putSuccess, "The message was not successfully put as expected.");
            bool clearSuccess = queueStorage.ClearMessages("test9");
            Assert.IsTrue(clearSuccess, "The queue was not cleared as expected.");
            QueueMessage queueMessage = queueStorage.GetMessage("test9");
            Assert.IsNull(queueMessage, "The queue message was not null when it was expected to be so.");
            bool deleteSuccess = queueStorage.DeleteQueue("test9");
            Assert.IsTrue(deleteSuccess, "The queue not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestDeleteMessage()
        {
            IQueueStorage queueStorage = new QueueStorage(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueStorage.CreateQueue("test10");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            bool putSuccess = queueStorage.PutMessage("test10", "<Stuff />");
            Assert.IsTrue(putSuccess, "The message was not successfully put as expected.");
            QueueMessage queueMessageBeforeDelete = queueStorage.GetMessage("test10");
            Assert.IsNotNull(queueMessageBeforeDelete);
            bool deleteMessageSuccess = queueStorage.DeleteMessage("test10", queueMessageBeforeDelete.MessageId, queueMessageBeforeDelete.PopReceipt);
            Assert.IsTrue(deleteMessageSuccess, "The message was not indicated to be deleted when it was expected to be.");
            QueueMessage queueMessageAfterDelete = queueStorage.GetMessage("test10");
            Assert.IsNull(queueMessageAfterDelete, "The queue message was not null when it was expected to be so.");
            bool deleteQueueSuccess = queueStorage.DeleteQueue("test10");
            Assert.IsTrue(deleteQueueSuccess, "The queue not deleted as expected.");
        }

        [TestMethod]
        [ExpectedException(typeof(AzureExtensionsException))]
        public void IntegrationTestDeleteMessageWithBadPopReceipt()
        {
            IQueueStorage queueStorage = new QueueStorage(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = queueStorage.CreateQueue("test11");
            Assert.IsTrue(createSuccess, "The queue was not created as expected.");
            bool putSuccess = queueStorage.PutMessage("test11", "<Stuff />");
            Assert.IsTrue(putSuccess, "The message was not successfully put as expected.");
            QueueMessage queueMessageBeforeDelete = queueStorage.GetMessage("test11");
            Assert.IsNotNull(queueMessageBeforeDelete);
            try
            {
                queueStorage.DeleteMessage("test11", queueMessageBeforeDelete.MessageId, "badPopReceipt");
            }
            finally
            {
                bool deleteQueueSuccess = queueStorage.DeleteQueue("test11");
                Assert.IsTrue(deleteQueueSuccess, "The queue not deleted as expected.");   
            }
        }

        private static void CleanupQueues()
        {
            IQueueStorage queueStorage = new QueueStorage(StorageAccount.Name, StorageAccount.Key);
            Queue[] queues = queueStorage.ListQueues();
            bool failure = false;
            foreach (Queue queue in queues)
            {
                if (queue.Name.StartsWith("test", StringComparison.OrdinalIgnoreCase))
                {
                    bool deleteSuccess = queueStorage.DeleteQueue(queue.Name);
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

namespace StealFocus.AzureExtensions.Tests.Rest.StorageService
{
    using System;
    using System.Globalization;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using StealFocus.AzureExtensions.Rest.StorageService;
    using StealFocus.AzureExtensions.Tests.Configuration;

    [TestClass]
    public class TableStorageTests
    {
        /// <remarks>
        /// Called once per test class.
        /// </remarks>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            TestAssembly.RunEmulatorIfRequired(StorageAccount.Name);
            TestAssembly.ThrottleTestClassesIfRunningUnderAzure(StorageAccount.Name);
            CleanupTables();
        }

        /// <remarks>
        /// Called once per test class.
        /// </remarks>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            CleanupTables();
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
        public void IntegrationTestListNames()
        {
            ITableStorage tableStorage = new TableStorage(StorageAccount.Name, StorageAccount.Key);
            Table[] tables = tableStorage.ListTables();
            Assert.IsTrue(tables.Length > 0, "The table list was empty when it was not expected to be so.");
            foreach (Table table in tables)
            {
                Console.WriteLine("Found table name - " + table.Name);
            }
        }

        [TestMethod]
        public void IntegrationTestListNamesWithRetries()
        {
            ITableStorage tableStorage = new TableStorage(StorageAccount.Name, StorageAccount.Key);
            Table[] tables = tableStorage.ListTables(2);
            Assert.IsTrue(tables.Length > 0, "The table list was empty when it was not expected to be so.");
            foreach (Table table in tables)
            {
                Console.WriteLine("Found table name - " + table.Name);
            }
        }

        [TestMethod]
        public void IntegrationTestCreateTableAndDeleteTable()
        {
            ITableStorage tableStorage = new TableStorage(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = tableStorage.CreateTable("Test1");
            Assert.IsTrue(createSuccess, "The table was not created as expected.");
            bool deleteSuccess = tableStorage.DeleteTable("Test1");
            Assert.IsTrue(deleteSuccess, "The table was not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestCreateTableAndDeleteTableWithRetries()
        {
            ITableStorage tableStorage = new TableStorage(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = tableStorage.CreateTable("Test2", 2);
            Assert.IsTrue(createSuccess, "The table was not created as expected.");
            bool deleteSuccess = tableStorage.DeleteTable("Test2", 2);
            Assert.IsTrue(deleteSuccess, "The table was not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestInsertEntity()
        {
            TableEntity tableEntity = new TableEntity
                {
                    SomeBooleanProperty = true,
                    SomeByteProperty = 1,
                    SomeCharacterProperty = 'a',
                    SomeDateTimeProperty = DateTime.Now,
                    SomeDecimalProperty = 2.0m,
                    SomeDoubleProperty = 3.0,
                    SomeFloatProperty = 4.0f,
                    SomeIntegerProperty = 5,
                    SomeLongProperty = 6,
                    SomeShortProperty = 7,
                    SomeStringProperty = "abc"
                };
            ITableStorage tableStorage = new TableStorage(StorageAccount.Name, StorageAccount.Key);
            tableStorage.CreateTable("Test3");
            bool insertSuccess = tableStorage.InsertEntity(
                "Test3", 
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture), 
                tableEntity.SomeStringProperty, 
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            tableStorage.DeleteTable("Test3");
        }

        [TestMethod]
        public void IntegrationTestInsertEntityWithRetries()
        {
            TableEntity tableEntity = new TableEntity
            {
                SomeBooleanProperty = true,
                SomeByteProperty = 1,
                SomeCharacterProperty = 'a',
                SomeDateTimeProperty = DateTime.Now,
                SomeDecimalProperty = 2.0m,
                SomeDoubleProperty = 3.0,
                SomeFloatProperty = 4.0f,
                SomeIntegerProperty = 5,
                SomeLongProperty = 6,
                SomeShortProperty = 7,
                SomeStringProperty = "abc"
            };
            ITableStorage tableStorage = new TableStorage(StorageAccount.Name, StorageAccount.Key);
            tableStorage.CreateTable("Test4");
            bool insertSuccess = tableStorage.InsertEntity(
                "Test4",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity,
                2);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            tableStorage.DeleteTable("Test4");
        }

        [TestMethod]
        public void IntegrationTestInsertEntityWithDateTimeMaxValue()
        {
            TableEntity tableEntity = new TableEntity
            {
                SomeBooleanProperty = true,
                SomeByteProperty = 1,
                SomeCharacterProperty = 'a',
                SomeDateTimeProperty = DateTime.MaxValue,
                SomeDecimalProperty = 2.0m,
                SomeDoubleProperty = 3.0,
                SomeFloatProperty = 4.0f,
                SomeIntegerProperty = 5,
                SomeLongProperty = 6,
                SomeShortProperty = 7,
                SomeStringProperty = "abc"
            };
            ITableStorage tableStorage = new TableStorage(StorageAccount.Name, StorageAccount.Key);
            tableStorage.CreateTable("Test5");
            bool insertSuccess = tableStorage.InsertEntity(
                "Test5",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            tableStorage.DeleteTable("Test5");
        }

        [TestMethod]
        public void IntegrationTestInsertEntityWithDateTimeMinValue()
        {
            TableEntity tableEntity = new TableEntity
            {
                SomeBooleanProperty = true,
                SomeByteProperty = 1,
                SomeCharacterProperty = 'a',
                SomeDateTimeProperty = DateTime.MinValue,
                SomeDecimalProperty = 2.0m,
                SomeDoubleProperty = 3.0,
                SomeFloatProperty = 4.0f,
                SomeIntegerProperty = 5,
                SomeLongProperty = 6,
                SomeShortProperty = 7,
                SomeStringProperty = "abc"
            };
            ITableStorage tableStorage = new TableStorage(StorageAccount.Name, StorageAccount.Key);
            tableStorage.CreateTable("Test6");
            bool insertSuccess = tableStorage.InsertEntity(
                "Test6",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            tableStorage.DeleteTable("Test6");
        }

        [TestMethod]
        public void IntegrationTestInsertEntityWithNullProperty()
        {
            TableEntity tableEntity = new TableEntity
            {
                SomeBooleanProperty = true,
                SomeByteProperty = 1,
                SomeCharacterProperty = 'a',
                SomeDateTimeProperty = DateTime.Now,
                SomeDecimalProperty = 2.0m,
                SomeDoubleProperty = 3.0,
                SomeFloatProperty = 4.0f,
                SomeIntegerProperty = 5,
                SomeLongProperty = 6,
                SomeShortProperty = 7,
                SomeStringProperty = null
            };
            ITableStorage tableStorage = new TableStorage(StorageAccount.Name, StorageAccount.Key);
            tableStorage.CreateTable("Test7");
            bool insertSuccess = tableStorage.InsertEntity(
                "Test7",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeIntegerProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            tableStorage.DeleteTable("Test7");
        }

        [TestMethod]
        public void IntegrationTestGetEntity()
        {
            TableEntity tableEntity = new TableEntity
            {
                SomeBooleanProperty = true,
                SomeByteProperty = 1,
                SomeCharacterProperty = 'a',
                SomeDateTimeProperty = DateTime.Now,
                SomeDecimalProperty = 2.0m,
                SomeDoubleProperty = 3.0,
                SomeFloatProperty = 4.0f,
                SomeIntegerProperty = 5,
                SomeLongProperty = 6,
                SomeShortProperty = 7,
                SomeStringProperty = "abc"
            };
            ITableStorage tableStorage = new TableStorage(StorageAccount.Name, StorageAccount.Key);
            tableStorage.CreateTable("Test8");
            bool insertSuccess = tableStorage.InsertEntity(
                "Test8",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            string entityXml = tableStorage.GetEntity(
                "Test8", 
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture), 
                tableEntity.SomeStringProperty);
            Assert.IsNotNull(entityXml);
            tableStorage.DeleteTable("Test8");
        }

        [TestMethod]
        public void IntegrationTestGetEntityWithRetries()
        {
            TableEntity tableEntity = new TableEntity
            {
                SomeBooleanProperty = true,
                SomeByteProperty = 1,
                SomeCharacterProperty = 'a',
                SomeDateTimeProperty = DateTime.Now,
                SomeDecimalProperty = 2.0m,
                SomeDoubleProperty = 3.0,
                SomeFloatProperty = 4.0f,
                SomeIntegerProperty = 5,
                SomeLongProperty = 6,
                SomeShortProperty = 7,
                SomeStringProperty = "abc"
            };
            ITableStorage tableStorage = new TableStorage(StorageAccount.Name, StorageAccount.Key);
            tableStorage.CreateTable("Test9");
            bool insertSuccess = tableStorage.InsertEntity(
                "Test9",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            string entityXml = tableStorage.GetEntity(
                "Test9",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                2);
            Assert.IsNotNull(entityXml);
            tableStorage.DeleteTable("Test9");
        }

        [TestMethod]
        public void IntegrationTestQueryEntities()
        {
            TableEntity tableEntity1 = new TableEntity
            {
                SomeBooleanProperty = true,
                SomeByteProperty = 1,
                SomeCharacterProperty = 'a',
                SomeDateTimeProperty = DateTime.Now,
                SomeDecimalProperty = 2.0m,
                SomeDoubleProperty = 3.0,
                SomeFloatProperty = 4.0f,
                SomeIntegerProperty = 5,
                SomeLongProperty = 6,
                SomeShortProperty = 7,
                SomeStringProperty = "abc"
            };
            TableEntity tableEntity2 = new TableEntity
            {
                SomeBooleanProperty = true,
                SomeByteProperty = 1,
                SomeCharacterProperty = 'a',
                SomeDateTimeProperty = DateTime.Now,
                SomeDecimalProperty = 2.0m,
                SomeDoubleProperty = 3.0,
                SomeFloatProperty = 4.0f,
                SomeIntegerProperty = 5,
                SomeLongProperty = 6,
                SomeShortProperty = 7,
                SomeStringProperty = "def"
            };
            ITableStorage tableStorage = new TableStorage(StorageAccount.Name, StorageAccount.Key);
            tableStorage.CreateTable("Test10");
            bool insertSuccess1 = tableStorage.InsertEntity(
                "Test10",
                tableEntity1.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity1.SomeStringProperty,
                tableEntity1);
            Assert.IsTrue(insertSuccess1, "The entity was not successfully inserted as expected.");
            bool insertSuccess2 = tableStorage.InsertEntity(
                "Test10",
                tableEntity2.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity2.SomeStringProperty,
                tableEntity2);
            Assert.IsTrue(insertSuccess2, "The entity was not successfully inserted as expected.");
            string entitiesXml = tableStorage.QueryEntities("Test10", "PartitionKey eq 'a'");
            Assert.IsNotNull(entitiesXml);
            tableStorage.DeleteTable("Test10");
        }

        [TestMethod]
        public void IntegrationTestQueryEntitiesWithRetries()
        {
            TableEntity tableEntity1 = new TableEntity
            {
                SomeBooleanProperty = true,
                SomeByteProperty = 1,
                SomeCharacterProperty = 'a',
                SomeDateTimeProperty = DateTime.Now,
                SomeDecimalProperty = 2.0m,
                SomeDoubleProperty = 3.0,
                SomeFloatProperty = 4.0f,
                SomeIntegerProperty = 5,
                SomeLongProperty = 6,
                SomeShortProperty = 7,
                SomeStringProperty = "abc"
            };
            TableEntity tableEntity2 = new TableEntity
            {
                SomeBooleanProperty = true,
                SomeByteProperty = 1,
                SomeCharacterProperty = 'a',
                SomeDateTimeProperty = DateTime.Now,
                SomeDecimalProperty = 2.0m,
                SomeDoubleProperty = 3.0,
                SomeFloatProperty = 4.0f,
                SomeIntegerProperty = 5,
                SomeLongProperty = 6,
                SomeShortProperty = 7,
                SomeStringProperty = "def"
            };
            ITableStorage tableStorage = new TableStorage(StorageAccount.Name, StorageAccount.Key);
            tableStorage.CreateTable("Test11");
            bool insertSuccess1 = tableStorage.InsertEntity(
                "Test11",
                tableEntity1.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity1.SomeStringProperty,
                tableEntity1);
            Assert.IsTrue(insertSuccess1, "The entity was not successfully inserted as expected.");
            bool insertSuccess2 = tableStorage.InsertEntity(
                "Test11",
                tableEntity2.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity2.SomeStringProperty,
                tableEntity2);
            Assert.IsTrue(insertSuccess2, "The entity was not successfully inserted as expected.");
            string entitiesXml = tableStorage.QueryEntities("Test11", "PartitionKey eq 'a'", 2);
            Assert.IsNotNull(entitiesXml);
            tableStorage.DeleteTable("Test11");
        }

        [TestMethod]
        public void IntegrationTestReplaceUpdateEntity()
        {
            TableEntity tableEntity = new TableEntity
            {
                SomeBooleanProperty = true,
                SomeByteProperty = 1,
                SomeCharacterProperty = 'a',
                SomeDateTimeProperty = DateTime.Now,
                SomeDecimalProperty = 2.0m,
                SomeDoubleProperty = 3.0,
                SomeFloatProperty = 4.0f,
                SomeIntegerProperty = 5,
                SomeLongProperty = 6,
                SomeShortProperty = 7,
                SomeStringProperty = "abc"
            };
            ITableStorage tableStorage = new TableStorage(StorageAccount.Name, StorageAccount.Key);
            tableStorage.CreateTable("Test12");
            bool insertSuccess = tableStorage.InsertEntity(
                "Test12",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            tableEntity.SomeByteProperty = 2;
            bool updateSuccess = tableStorage.ReplaceUpdateEntity(
                "Test12",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(updateSuccess);
            tableStorage.DeleteTable("Test12");
        }

        [TestMethod]
        public void IntegrationTestReplaceUpdateEntityWithRetries()
        {
            TableEntity tableEntity = new TableEntity
            {
                SomeBooleanProperty = true,
                SomeByteProperty = 1,
                SomeCharacterProperty = 'a',
                SomeDateTimeProperty = DateTime.Now,
                SomeDecimalProperty = 2.0m,
                SomeDoubleProperty = 3.0,
                SomeFloatProperty = 4.0f,
                SomeIntegerProperty = 5,
                SomeLongProperty = 6,
                SomeShortProperty = 7,
                SomeStringProperty = "abc"
            };
            ITableStorage tableStorage = new TableStorage(StorageAccount.Name, StorageAccount.Key);
            tableStorage.CreateTable("Test13");
            bool insertSuccess = tableStorage.InsertEntity(
                "Test13",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            tableEntity.SomeByteProperty = 2;
            bool updateSuccess = tableStorage.ReplaceUpdateEntity(
                "Test13",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity,
                2);
            Assert.IsTrue(updateSuccess);
            tableStorage.DeleteTable("Test13");
        }

        [TestMethod]
        public void IntegrationTestMergeUpdateEntity()
        {
            TableEntity tableEntity = new TableEntity
            {
                SomeBooleanProperty = true,
                SomeByteProperty = 1,
                SomeCharacterProperty = 'a',
                SomeDateTimeProperty = DateTime.Now,
                SomeDecimalProperty = 2.0m,
                SomeDoubleProperty = 3.0,
                SomeFloatProperty = 4.0f,
                SomeIntegerProperty = 5,
                SomeLongProperty = 6,
                SomeShortProperty = 7,
                SomeStringProperty = "abc"
            };
            ITableStorage tableStorage = new TableStorage(StorageAccount.Name, StorageAccount.Key);
            tableStorage.CreateTable("Test14");
            bool insertSuccess = tableStorage.InsertEntity(
                "Test14",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            tableEntity.SomeByteProperty = 2;
            bool updateSuccess = tableStorage.MergeUpdateEntity(
                "Test14",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(updateSuccess);
            tableStorage.DeleteTable("Test14");
        }

        [TestMethod]
        public void IntegrationTestMergeUpdateEntityWithRetries()
        {
            TableEntity tableEntity = new TableEntity
            {
                SomeBooleanProperty = true,
                SomeByteProperty = 1,
                SomeCharacterProperty = 'a',
                SomeDateTimeProperty = DateTime.Now,
                SomeDecimalProperty = 2.0m,
                SomeDoubleProperty = 3.0,
                SomeFloatProperty = 4.0f,
                SomeIntegerProperty = 5,
                SomeLongProperty = 6,
                SomeShortProperty = 7,
                SomeStringProperty = "abc"
            };
            ITableStorage tableStorage = new TableStorage(StorageAccount.Name, StorageAccount.Key);
            tableStorage.CreateTable("Test15");
            bool insertSuccess = tableStorage.InsertEntity(
                "Test15",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            tableEntity.SomeByteProperty = 2;
            bool updateSuccess = tableStorage.MergeUpdateEntity(
                "Test15",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity,
                2);
            Assert.IsTrue(updateSuccess);
            tableStorage.DeleteTable("Test15");
        }

        [TestMethod]
        public void IntegrationTestDeleteEntity()
        {
            TableEntity tableEntity = new TableEntity
            {
                SomeBooleanProperty = true,
                SomeByteProperty = 1,
                SomeCharacterProperty = 'a',
                SomeDateTimeProperty = DateTime.Now,
                SomeDecimalProperty = 2.0m,
                SomeDoubleProperty = 3.0,
                SomeFloatProperty = 4.0f,
                SomeIntegerProperty = 5,
                SomeLongProperty = 6,
                SomeShortProperty = 7,
                SomeStringProperty = "abc"
            };
            ITableStorage tableStorage = new TableStorage(StorageAccount.Name, StorageAccount.Key);
            tableStorage.CreateTable("Test16");
            bool insertSuccess = tableStorage.InsertEntity(
                "Test16",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            bool deleteSuccess = tableStorage.DeleteEntity(
                "Test16",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty);
            Assert.IsTrue(deleteSuccess);
            tableStorage.DeleteTable("Test16");
        }

        [TestMethod]
        public void IntegrationTestDeleteEntityWithRetries()
        {
            TableEntity tableEntity = new TableEntity
            {
                SomeBooleanProperty = true,
                SomeByteProperty = 1,
                SomeCharacterProperty = 'a',
                SomeDateTimeProperty = DateTime.Now,
                SomeDecimalProperty = 2.0m,
                SomeDoubleProperty = 3.0,
                SomeFloatProperty = 4.0f,
                SomeIntegerProperty = 5,
                SomeLongProperty = 6,
                SomeShortProperty = 7,
                SomeStringProperty = "abc"
            };
            ITableStorage tableStorage = new TableStorage(StorageAccount.Name, StorageAccount.Key);
            tableStorage.CreateTable("Test17");
            bool insertSuccess = tableStorage.InsertEntity(
                "Test17",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            bool deleteSuccess = tableStorage.DeleteEntity(
                "Test17",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                2);
            Assert.IsTrue(deleteSuccess);
            tableStorage.DeleteTable("Test17");
        }

        private static void CleanupTables()
        {
            ITableStorage tableStorage = new TableStorage(StorageAccount.Name, StorageAccount.Key);
            Table[] tables = tableStorage.ListTables();
            bool failure = false;
            foreach (Table table in tables)
            {
                if (table.Name.StartsWith("Test", StringComparison.OrdinalIgnoreCase))
                {
                    bool deleteSuccess = tableStorage.DeleteTable(table.Name);
                    if (!deleteSuccess)
                    {
                        Console.WriteLine("Failed to delete table named '{0}'.", table.Name);
                        failure = true;
                    }
                }
            }

            Assert.IsFalse(failure, "The clean-up failed to delete at least one table, see console output for more information.");
        }
    }
}

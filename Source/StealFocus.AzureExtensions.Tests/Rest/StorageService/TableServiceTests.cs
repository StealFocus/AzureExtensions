namespace StealFocus.AzureExtensions.Tests.Rest.StorageService
{
    using System;
    using System.Globalization;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using StealFocus.AzureExtensions.Rest.StorageService;
    using StealFocus.AzureExtensions.Tests.Configuration;

    [TestClass]
    public class TableServiceTests
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
            ITableService tableService = new TableService(StorageAccount.Name, StorageAccount.Key);
            Table[] tables = tableService.ListTables();
            Assert.IsTrue(tables.Length > 0, "The table list was empty when it was not expected to be so.");
            foreach (Table table in tables)
            {
                Console.WriteLine("Found table name - " + table.Name);
            }
        }

        [TestMethod]
        public void IntegrationTestListNamesWithRetries()
        {
            ITableService tableService = new TableService(StorageAccount.Name, StorageAccount.Key);
            Table[] tables = tableService.ListTables(2);
            Assert.IsTrue(tables.Length > 0, "The table list was empty when it was not expected to be so.");
            foreach (Table table in tables)
            {
                Console.WriteLine("Found table name - " + table.Name);
            }
        }

        [TestMethod]
        public void IntegrationTestCreateTableAndDeleteTable()
        {
            ITableService tableService = new TableService(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = tableService.CreateTable("Test1");
            Assert.IsTrue(createSuccess, "The table was not created as expected.");
            bool deleteSuccess = tableService.DeleteTable("Test1");
            Assert.IsTrue(deleteSuccess, "The table was not deleted as expected.");
        }

        [TestMethod]
        public void IntegrationTestCreateTableAndDeleteTableWithRetries()
        {
            ITableService tableService = new TableService(StorageAccount.Name, StorageAccount.Key);
            bool createSuccess = tableService.CreateTable("Test2", 2);
            Assert.IsTrue(createSuccess, "The table was not created as expected.");
            bool deleteSuccess = tableService.DeleteTable("Test2", 2);
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
            ITableService tableService = new TableService(StorageAccount.Name, StorageAccount.Key);
            tableService.CreateTable("Test3");
            bool insertSuccess = tableService.InsertEntity(
                "Test3", 
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture), 
                tableEntity.SomeStringProperty, 
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            tableService.DeleteTable("Test3");
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
            ITableService tableService = new TableService(StorageAccount.Name, StorageAccount.Key);
            tableService.CreateTable("Test4");
            bool insertSuccess = tableService.InsertEntity(
                "Test4",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity,
                2);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            tableService.DeleteTable("Test4");
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
            ITableService tableService = new TableService(StorageAccount.Name, StorageAccount.Key);
            tableService.CreateTable("Test5");
            bool insertSuccess = tableService.InsertEntity(
                "Test5",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            tableService.DeleteTable("Test5");
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
            ITableService tableService = new TableService(StorageAccount.Name, StorageAccount.Key);
            tableService.CreateTable("Test6");
            bool insertSuccess = tableService.InsertEntity(
                "Test6",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            tableService.DeleteTable("Test6");
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
            ITableService tableService = new TableService(StorageAccount.Name, StorageAccount.Key);
            tableService.CreateTable("Test7");
            bool insertSuccess = tableService.InsertEntity(
                "Test7",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeIntegerProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            tableService.DeleteTable("Test7");
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
            ITableService tableService = new TableService(StorageAccount.Name, StorageAccount.Key);
            tableService.CreateTable("Test8");
            bool insertSuccess = tableService.InsertEntity(
                "Test8",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            string entityXml = tableService.GetEntity(
                "Test8", 
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture), 
                tableEntity.SomeStringProperty);
            Assert.IsNotNull(entityXml);
            tableService.DeleteTable("Test8");
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
            ITableService tableService = new TableService(StorageAccount.Name, StorageAccount.Key);
            tableService.CreateTable("Test9");
            bool insertSuccess = tableService.InsertEntity(
                "Test9",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            string entityXml = tableService.GetEntity(
                "Test9",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                2);
            Assert.IsNotNull(entityXml);
            tableService.DeleteTable("Test9");
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
            ITableService tableService = new TableService(StorageAccount.Name, StorageAccount.Key);
            tableService.CreateTable("Test10");
            bool insertSuccess1 = tableService.InsertEntity(
                "Test10",
                tableEntity1.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity1.SomeStringProperty,
                tableEntity1);
            Assert.IsTrue(insertSuccess1, "The entity was not successfully inserted as expected.");
            bool insertSuccess2 = tableService.InsertEntity(
                "Test10",
                tableEntity2.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity2.SomeStringProperty,
                tableEntity2);
            Assert.IsTrue(insertSuccess2, "The entity was not successfully inserted as expected.");
            string entitiesXml = tableService.QueryEntities("Test10", "PartitionKey eq 'a'");
            Assert.IsNotNull(entitiesXml);
            tableService.DeleteTable("Test10");
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
            ITableService tableService = new TableService(StorageAccount.Name, StorageAccount.Key);
            tableService.CreateTable("Test11");
            bool insertSuccess1 = tableService.InsertEntity(
                "Test11",
                tableEntity1.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity1.SomeStringProperty,
                tableEntity1);
            Assert.IsTrue(insertSuccess1, "The entity was not successfully inserted as expected.");
            bool insertSuccess2 = tableService.InsertEntity(
                "Test11",
                tableEntity2.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity2.SomeStringProperty,
                tableEntity2);
            Assert.IsTrue(insertSuccess2, "The entity was not successfully inserted as expected.");
            string entitiesXml = tableService.QueryEntities("Test11", "PartitionKey eq 'a'", 2);
            Assert.IsNotNull(entitiesXml);
            tableService.DeleteTable("Test11");
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
            ITableService tableService = new TableService(StorageAccount.Name, StorageAccount.Key);
            tableService.CreateTable("Test12");
            bool insertSuccess = tableService.InsertEntity(
                "Test12",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            tableEntity.SomeByteProperty = 2;
            bool updateSuccess = tableService.ReplaceUpdateEntity(
                "Test12",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(updateSuccess);
            tableService.DeleteTable("Test12");
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
            ITableService tableService = new TableService(StorageAccount.Name, StorageAccount.Key);
            tableService.CreateTable("Test13");
            bool insertSuccess = tableService.InsertEntity(
                "Test13",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            tableEntity.SomeByteProperty = 2;
            bool updateSuccess = tableService.ReplaceUpdateEntity(
                "Test13",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity,
                2);
            Assert.IsTrue(updateSuccess);
            tableService.DeleteTable("Test13");
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
            ITableService tableService = new TableService(StorageAccount.Name, StorageAccount.Key);
            tableService.CreateTable("Test14");
            bool insertSuccess = tableService.InsertEntity(
                "Test14",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            tableEntity.SomeByteProperty = 2;
            bool updateSuccess = tableService.MergeUpdateEntity(
                "Test14",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(updateSuccess);
            tableService.DeleteTable("Test14");
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
            ITableService tableService = new TableService(StorageAccount.Name, StorageAccount.Key);
            tableService.CreateTable("Test15");
            bool insertSuccess = tableService.InsertEntity(
                "Test15",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            tableEntity.SomeByteProperty = 2;
            bool updateSuccess = tableService.MergeUpdateEntity(
                "Test15",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity,
                2);
            Assert.IsTrue(updateSuccess);
            tableService.DeleteTable("Test15");
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
            ITableService tableService = new TableService(StorageAccount.Name, StorageAccount.Key);
            tableService.CreateTable("Test16");
            bool insertSuccess = tableService.InsertEntity(
                "Test16",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            bool deleteSuccess = tableService.DeleteEntity(
                "Test16",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty);
            Assert.IsTrue(deleteSuccess);
            tableService.DeleteTable("Test16");
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
            ITableService tableService = new TableService(StorageAccount.Name, StorageAccount.Key);
            tableService.CreateTable("Test17");
            bool insertSuccess = tableService.InsertEntity(
                "Test17",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                tableEntity);
            Assert.IsTrue(insertSuccess, "The entity was not successfully inserted as expected.");
            bool deleteSuccess = tableService.DeleteEntity(
                "Test17",
                tableEntity.SomeCharacterProperty.ToString(CultureInfo.CurrentCulture),
                tableEntity.SomeStringProperty,
                2);
            Assert.IsTrue(deleteSuccess);
            tableService.DeleteTable("Test17");
        }

        private static void CleanupTables()
        {
            ITableService tableService = new TableService(StorageAccount.Name, StorageAccount.Key);
            Table[] tables = tableService.ListTables();
            bool failure = false;
            foreach (Table table in tables)
            {
                if (table.Name.StartsWith("Test", StringComparison.OrdinalIgnoreCase))
                {
                    bool deleteSuccess = tableService.DeleteTable(table.Name);
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

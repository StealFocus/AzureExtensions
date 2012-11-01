namespace StealFocus.AzureExtensions.Rest.StorageService
{
    using System;

    public interface ITableStorage
    {
        /// <summary>
        /// Lists the Table names found in the Storage Account.
        /// </summary>
        /// <returns>An <see cref="Array" />. The list of Tables.</returns>
        Table[] ListTables();

        /// <summary>
        /// Lists the Table names found in the Storage Account, will retry a given number of times should a failure be 
        /// encountered using an interval of 100 milliseconds between attempts.
        /// </summary>
        /// <param name="numberOfAttempts">An <see cref="int"/>. The number of attempts to make.</param>
        /// <returns>An <see cref="Array" />. The list of Tables.</returns>
        Table[] ListTables(int numberOfAttempts);

        /// <summary>
        /// Lists the Table names found in the Storage Account, will retry a given number of times should a failure be 
        /// encountered using the supplied interval between attempts.
        /// </summary>
        /// <param name="numberOfAttempts">An <see cref="int"/>. The number of attempts to make.</param>
        /// <param name="timeBetweenAttemptsInMilliseconds">An <see cref="int"/>. The time between attempts in milliseconds.</param>
        /// <returns>An <see cref="Array" />. The list of Tables.</returns>
        Table[] ListTables(int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        bool CreateTable(string tableName);

        bool CreateTable(string tableName, int numberOfAttempts);

        bool CreateTable(string tableName, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        bool DeleteTable(string tableName);

        bool DeleteTable(string tableName, int numberOfAttempts);

        bool DeleteTable(string tableName, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        bool InsertEntity(string tableName, string partitionKey, string rowKey, object entity);

        bool InsertEntity(string tableName, string partitionKey, string rowKey, object entity, int numberOfAttempts);

        bool InsertEntity(string tableName, string partitionKey, string rowKey, object entity, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        string GetEntity(string tableName, string partitionKey, string rowKey);

        string GetEntity(string tableName, string partitionKey, string rowKey, int numberOfAttempts);

        string GetEntity(string tableName, string partitionKey, string rowKey, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        string QueryEntities(string tableName, string filter);

        string QueryEntities(string tableName, string filter, int numberOfAttempts);

        string QueryEntities(string tableName, string filter, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        bool ReplaceUpdateEntity(string tableName, string partitionKey, string rowKey, object entity);

        bool ReplaceUpdateEntity(string tableName, string partitionKey, string rowKey, object entity, int numberOfAttempts);

        bool ReplaceUpdateEntity(string tableName, string partitionKey, string rowKey, object entity, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        bool MergeUpdateEntity(string tableName, string partitionKey, string rowKey, object entity);

        bool MergeUpdateEntity(string tableName, string partitionKey, string rowKey, object entity, int numberOfAttempts);

        bool MergeUpdateEntity(string tableName, string partitionKey, string rowKey, object entity, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        bool DeleteEntity(string tableName, string partitionKey, string rowKey);

        bool DeleteEntity(string tableName, string partitionKey, string rowKey, int numberOfAttempts);
    
        bool DeleteEntity(string tableName, string partitionKey, string rowKey, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);
    }
}
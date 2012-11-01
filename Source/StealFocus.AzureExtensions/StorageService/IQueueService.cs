namespace StealFocus.AzureExtensions.StorageService
{
    using System;
    using System.Collections.Generic;

    public interface IQueueService
    {
        Queue[] ListQueues();

        Queue[] ListQueues(int numberOfAttempts);

        Queue[] ListQueues(int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        bool CreateQueue(string queueName);

        bool CreateQueue(string queueName, int numberOfAttempts);

        bool CreateQueue(string queueName, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        bool DeleteQueue(string queueName);

        bool DeleteQueue(string queueName, int numberOfAttempts);

        bool DeleteQueue(string queueName, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        SortedList<string, string> GetQueueMetadata(string queueName);

        SortedList<string, string> GetQueueMetadata(string queueName, int numberOfAttempts);

        SortedList<string, string> GetQueueMetadata(string queueName, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        bool SetQueueMetadata(string queueName, SortedList<string, string> metadataList);

        bool SetQueueMetadata(string queueName, SortedList<string, string> metadataList, int numberOfAttempts);

        bool SetQueueMetadata(string queueName, SortedList<string, string> metadataList, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        bool PutMessage(string queueName, string messageBody);

        bool PutMessage(string queueName, string messageBody, int numberOfAttempts);

        bool PutMessage(string queueName, string messageBody, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        QueueMessage PeekMessage(string queueName);

        QueueMessage PeekMessage(string queueName, int numberOfAttempts);

        QueueMessage PeekMessage(string queueName, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        QueueMessage GetMessage(string queueName);

        QueueMessage GetMessage(string queueName, int numberOfAttempts);

        QueueMessage GetMessage(string queueName, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        bool ClearMessages(string queueName);

        bool ClearMessages(string queueName, int numberOfAttempts);

        bool ClearMessages(string queueName, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        bool DeleteMessage(string queueName, Guid messageId, string popReceipt);

        bool DeleteMessage(string queueName, Guid messageId, string popReceipt, int numberOfAttempts);

        bool DeleteMessage(string queueName, Guid messageId, string popReceipt, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);
    }
}
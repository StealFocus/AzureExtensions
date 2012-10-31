namespace StealFocus.AzureExtensions.Rest
{
    using System.Collections.Generic;

    public interface IBlobStorage
    {
        BlobContainer[] ListContainers();

        BlobContainer[] ListContainers(int numberOfAttempts);

        BlobContainer[] ListContainers(int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        bool CreateContainer(string container);

        bool CreateContainer(string container, int numberOfAttempts);

        bool CreateContainer(string container, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        bool DeleteContainer(string container);

        bool DeleteContainer(string container, int numberOfAttempts);

        bool DeleteContainer(string container, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        SortedList<string, string> GetContainerProperties(string container);

        SortedList<string, string> GetContainerMetadata(string container);

        bool SetContainerMetadata(string container, SortedList<string, string> metadataList);

        string GetContainerAcl(string container);

        bool SetContainerAcl(string container, string containerAccessLevel);
    }
}
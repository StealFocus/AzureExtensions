namespace StealFocus.AzureExtensions.Rest.StorageService
{
    using System.Collections.Generic;

    public interface IBlobStorage
    {
        BlobContainer[] ListContainers();

        BlobContainer[] ListContainers(int numberOfAttempts);

        BlobContainer[] ListContainers(int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        bool CreateContainer(string containerName);

        bool CreateContainer(string containerName, int numberOfAttempts);

        bool CreateContainer(string containerName, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        bool DeleteContainer(string containerName);

        bool DeleteContainer(string containerName, int numberOfAttempts);

        bool DeleteContainer(string containerName, int numberOfAttempts, int timeBetweenAttemptsInMilliseconds);

        SortedList<string, string> GetContainerProperties(string containerName);

        SortedList<string, string> GetContainerMetadata(string containerName);

        bool SetContainerMetadata(string containerName, SortedList<string, string> metadataList);

        ContainerAcl GetContainerAcl(string containerName);

        bool SetContainerAcl(string containerName, ContainerAcl containerAcl);
    }
}
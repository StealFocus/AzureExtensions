namespace StealFocus.AzureExtensions.StorageService
{
    using System.Collections.Generic;

    public interface IBlobService
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

        string[] ListBlobs(string containerName);

        bool PutBlob(string containerName, string blobName, string blobContent);

        /// <summary>
        /// Puts content in a blob.
        /// </summary>
        /// <param name="containerName">A <see cref="string"/>. The container name.</param>
        /// <param name="blobName">A <see cref="string"/>. The blob name.</param>
        /// <param name="pageSize">An <see cref="int"/>. Must be aligned to a 512-byte boundary.</param>
        bool PutPageBlob(string containerName, string blobName, int pageSize);

        string GetBlob(string containerName, string blobName);

        bool PutBlobIfUnchanged(string containerName, string blobName, string content, string expectedETagValue);

        SortedList<string, string> GetBlobMetadata(string containerName, string blobName);

        SortedList<string, string> GetBlobProperties(string containerName, string blobName);

        bool PutBlobAsMD5Hash(string container, string blob, string content);
    }
}
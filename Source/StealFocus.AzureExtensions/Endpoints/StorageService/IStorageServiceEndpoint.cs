namespace StealFocus.AzureExtensions.Endpoints.StorageService
{
    using System;

    internal interface IStorageServiceEndpoint
    {
        Uri Address { get; }

        bool IsTableStorage { get; }
    }
}

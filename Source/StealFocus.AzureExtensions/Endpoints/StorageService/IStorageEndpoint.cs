namespace StealFocus.AzureExtensions.Endpoints.StorageService
{
    using System;

    internal interface IStorageEndpoint
    {
        Uri Address { get; }

        bool IsTableStorage { get; }
    }
}

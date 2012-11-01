namespace StealFocus.AzureExtensions.StorageService.Endpoints
{
    using System;

    internal interface IStorageServiceEndpoint
    {
        Uri Address { get; }

        bool IsTableStorage { get; }
    }
}

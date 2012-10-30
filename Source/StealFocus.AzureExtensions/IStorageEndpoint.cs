namespace StealFocus.AzureExtensions
{
    using System;

    internal interface IStorageEndpoint
    {
        Uri Address { get; }

        bool IsTableStorage { get; }
    }
}

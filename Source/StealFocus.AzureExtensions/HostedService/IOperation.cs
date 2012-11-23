namespace StealFocus.AzureExtensions.HostedService
{
    using System;

    public interface IOperation
    {
        OperationResult StatusCheck(Guid subscriptionId, string certificateThumbprint, string requestId);
    }
}
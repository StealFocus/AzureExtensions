namespace StealFocus.AzureExtensions.HostedService
{
    using System;

    public interface IOperation
    {
        /// <param name="subscriptionId">The Subscription ID.</param>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <param name="requestId">The request ID, returned in the headers of the response to the original request.</param>
        OperationResult StatusCheck(Guid subscriptionId, string certificateThumbprint, string requestId);
    }
}
namespace StealFocus.AzureExtensions
{
    using System;

    public interface ISubscription
    {
        Guid SubscriptionId { get; }

        string CertificateThumbprint { get; }

        string[] ListHostedServices();
    }
}
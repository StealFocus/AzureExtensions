namespace StealFocus.AzureExtensions
{
    using System;
    using System.Globalization;

    using StealFocus.AzureExtensions.Configuration;

    internal class QueueStorageEndpoint : IStorageEndpoint
    {
        private const string QueueStorageAddressFormat = "http://{0}.queue.core.windows.net/";

        private const string QueueStorageEmulatorAddressFormat = "http://127.0.0.1:10001/{0}/";

        private readonly Uri address;

        internal QueueStorageEndpoint(string storageAccountName)
        {
            if (storageAccountName == DevelopmentStorage.AccountName)
            {
                this.address = new Uri(string.Format(CultureInfo.CurrentCulture, QueueStorageEmulatorAddressFormat, storageAccountName));
            }
            else
            {
                this.address = new Uri(string.Format(CultureInfo.CurrentCulture, QueueStorageAddressFormat, storageAccountName));
            }
        }

        public Uri Address
        {
            get
            {
                return this.address;
            }
        }

        public bool IsTableStorage
        {
            get { return false; }
        }
    }
}

namespace StealFocus.AzureExtensions.Endpoints.StorageService
{
    using System;
    using System.Globalization;

    using StealFocus.AzureExtensions.Configuration;

    internal class QueueServiceEndpoint : IStorageServiceEndpoint
    {
        private const string QueueStorageAddressFormat = "{0}://{1}.queue.core.windows.net/";

        private const string QueueStorageEmulatorAddressFormat = "http://127.0.0.1:10001/{0}/";

        private readonly Uri address;

        internal QueueServiceEndpoint(string storageAccountName, bool useHttps)
        {
            if (storageAccountName == DevelopmentStorage.AccountName)
            {
                this.address = new Uri(string.Format(CultureInfo.CurrentCulture, QueueStorageEmulatorAddressFormat, storageAccountName));
            }
            else
            {
                string protocol = useHttps ? "https" : "http";
                this.address = new Uri(string.Format(CultureInfo.CurrentCulture, QueueStorageAddressFormat, protocol, storageAccountName));
            }
        }
        
        internal QueueServiceEndpoint(string storageAccountName) : this(storageAccountName, true)
        {
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

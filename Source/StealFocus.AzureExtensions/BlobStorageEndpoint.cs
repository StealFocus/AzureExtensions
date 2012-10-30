namespace StealFocus.AzureExtensions
{
    using System;
    using System.Globalization;

    using StealFocus.AzureExtensions.Configuration;

    internal class BlobStorageEndpoint : IStorageEndpoint
    {
        private const string BlobStorageAddressFormat = "http://{0}.blob.core.windows.net/";

        private const string BlobStorageEmulatorAddressFormat = "http://127.0.0.1:10000/{0}/";

        private readonly Uri address;

        internal BlobStorageEndpoint(string storageAccountName)
        {
            if (storageAccountName == DevelopmentStorage.AccountName)
            {
                this.address = new Uri(string.Format(CultureInfo.CurrentCulture, BlobStorageEmulatorAddressFormat, storageAccountName));
            }
            else
            {
                this.address = new Uri(string.Format(CultureInfo.CurrentCulture, BlobStorageAddressFormat, storageAccountName));
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

namespace StealFocus.AzureExtensions.StorageService.Endpoints
{
    using System;
    using System.Globalization;

    using StealFocus.AzureExtensions.StorageService.Configuration;

    internal class BlobServiceEndpoint : IStorageServiceEndpoint
    {
        private const string BlobStorageAddressFormat = "{0}://{1}.blob.core.windows.net/";

        private const string BlobStorageEmulatorAddressFormat = "http://127.0.0.1:10000/{0}/";

        private readonly Uri address;

        internal BlobServiceEndpoint(string storageAccountName, bool useHttps)
        {
            if (storageAccountName == DevelopmentStorage.AccountName)
            {
                this.address = new Uri(string.Format(CultureInfo.CurrentCulture, BlobStorageEmulatorAddressFormat, storageAccountName));
            }
            else
            {
                string protocol = useHttps ? "https" : "http";
                this.address = new Uri(string.Format(CultureInfo.CurrentCulture, BlobStorageAddressFormat, protocol, storageAccountName));
            }
        }

        internal BlobServiceEndpoint(string storageAccountName) : this(storageAccountName, true)
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

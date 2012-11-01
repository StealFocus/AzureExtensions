namespace StealFocus.AzureExtensions.StorageService.Endpoints
{
    using System;
    using System.Globalization;

    using StealFocus.AzureExtensions.StorageService.Configuration;

    internal class TableServiceEndpoint : IStorageServiceEndpoint
    {
        private const string TableStorageAddressFormat = "{0}://{1}.table.core.windows.net/";

        private const string TableStorageEmulatorAddressFormat = "http://127.0.0.1:10002/{0}/";

        private readonly Uri address;

        internal TableServiceEndpoint(string storageAccountName, bool useHttps)
        {
            if (storageAccountName == DevelopmentStorage.AccountName)
            {
                this.address = new Uri(string.Format(CultureInfo.CurrentCulture, TableStorageEmulatorAddressFormat, storageAccountName));
            }
            else
            {
                string protocol = useHttps ? "https" : "http";
                this.address = new Uri(string.Format(CultureInfo.CurrentCulture, TableStorageAddressFormat, protocol, storageAccountName));
            }
        }
        
        internal TableServiceEndpoint(string storageAccountName) : this(storageAccountName, true)
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
            get { return true; }
        }
    }
}

namespace StealFocus.AzureExtensions.Endpoints.StorageService
{
    using System;
    using System.Globalization;

    using StealFocus.AzureExtensions.Configuration;

    internal class TableServiceEndpoint : IStorageServiceEndpoint
    {
        private const string TableStorageAddressFormat = "http://{0}.table.core.windows.net/";

        private const string TableStorageEmulatorAddressFormat = "http://127.0.0.1:10002/{0}/";

        private readonly Uri address;

        internal TableServiceEndpoint(string storageAccountName)
        {
            if (storageAccountName == DevelopmentStorage.AccountName)
            {
                this.address = new Uri(string.Format(CultureInfo.CurrentCulture, TableStorageEmulatorAddressFormat, storageAccountName));
            }
            else
            {
                this.address = new Uri(string.Format(CultureInfo.CurrentCulture, TableStorageAddressFormat, storageAccountName));
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
            get { return true; }
        }
    }
}

namespace StealFocus.AzureExtensions.StorageService
{
    using System;

    public class Table
    {
        internal Table(string name, string id, Uri url, DateTime updated)
        {
            this.Name = name;
            this.Id = id;
            this.Url = url;
            this.Updated = updated;
        }

        public string Name { get; private set; }

        public string Id { get; private set; }

        public Uri Url { get; private set; }

        public DateTime Updated { get; private set; }
    }
}

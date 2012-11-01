namespace StealFocus.AzureExtensions.StorageService
{
    using System;

    public class BlobContainer
    {
        internal BlobContainer(string name, Uri url, DateTime lastModified, string etag)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The Name may not be null or empty.", "name");
            }

            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            if (string.IsNullOrEmpty(etag))
            {
                throw new ArgumentException("The ETag may not be null or empty.", "etag");
            }

            this.Name = name;
            this.Url = url;
            this.LastModified = lastModified;
            this.Etag = etag;
        }

        public string Name { get; set; }

        public Uri Url { get; set; }

        public DateTime LastModified { get; set; }

        public string Etag { get; set; }
    }
}

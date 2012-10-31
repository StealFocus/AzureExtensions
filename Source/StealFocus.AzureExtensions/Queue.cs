namespace StealFocus.AzureExtensions
{
    using System;

    public class Queue
    {
        internal Queue(string name, Uri url)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The Name may not be null or empty.", "name");
            }

            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            this.Name = name;
            this.Url = url;
        }
    
        public string Name { get; private set; }

        public Uri Url { get; private set; }
    }
}

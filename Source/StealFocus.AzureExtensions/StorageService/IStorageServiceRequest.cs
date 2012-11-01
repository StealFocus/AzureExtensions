namespace StealFocus.AzureExtensions.StorageService
{
    using System.Collections.Generic;
    using System.Net;

    internal interface IStorageServiceRequest
    {
        HttpWebRequest Create(
            string method,
            string resource,
            string requestBody = null,
            SortedList<string, string> headers = null,
            string ifMatch = "",
            string md5 = "");
    }
}
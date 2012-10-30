namespace StealFocus.AzureExtensions.Rest
{
    using System.Collections.Generic;
    using System.Net;

    internal interface IStorageApiRequest
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
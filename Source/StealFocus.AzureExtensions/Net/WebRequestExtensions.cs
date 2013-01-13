namespace StealFocus.AzureExtensions.Net
{
    using System;
    using System.Net;
    using System.Threading;

    internal static class WebRequestExtensions
    {
        private const int DefaultThrottleTimeInMilliseconds = 1000;

        private static readonly object SyncRoot = new object();

        /// <remarks>
        /// The Windows Azure Management REST API does not allow a rapid succession of requests. Calls via this 
        /// mechanism will throttle the calls.
        /// </remarks>
        public static WebResponse GetResponseThrottled(this WebRequest webRequest)
        {
            return webRequest.GetResponseThrottled(DefaultThrottleTimeInMilliseconds);
        }

        /// <remarks>
        /// The Windows Azure Management REST API does not allow a rapid succession of requests. Calls via this 
        /// mechanism will throttle the calls.
        /// </remarks>
        public static WebResponse GetResponseThrottled(this WebRequest webRequest, int throttleTimeInMilliseconds)
        {
            if (webRequest == null)
            {
                throw new ArgumentNullException("webRequest");
            }

            WebResponse webResponse;
            lock (SyncRoot)
            {
                Thread.Sleep(throttleTimeInMilliseconds);
                webResponse = webRequest.GetResponse();
            }

            return webResponse;
        }
    }
}

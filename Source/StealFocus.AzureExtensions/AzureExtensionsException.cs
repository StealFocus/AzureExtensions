namespace StealFocus.AzureExtensions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// AzureExtensionsException Class.
    /// </summary>
    [Serializable]
    public class AzureExtensionsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureExtensionsException"/> class.
        /// </summary>
        public AzureExtensionsException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureExtensionsException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public AzureExtensionsException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureExtensionsException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner Exception.</param>
        public AzureExtensionsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureExtensionsException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info.</param>
        /// <param name="context">The context.</param>
        protected AzureExtensionsException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
        }
    }
}

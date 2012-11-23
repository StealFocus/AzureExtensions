namespace StealFocus.AzureExtensions
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Linq;

    /// <summary>
    /// AzureExtensionsOperationException Class.
    /// </summary>
    [Serializable]
    public class AzureExtensionsOperationException : AzureExtensionsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureExtensionsOperationException"/> class.
        /// </summary>
        public AzureExtensionsOperationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureExtensionsOperationException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public AzureExtensionsOperationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureExtensionsOperationException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner Exception.</param>
        public AzureExtensionsOperationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureExtensionsOperationException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info.</param>
        /// <param name="context">The context.</param>
        protected AzureExtensionsOperationException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
        }

        public XDocument ResponseBody { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ResponseBody", this.ResponseBody);
        }
    }
}

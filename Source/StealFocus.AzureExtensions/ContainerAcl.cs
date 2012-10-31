namespace StealFocus.AzureExtensions
{
    public class ContainerAcl
    {
        /// <summary>
        /// Creates a new instance of <see cref="ContainerAcl" />.
        /// </summary>
        /// <param name="accessLevel">A <see cref="string"/>. One of 'blob' or 'container', supply <see cref="string.Empty" /> or null for private.</param>
        /// <param name="signedIdentifiersXml">A <see cref="string"/>. Example
        /// <code>
        /// <![CDATA[
        /// <?xml version="1.0" encoding="utf-8"?>
        /// <SignedIdentifiers>
        ///   <SignedIdentifier> 
        ///     <Id>unique-64-character-value</Id>
        ///     <AccessPolicy>
        ///       <Start>YYYY-MM-DDThh:mm:ss.ffffffTZD</Start>
        ///       <Expiry>YYYY-MM-DDThh:mm:ss.ffffffTZD</Expiry>
        ///       <Permission>abbreviated-permission-list</Permission>
        ///     </AccessPolicy>
        ///   </SignedIdentifier>
        /// </SignedIdentifiers>
        /// ]]>
        /// </code>
        /// </param>
        public ContainerAcl(string accessLevel, string signedIdentifiersXml)
        {
            this.AccessLevel = accessLevel;
            this.SignedIdentifiersXml = signedIdentifiersXml;
        }

        public ContainerAcl(string accessLevel) : this(accessLevel, null)
        {
        }

        public string AccessLevel { get; private set; }

        public string SignedIdentifiersXml { get; private set; }
    }
}

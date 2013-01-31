namespace StealFocus.AzureExtensions.HostedService
{
    using System;
    using System.Xml.Linq;

    public interface IDeployment
    {
        /// <param name="subscriptionId">The Subscription ID.</param>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="deploymentSlot">Either "Production" or "Staging".</param>
        bool CheckExists(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot);

        /// <param name="subscriptionId">The Subscription ID.</param>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="deploymentSlot">Either "Production" or "Staging".</param>
        string DeleteRequest(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot);

        /// <param name="subscriptionId">The Subscription ID.</param>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="deploymentSlot">Either "Production" or "Staging".</param>
        /// <param name="deploymentName">Should not contain spaces.</param>
        /// <param name="packageUrl">The URL to the <![CDATA[.cspkg]]> in blob storage.</param>
        /// <param name="label">Limited to 100 characters.</param>
        /// <param name="configurationFilePath">The path to the <![CDATA[.cscfg]]> file.</param>
        /// <param name="startDeployment">Whether to start after deployment.</param>
        /// <param name="treatWarningsAsError">Whether to treat warnings as errors.</param>
        string CreateRequest(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot, string deploymentName, Uri packageUrl, string label, string configurationFilePath, bool startDeployment, bool treatWarningsAsError);

        /// <param name="subscriptionId">The Subscription ID.</param>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="deploymentSlot">Either "Production" or "Staging".</param>
        XDocument GetInformation(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot);

        /// <param name="subscriptionId">The Subscription ID.</param>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="deploymentSlot">Either "Production" or "Staging".</param>
        XDocument GetConfiguration(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot);

        /// <param name="subscriptionId">The Subscription ID.</param>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="deploymentSlot">Either "Production" or "Staging".</param>
        /// <param name="configuration">The XML representing the new configuration i.e. the contents of a <![CDATA[.cscfg]]> file.</param>
        /// <param name="treatWarningsAsError">A <see cref="bool"/>.</param>
        /// <param name="mode">Either "Auto" or "Manual".</param>
        string ChangeConfiguration(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot, XDocument configuration, bool treatWarningsAsError, string mode);

        /// <param name="subscriptionId">The Subscription ID.</param>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="deploymentSlot">Either "Production" or "Staging".</param>
        /// <param name="horizontalScales">Role names and required instance counts.</param>
        /// <param name="treatWarningsAsError">Whether to treat any warnings as errors.</param>
        /// <param name="mode">Auto|Manual</param>
        /// <returns>The ID of the request to update the configuration of the deployment. Null if no update was made (if the deployment was already scaled to the specification).</returns>
        string HorizontallyScale(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot, HorizontalScale[] horizontalScales, bool treatWarningsAsError, string mode);

        /// <param name="subscriptionId">The Subscription ID.</param>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="deploymentSlot">Either "Production" or "Staging".</param>
        /// <param name="roleName">The name of the role.</param>
        string GetInstanceSize(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot, string roleName);

        /// <param name="subscriptionId">The Subscription ID.</param>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="deploymentSlot">Either "Production" or "Staging".</param>
        /// <param name="roleName">The name of the role.</param>
        int GetInstanceCount(Guid subscriptionId, string certificateThumbprint, string serviceName, string deploymentSlot, string roleName);
    }
}

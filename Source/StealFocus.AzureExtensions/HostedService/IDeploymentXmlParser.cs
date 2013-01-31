namespace StealFocus.AzureExtensions.HostedService
{
    internal interface IDeploymentXmlParser
    {
        string GetInstanceSize(string roleName);

        int GetInstanceCount(string roleName);
    }
}
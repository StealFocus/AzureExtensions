namespace StealFocus.AzureExtensions.StorageService.Configuration
{
    /// <summary>
    /// Holds the credentials for the Storage Emulator.
    /// </summary>
    /// <remarks>
    /// These values will be the same on any machine so can just be hard coded.
    /// </remarks>
    internal static class DevelopmentStorage
    {
        internal const string AccountName = "devstoreaccount1";

        internal const string AccountKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";
    }
}

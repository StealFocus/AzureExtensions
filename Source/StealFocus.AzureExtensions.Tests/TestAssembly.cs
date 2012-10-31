namespace StealFocus.AzureExtensions.Tests
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using StealFocus.AzureExtensions.Configuration;
    using StealFocus.AzureExtensions.Tests.Configuration;

    [TestClass]
    public static class TestAssembly
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
        }

        [AssemblyCleanup]
        public static void CleanUp()
        {
        }

        public static void RunEmulatorIfRequired(string storageAccountName)
        {
            if (EmulatorIsRequired(storageAccountName) && !IsStorageEmulatorRunning())
            {
                StartEmulator();
            }
        }

        /// <summary>
        /// Azure (but not the Storage Emulator) throttles requests, this method is used to prevent requests being ignored under a long run of tests.
        /// </summary>
        /// <param name="storageAccountName"></param>
        public static void ThrottleTestMethodsIfRunningUnderAzure(string storageAccountName)
        {
            if (storageAccountName != DevelopmentStorage.AccountName)
            {
                Thread.Sleep(TestThrottling.IntervalBetweenTestMethodsInMilliseconds);
            }
        }

        /// <summary>
        /// Azure (but not the Storage Emulator) throttles requests, this method is used to prevent requests being ignored under a long run of tests.
        /// </summary>
        /// <param name="storageAccountName"></param>
        public static void ThrottleTestClassesIfRunningUnderAzure(string storageAccountName)
        {
            if (storageAccountName != DevelopmentStorage.AccountName)
            {
                Thread.Sleep(TestThrottling.IntervalBetweenTestClassesInMilliseconds);
            }
        }

        private static bool EmulatorIsRequired(string storageAccountName)
        {
            return storageAccountName == DevelopmentStorage.AccountName;
        }

        private static bool IsStorageEmulatorRunning()
        {
            // Find "DSServiceLDB.exe"
            Process[] runningProcessNames = Process.GetProcessesByName("DSServiceLDB");
            if (runningProcessNames.Length > 0)
            {
                return true;
            }

            return false;
        }

        private static void StartEmulator()
        {
            string windowsAzureDesktopExecutionToolPath = Environment.ExpandEnvironmentVariables("%SystemDrive%\\Program Files\\Microsoft SDKs\\Windows Azure\\Emulator\\csrun.exe");
            Process process = Process.Start(windowsAzureDesktopExecutionToolPath, "/devstore:start");
            if (process == null)
            {
                Assert.Fail("Failed to establish a process for the Windows Azure Desktop Execution Tool.");
            }

            bool success = process.WaitForExit(30000);
            if (!success)
            {
                Assert.Fail("The Windows Azure Desktop Execution Tool did not start within the timeout period.");
            }
        }
    }
}

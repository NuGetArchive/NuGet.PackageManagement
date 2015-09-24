using System;
using System.IO;
using NuGet.Versioning;
using Test.Utility;
using Xunit;

namespace NuGet.CommandLine.Test
{
    public class NuGetAddCommandTests
    {
        private static readonly string NupkgFileFormat = "{0}.{1}.nupkg";
        private class TestInfo : IDisposable
        {
            public string NuGetExePath { get; }
            public string SourceParamFolder { get; set; }
            public string RandomNupkgFolder { get { return Path.GetDirectoryName(RandomNupkgFilePath); } }
            public string PackageId { get; }
            public NuGetVersion PackageVersion { get; }
            public FileInfo TestPackage { get; set; }
            public string RandomNupkgFilePath { get { return TestPackage.FullName; } }
            public string WorkingPath { get; }

            public TestInfo()
            {
                NuGetExePath = Util.GetNuGetExePath();
                WorkingPath = TestFilesystemUtility.CreateRandomTestFolder();
                PackageId = "AbCd";
                PackageVersion = new NuGetVersion("1.0.0.0");
            }

            public void Init()
            {
                Init(TestFilesystemUtility.CreateRandomTestFolder());
            }

            public void Init(string sourceParamFolder)
            {
                var randomNupkgFolder = TestFilesystemUtility.CreateRandomTestFolder();
                var testPackage = TestPackages.GetLegacyTestPackage(
                    randomNupkgFolder,
                    PackageId,
                    PackageVersion.ToString());

                Init(sourceParamFolder, testPackage);
            }

            public void Init(FileInfo testPackage)
            {
                Init(TestFilesystemUtility.CreateRandomTestFolder(), testPackage);
            }

            public void Init(string sourceParamFolder, FileInfo testPackage)
            {
                SourceParamFolder = sourceParamFolder;
                TestPackage = testPackage;
            }

            public void Dispose()
            {
                TestFilesystemUtility.DeleteRandomTestFolders(
                    SourceParamFolder,
                    RandomNupkgFolder,
                    WorkingPath);
            }
        }

        private static void VerifyPackageExists(
            string normalizedId,
            string normalizedVersion,
            string packagesDirectory)
        {
            var packageIdDirectory = Path.Combine(packagesDirectory, normalizedId);
            Assert.True(Directory.Exists(packageIdDirectory));

            var packageVersionDirectory = Path.Combine(packageIdDirectory, normalizedVersion);
            Assert.True(Directory.Exists(packageVersionDirectory));

            var nupkgFileName = GetNupkgFileName(normalizedId, normalizedVersion);

            var nupkgFilePath = Path.Combine(packageVersionDirectory, nupkgFileName);
            Assert.True(File.Exists(nupkgFilePath));

            var nupkgSHAFilePath = Path.Combine(packageVersionDirectory, nupkgFileName + ".sha512");
            Assert.True(File.Exists(nupkgSHAFilePath));

            var nuspecFilePath = Path.Combine(packageVersionDirectory, normalizedId + ".nuspec");
            Assert.True(File.Exists(nuspecFilePath));
        }

        private static void VerifyPackageDoesNotExist(
            string normalizedId,
            string normalizedVersion,
            string packagesDirectory)
        {
            var packageIdDirectory = Path.Combine(packagesDirectory, normalizedId);
            Assert.False(Directory.Exists(packageIdDirectory));
        }

        private static string GetNupkgFileName(string normalizedId, string normalizedVersion)
        {
            return string.Format(NupkgFileFormat, normalizedId, normalizedVersion);
        }

        [Fact]
        public void AddCommand_Success_SpecifiedSource()
        {
            // Arrange
            using (var testInfo = new TestInfo())
            {
                testInfo.Init();

                var args = new string[]
                {
                    "add",
                    testInfo.RandomNupkgFilePath,
                    "-Source",
                    testInfo.SourceParamFolder
                };

                // Act
                var result = CommandRunner.Run(
                    testInfo.NuGetExePath,
                    testInfo.WorkingPath,
                    string.Join(" ", args),
                    waitForExit: true);

                // Assert
                Util.VerifyResultSuccess(result);
                VerifyPackageExists(
                    testInfo.PackageId.ToLowerInvariant(),
                    testInfo.PackageVersion.ToNormalizedString(),
                    testInfo.SourceParamFolder);
            }
        }

        [Fact]
        public void AddCommand_Success_SpecifiedRelativeSource()
        {
            // Arrange
            using (var testInfo = new TestInfo())
            {
                var sourceParamFullPath = Path.Combine(testInfo.WorkingPath, "relativePathOfflineFeed");
                testInfo.Init(sourceParamFullPath);

                var args = new string[]
                {
                    "add",
                    testInfo.RandomNupkgFilePath,
                    "-Source",
                    "relativePathOfflineFeed"
                };

                // Act
                var result = CommandRunner.Run(
                    testInfo.NuGetExePath,
                    testInfo.WorkingPath,
                    string.Join(" ", args),
                    waitForExit: true);

                // Assert
                Util.VerifyResultSuccess(result);

                VerifyPackageExists(
                    testInfo.PackageId.ToLowerInvariant(),
                    testInfo.PackageVersion.ToNormalizedString(),
                    testInfo.SourceParamFolder);
            }
        }

        [Fact]
        public void AddCommand_Success_SourceDoesNotExist()
        {
            // Arrange
            using (var testInfo = new TestInfo())
            {
                var currentlyNonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                testInfo.Init(currentlyNonExistentPath);

                var args = new string[]
                {
                    "add",
                    testInfo.RandomNupkgFilePath,
                    "-Source",
                    testInfo.SourceParamFolder
                };

                // Act
                var result = CommandRunner.Run(
                    testInfo.NuGetExePath,
                    testInfo.WorkingPath,
                    string.Join(" ", args),
                    waitForExit: true);

                // Assert
                Util.VerifyResultSuccess(result);

                VerifyPackageExists(
                    testInfo.PackageId.ToLowerInvariant(),
                    testInfo.PackageVersion.ToNormalizedString(),
                    testInfo.SourceParamFolder);
            }
        }

        [Fact]
        public void AddCommand_Fail_HttpSource()
        {
            // Arrange
            using (var testInfo = new TestInfo())
            {
                testInfo.Init("https://api.nuget.org/v3/index.json");

                var args = new string[]
                {
                    "add",
                    testInfo.RandomNupkgFilePath,
                    "-Source",
                    testInfo.SourceParamFolder
                };

                // Act
                var result = CommandRunner.Run(
                    testInfo.NuGetExePath,
                    testInfo.WorkingPath,
                    string.Join(" ", args),
                    waitForExit: true);

                // Assert
                var expectedErrorMessage
                    = string.Format("'{0}' should be a local path or a UNC share path.", testInfo.SourceParamFolder);
                Util.VerifyResultFailure(result, expectedErrorMessage);

                VerifyPackageDoesNotExist(
                    testInfo.PackageId.ToLowerInvariant(),
                    testInfo.PackageVersion.ToNormalizedString(),
                    testInfo.SourceParamFolder);
            }
        }

        [Fact]
        public void AddCommand_Fail_NupkgFileDoesNotExist()
        {
            // Arrange
            using (var testInfo = new TestInfo())
            {
                testInfo.Init();

                var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                var args = new string[]
                {
                    "add",
                    nonExistentPath,
                    "-Source",
                    testInfo.SourceParamFolder
                };

                // Act
                var result = CommandRunner.Run(
                    testInfo.NuGetExePath,
                    testInfo.WorkingPath,
                    string.Join(" ", args),
                    waitForExit: true);

                // Assert
                var expectedErrorMessage
                    = string.Format("'{0}' is not found.", nonExistentPath);

                Util.VerifyResultFailure(result, expectedErrorMessage);

                VerifyPackageDoesNotExist(
                    testInfo.PackageId.ToLowerInvariant(),
                    testInfo.PackageVersion.ToNormalizedString(),
                    testInfo.SourceParamFolder);
            }
        }

        [Fact]
        public void AddCommand_Fail_NupkgFileIsAnHttpLink()
        {
            // Arrange
            using (var testInfo = new TestInfo())
            {
                testInfo.Init();

                var invalidNupkgFilePath = "http://www.nuget.org/api/v2/package/EntityFramework/5.0.0";
                var args = new string[]
                {
                    "add",
                    invalidNupkgFilePath,
                    "-Source",
                    testInfo.SourceParamFolder
                };

                // Act
                var result = CommandRunner.Run(
                    testInfo.NuGetExePath,
                    testInfo.WorkingPath,
                    string.Join(" ", args),
                    waitForExit: true);

                // Assert
                var expectedErrorMessage
                    = string.Format("'{0}' should be a local path or a UNC share path.", invalidNupkgFilePath);

                Util.VerifyResultFailure(result, expectedErrorMessage);

                VerifyPackageDoesNotExist(
                    testInfo.PackageId.ToLowerInvariant(),
                    testInfo.PackageVersion.ToNormalizedString(),
                    testInfo.SourceParamFolder);
            }
        }

        [Fact]
        public void AddCommand_Fail_NupkgPathIsNotANupkg()
        {
            // Arrange
            using (var testInfo = new TestInfo())
            {
                var testPackage = new FileInfo(Path.Combine(Path.GetTempPath(), Path.GetTempFileName()));
                testInfo.Init(testPackage);

                var args = new string[]
                {
                    "add",
                    testInfo.RandomNupkgFilePath,
                    "-Source",
                    testInfo.SourceParamFolder
                };

                // Act
                var result = CommandRunner.Run(
                    testInfo.NuGetExePath,
                    testInfo.WorkingPath,
                    string.Join(" ", args),
                    waitForExit: true);

                // Assert
                var expectedErrorMessage
                    = string.Format("'{0}' is not a valid nupkg file.", testInfo.RandomNupkgFilePath);

                Util.VerifyResultFailure(result, expectedErrorMessage);

                VerifyPackageDoesNotExist(
                    testInfo.PackageId.ToLowerInvariant(),
                    testInfo.PackageVersion.ToNormalizedString(),
                    testInfo.SourceParamFolder);
            }
        }

        [Fact]
        public void AddCommand_Fail_CorruptNupkgFile()
        {
            // Arrange
            using (var testInfo = new TestInfo())
            {
                var testPackage = new FileInfo(Path.Combine(Path.GetTempPath(), Path.GetTempFileName()));
                testInfo.Init(testPackage);

                var args = new string[]
                {
                    "add",
                    testInfo.RandomNupkgFilePath,
                    "-Source",
                    testInfo.SourceParamFolder
                };

                // Act
                var result = CommandRunner.Run(
                    testInfo.NuGetExePath,
                    testInfo.WorkingPath,
                    string.Join(" ", args),
                    waitForExit: true);

                // Assert
                var expectedErrorMessage
                    = string.Format("'{0}' is not a valid nupkg file.", testInfo.RandomNupkgFilePath);

                Util.VerifyResultFailure(result, expectedErrorMessage);

                VerifyPackageDoesNotExist(
                    testInfo.PackageId.ToLowerInvariant(),
                    testInfo.PackageVersion.ToNormalizedString(),
                    testInfo.SourceParamFolder);
            }
        }
    }
}

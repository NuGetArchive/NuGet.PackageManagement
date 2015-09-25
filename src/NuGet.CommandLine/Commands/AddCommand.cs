using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Packaging;

namespace NuGet.CommandLine
{
    [Command(typeof(NuGetCommand), "add", "AddCommandDescription;DefaultConfigDescription",
        MinArgs = 1, MaxArgs = 2, UsageDescriptionResourceName = "AddCommandUsageDescription",
        UsageSummaryResourceName = "AddCommandUsageSummary", UsageExampleResourceName = "AddCommandUsageExamples")]
    public class AddCommand : Command
    {
        [Option(typeof(NuGetCommand), "AddCommandSourceDescription", AltName = "src")]
        public string Source { get; set; }

        public override async Task ExecuteCommandAsync()
        {
            var packagePath = Arguments[0];
            ValidatePath(packagePath);

            if (!File.Exists(packagePath))
            {
                throw new CommandLineException(
                    LocalizedResourceManager.GetString(nameof(NuGetResources.NupkgPath_NotFound)),
                    packagePath);
            }

            Source = GetEffectiveSourceFeedFolder();
            ValidatePath(Source);

            // If the Source Feed Folder does not exist, it will be created.

            using (var packageStream = File.OpenRead(packagePath))
            {
                try
                {
                    var packageReader = new PackageReader(packageStream);
                    var packageIdentity = packageReader.GetIdentity();

                    bool isValidPackage;
                    if (OfflineFeedUtility.PackageExists(packageIdentity, Source, out isValidPackage))
                    {
                        // Package already exists. Verify if it is valid
                        if (isValidPackage)
                        {
                            var message = string.Format(LocalizedResourceManager.GetString(
                                nameof(NuGetResources.AddCommand_PackageAlreadyExists)), packageIdentity, Source);

                            Console.LogInformation(message);
                        }
                        else
                        {
                            var message = string.Format(LocalizedResourceManager.GetString(
                                nameof(NuGetResources.AddCommand_ExistingPackageInvalid)), packageIdentity, Source);

                            throw new CommandLineException(message);
                        }
                    }
                    else
                    {
                        packageStream.Seek(0, SeekOrigin.Begin);
                        var versionFolderPathContext = new VersionFolderPathContext(
                            packageIdentity,
                            Source,
                            Console,
                            fixNuspecIdCasing: false,
                            extractNuspecOnly: true,
                            normalizeFileNames: true);

                        await NuGetPackageUtils.InstallFromSourceAsync(
                            stream => packageStream.CopyToAsync(stream),
                            versionFolderPathContext,
                            token: CancellationToken.None);

                        Console.LogInformation(
                            LocalizedResourceManager.GetString(nameof(NuGetResources.AddCommand_SuccessfullyAdded)));
                    }
                }
                catch (InvalidDataException)
                {
                    throw new CommandLineException(
                        LocalizedResourceManager.GetString(nameof(NuGetResources.NupkgPath_InvalidNupkg)),
                        packagePath);
                }
            }
        }

        private string GetEffectiveSourceFeedFolder()
        {
            if (string.IsNullOrEmpty(Source))
            {
                return SettingsUtility.GetOfflineFeed(Settings);
            }

            return Source;
        }

        private void ValidatePath(string path)
        {
            Uri pathUri;
            if (!Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out pathUri))
            {
                throw new CommandLineException(
                    LocalizedResourceManager.GetString(nameof(NuGetResources.Path_Invalid)),
                    path);
            }

            if (!pathUri.IsAbsoluteUri)
            {
                path = Path.GetFullPath(path);
                pathUri = new Uri(path);
            }

            if (!pathUri.IsFile && !pathUri.IsUnc)
            {
                throw new CommandLineException(
                    LocalizedResourceManager.GetString(nameof(NuGetResources.Path_Invalid_NotFileNotUnc)),
                    path);
            }
        }
    }
}

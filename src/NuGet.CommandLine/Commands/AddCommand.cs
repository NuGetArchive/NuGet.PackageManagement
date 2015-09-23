using NuGet.Packaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
            string packagePath = Arguments[0];

            using (var packageStream = File.OpenRead(packagePath))
            {
                var packageReader = new PackageReader(packageStream);
                var packageIdentity = packageReader.GetIdentity();
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
            }
        }
    }
}

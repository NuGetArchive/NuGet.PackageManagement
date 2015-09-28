using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;

namespace NuGet.CommandLine
{
    [Command(typeof(NuGetCommand), "init", "InitCommandDescription;DefaultConfigDescription",
    MinArgs = 1, MaxArgs = 2, UsageDescriptionResourceName = "InitCommandUsageDescription",
    UsageSummaryResourceName = "InitCommandUsageSummary", UsageExampleResourceName = "InitCommandUsageExamples")]
    public class InitCommand : Command
    {
        public override async Task ExecuteCommandAsync()
        {
            // Arguments[0] will not be null at this point.
            // Because, this command has MinArgs set to 1.
            var source = Arguments[0];

            var destination = Arguments[1];

            var packagePaths = Directory.EnumerateFiles(source, "*.nupkg");
            foreach(var packagePath in packagePaths)
            {
                var offlineFeedAddContext = new OfflineFeedAddContext(
                    packagePath,
                    destination,
                    Console, // IConsole is an ILogger
                    throwIfSourcePackageIsInvalid: false,
                    throwIfPackageExistsAndInvalid: false,
                    throwIfPackageExists: false);

                await OfflineFeedUtility.AddPackageToSource(offlineFeedAddContext, CancellationToken.None);
            }
        }
    }
}

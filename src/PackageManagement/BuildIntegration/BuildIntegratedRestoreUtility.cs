using NuGet.ProjectManagement;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NuGet.PackageManagement
{
    public static class BuildIntegratedRestoreUtility
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public static async Task Restore(string jsonConfigPath, INuGetProjectContext projectContext, CancellationToken token)
        {
            // Limit to only 1 restore at a time
            await _semaphore.WaitAsync(token);

            try
            {
                token.ThrowIfCancellationRequested();

                await RestoreCore(jsonConfigPath, projectContext, token);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static async Task RestoreCore(string jsonConfigPath, INuGetProjectContext projectContext, CancellationToken token)
        {
            FileInfo file = new FileInfo(jsonConfigPath);

            // Call DNU to restore
            Task dnuTask = null;
            string dnuPath = Environment.GetEnvironmentVariable("DNU_CMD_PATH");

            if (String.IsNullOrEmpty(dnuPath) || !dnuPath.EndsWith("dnu.cmd"))
            {
                throw new InvalidOperationException("Set the environment variable DNU_CMD_PATH to dnu.cmd");
            }
            else
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = dnuPath;
                startInfo.Arguments = "restore";
                startInfo.CreateNoWindow = true;
                startInfo.WorkingDirectory = file.Directory.FullName;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;

                var process = new Process();
                process.StartInfo = startInfo;
                process.EnableRaisingEvents = true;

                process.ErrorDataReceived += (o, e) =>
                {
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        projectContext.Log(MessageLevel.Info, "{0}", e.Data);
                    }
                };

                process.OutputDataReceived += (o, e) =>
                {
                    projectContext.Log(MessageLevel.Info, "{0}", e.Data);
                };

                await Task.Run(() =>
                {
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        projectContext.ReportError(Strings.BuildIntegratedPackageRestoreFailed);
                    }
                });
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NuGet.Versioning;

namespace NuGet.ProjectManagement.Projects
{
    /// <summary>
    /// A NuGet integrated MSBuild project.
    /// These projects contain a project.json
    /// </summary>
    public class BuildIntegratedNuGetProject : NuGetProject, INuGetIntegratedProject
    {
        private readonly FileInfo _projectJson;

        public BuildIntegratedNuGetProject(FileInfo projectJson)
        {
            if (projectJson == null)
            {
                throw new ArgumentNullException(nameof(projectJson));
            }

            _projectJson = projectJson;
        }

        public override async Task<IEnumerable<PackageReference>> GetInstalledPackagesAsync(CancellationToken token)
        {
            List<PackageReference> packages = new List<PackageReference>();

            //  Find all dependencies and convert them into packages.config style references
            foreach (var dependency in ProjectJsonUtility.GetDependencies(await GetJson()))
            {
                // Use the minimum version of the range for the identity
                var identity = new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion);

                // Pass the actual version range as the allowed range
                // TODO: PackageReference needs to support this fully
                packages.Add(new PackageReference(identity, null, true, false, false, dependency.VersionRange));
            }

            return packages;
        }

        public override async Task<bool> InstallPackageAsync(PackageIdentity packageIdentity, Stream packageStream, INuGetProjectContext nuGetProjectContext, CancellationToken token)
        {
            var dependency = new PackageDependency(packageIdentity.Id, new VersionRange(packageIdentity.Version));

            var json = await GetJson();

            ProjectJsonUtility.AddDependency(json, dependency);

            await SaveJson(json);

            return true;
        }

        public override async Task<bool> UninstallPackageAsync(PackageIdentity packageIdentity, INuGetProjectContext nuGetProjectContext, CancellationToken token)
        {
            var json = await GetJson();

            ProjectJsonUtility.RemoveDependency(json, packageIdentity.Id);

            await SaveJson(json);

            return true;
        }

        private async Task<JObject> GetJson()
        {
            using (var streamReader = new StreamReader(_projectJson.OpenRead()))
            {
                return JObject.Parse(streamReader.ReadToEnd());
            }
        }

        private async Task SaveJson(JObject json)
        {
            using (var writer = new StreamWriter(_projectJson.FullName, false, Encoding.UTF8))
            {
                writer.Write(json.ToString());
            }
        }
    }
}

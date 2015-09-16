using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.PackageManagement.UI
{
    public class PackageProvidersModel
    {
        public IEnumerable<IPackageProvider> PackageProviders
        {
            get;
            private set;
        }

        public string PackageId
        {
            get; private set;
        }

        public string ProjectName
        {
            get; private set;
        }

        public PackageProvidersModel(
            IEnumerable<IPackageProvider> packageProviders,
            string packageId,
            string projectName)
        {
            PackageProviders = packageProviders;
            PackageId = packageId;
            ProjectName = projectName;
        }
    }
}

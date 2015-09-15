namespace NuGet.PackageManagement
{
    public interface IPackageProvider
    {
        string Name { get; }

        bool PackageExists(string packageId, string projectName);

        void LaunchUI(string packageId, string projectName);
    }
}
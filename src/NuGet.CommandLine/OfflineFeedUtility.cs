using System;
using System.IO;
using System.Security.Cryptography;
using NuGet.Packaging;
using NuGet.Packaging.Core;

namespace NuGet.CommandLine
{
    public static class OfflineFeedUtility
    {
        public static bool PackageExists(
            PackageIdentity packageIdentity,
            string offlineFeed,
            out bool isValidPackage)
        {
            if (packageIdentity == null)
            {
                throw new ArgumentNullException(nameof(packageIdentity));
            }

            if (string.IsNullOrEmpty(offlineFeed))
            {
                throw new ArgumentNullException(nameof(offlineFeed));
            }

            var versionFolderPathResolver = new VersionFolderPathResolver(offlineFeed, normalizePackageId: true);

            var nupkgFilePath
                = versionFolderPathResolver.GetPackageFilePath(packageIdentity.Id, packageIdentity.Version);

            var hashFilePath
                = versionFolderPathResolver.GetHashPath(packageIdentity.Id, packageIdentity.Version);

            var nuspecFilePath
                = versionFolderPathResolver.GetManifestFilePath(packageIdentity.Id, packageIdentity.Version);

            var nupkgFileExists = File.Exists(nupkgFilePath);

            var hashFileExists = File.Exists(hashFilePath);

            var nuspecFileExists = File.Exists(nuspecFilePath);

            if (nupkgFileExists || hashFileExists || nuspecFileExists)
            {
                if (!nupkgFileExists || !hashFileExists || !nuspecFileExists)
                {
                    // One of the necessary files to represent the package in the feed does not exist
                    isValidPackage = false;
                }
                else
                {
                    // All the necessary files to represent the package in the feed are present.
                    // Check if the existing nupkg matches the hash. Otherwise, it is considered invalid.
                    var packageHash = GetHash(nupkgFilePath);
                    var existingHash = File.ReadAllText(hashFilePath);

                    isValidPackage = packageHash.Equals(existingHash, StringComparison.Ordinal);
                }

                return true;
            }

            isValidPackage = false;
            return false;
        }

        private static string GetHash(string nupkgFilePath)
        {
            string packageHash;
            using (var nupkgStream
                = File.Open(nupkgFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sha512 = SHA512.Create())
                {
                    packageHash = Convert.ToBase64String(sha512.ComputeHash(nupkgStream));
                }
            }

            return packageHash;
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol.VisualStudio;
using NuGet.Versioning;

namespace NuGet.PackageManagement.UI
{
    // This is the model class behind the package items in the infinite scroll list.
    // Some of its properties, such as Latest Version, Status, are fetched on-demand in the background.
    public class SearchResultPackageMetadata : INotifyPropertyChanged
    {
        private PackageStatus _status;

        public event PropertyChangedEventHandler PropertyChanged;

        private static string[] _scalingFactor = new string[] {
            "",
            "K", // kilo
            "M", // mega, million
            "G", // giga, billion
            "T"  // tera,
        };

        public string Id { get; set; }

        public string Author { get; set; }

        public NuGetVersion Version { get; set; }

        // installed version of the pacakge.
        public NuGetVersion InstalledVersion { get; set; }

        // When update is available, the latest version
        private NuGetVersion _latestVersion;

        public NuGetVersion LatestVersion
        {
            get
            {
                return _latestVersion;
            }
            set
            {
                if (!VersionEquals(_latestVersion, value))
                {
                    _latestVersion = value;
                    OnPropertyChanged(nameof(LatestVersion));
                }
            }
        }

        private bool VersionEquals(NuGetVersion v1, NuGetVersion v2)
        {
            if (v1 == null && v2 == null)
            {
                return true;
            }

            if ((v1 == null && v2 != null) ||
                (v1 != null && v2 == null))
            {
                return false;
            }

            return v1.Equals(v2, VersionComparison.Default);
        }

        private int? _downloadCount;

        public int? DownloadCount
        {
            get
            {
                return _downloadCount;
            }
            set
            {
                _downloadCount = value;
                if (_downloadCount.HasValue)
                {
                    double v = _downloadCount.Value;
                    int exp = 0;
                    while (v > 1000)
                    {
                        v /= 1000;
                        ++exp;
                    }

                    _downloadCountText = string.Format(
                        CultureInfo.CurrentCulture,
                        "{0:G3}{1}",
                        v,
                        _scalingFactor[exp]);
                }
                else
                {
                    _downloadCountText = string.Empty;
                }

                OnPropertyChanged(nameof(DownloadCountText));
            }
        }

        private string _downloadCountText;

        public string DownloadCountText
        {
            get
            {
                return _downloadCountText;
            }
        }

        public string Summary { get; set; }

        // Indicates whether the background loader has started.
        private bool BackgroundLoaderRun { get; set; }

        public PackageStatus Status
        {
            get
            {
                if (!BackgroundLoaderRun)
                {
                    BackgroundLoaderRun = true;

                    Task.Run(async () =>
                    {
                        var result = await BackgroundLoader.Value;

                        await NuGetUIThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                        Status = result.Status;
                        LatestVersion = result.LatestVersion;
                    });
                }

                return _status;
            }

            private set
            {
                bool refresh = _status != value;
                _status = value;

                if (refresh)
                {
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        private Lazy<Task<BackgroundLoaderResult>> _backgroundLoader;

        internal Lazy<Task<BackgroundLoaderResult>> BackgroundLoader
        {
            get
            {
                return _backgroundLoader;
            }

            set
            {
                if (_backgroundLoader != value)
                {
                    BackgroundLoaderRun = false;
                }

                _backgroundLoader = value;

                OnPropertyChanged(nameof(Status));
            }
        }

        public SearchResultPackageMetadata(SourceRepository source)
        {
            Source = source;
        }

        public SourceRepository Source { get; }

        public Uri IconUrl { get; set; }

        public Lazy<Task<IEnumerable<VersionInfo>>> Versions { get; set; }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, e);
            }
        }

        public override string ToString()
        {
            return Id;
        }
    }

    internal class BackgroundLoaderResult
    {
        public PackageStatus Status { get; set; }
        public NuGetVersion LatestVersion { get; set; }
    }
}
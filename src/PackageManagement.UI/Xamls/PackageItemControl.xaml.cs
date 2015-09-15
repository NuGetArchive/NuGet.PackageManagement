// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Windows.Controls;

namespace NuGet.PackageManagement.UI
{
    /// <summary>
    /// This control is used as list items in the package list. Its DataContext is
    /// SearchResultPackageMetadata.
    /// </summary>
    public partial class PackageItemControl : UserControl
    {
        public PackageItemControl()
        {
            InitializeComponent();

            _packageProviders.PackageProviderClicked += PackageProviders_PackageProviderClicked;
        }

        private void PackageProviders_PackageProviderClicked(object sender, PackageProviderEventArgs e)
        {
            var searchResultPackageMetadata = DataContext as SearchResultPackageMetadata;
            if (searchResultPackageMetadata == null)
            {
                return;
            }

            e.PackageProvider.LaunchUI(searchResultPackageMetadata.Id, searchResultPackageMetadata.ProjectName);
        }
    }
}

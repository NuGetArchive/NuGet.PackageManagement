using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NuGet.PackageManagement.UI
{
    /// <summary>
    /// Interaction logic for PackageProviders.xaml.
    /// DataContext is PackageProvidersModel
    /// </summary>
    /// 
    public partial class PackageProviders : UserControl
    {
        public PackageProviders()
        {
            InitializeComponent();

            this.DataContextChanged += PackageProviders_DataContextChanged;
        }

        public event EventHandler<PackageProviderEventArgs> PackageProviderClicked;

        [SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
        private void PackageProviders_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _textBlock.Inlines.Clear();
            var packageProvidersModel = DataContext as PackageProvidersModel;
            if (packageProvidersModel != null &&
                packageProvidersModel.PackageProviders != null &&
                packageProvidersModel.PackageProviders.Any())
            {
                _textBlock.Inlines.Add(new Run("[Consider using "));
                bool firstProvider = true;
                foreach (var provider in packageProvidersModel.PackageProviders)
                {
                    if (!firstProvider)
                    {
                        _textBlock.Inlines.Add(", or ");
                    }

                    var hyperLink = new Hyperlink(new Run(provider.Name));
                    hyperLink.Tag = provider;
                    hyperLink.Click += HyperLink_Click;
                    hyperLink.ToolTip = string.Format(
                        CultureInfo.CurrentCulture,
                        "Open it with {0}",
                        provider.Name);
                    _textBlock.Inlines.Add(hyperLink);
                    firstProvider = false;
                }

                _textBlock.Inlines.Add(new Run(" instead]"));
            }
        }

        private void HyperLink_Click(object sender, RoutedEventArgs e)
        {
            var hyperLink = sender as Hyperlink;
            if (hyperLink == null)
            {
                return;
            }

            var packageProvidersModel = DataContext as PackageProvidersModel;
            if (PackageProviderClicked != null)
            {
                var packageProvider = hyperLink.Tag as IPackageProvider;
                PackageProviderClicked(this, 
                    new PackageProviderEventArgs(
                        packageProvider,
                        packageProvidersModel.PackageId,
                        packageProvidersModel.ProjectName));
            }            
        }
    }

    public class PackageProviderEventArgs : EventArgs
    {
        public PackageProviderEventArgs(IPackageProvider packageProvider,
            string packageId,
            string projectName)
        {
            PackageProvider = packageProvider;
            PackageId = packageId;
            ProjectName = projectName;
        }

        public IPackageProvider PackageProvider
        {
            get;
            private set;
        }

        public string PackageId
        {
            get;
            private set;
        }

        public string ProjectName
        {
            get;
            private set;
        }
    }
}

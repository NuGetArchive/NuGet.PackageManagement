using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    /// DataContext is IEnumerable of IPackageProvider
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
            var providers = DataContext as IEnumerable<IPackageProvider>;
            _textBlock.Inlines.Clear();
            if (providers != null && providers.Any())
            {
                _textBlock.Inlines.Add(new Run("[Consider using "));
                bool firstProvider = true;
                foreach (var provider in providers)
                {
                    if (!firstProvider)
                    {
                        _textBlock.Inlines.Add(", or ");
                    }

                    var hyperLink = new Hyperlink(new Run(provider.Name));
                    hyperLink.Tag = provider;
                    hyperLink.Click += HyperLink_Click;
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

            if (PackageProviderClicked != null)
            {
                var packageProvider = hyperLink.Tag as IPackageProvider;
                PackageProviderClicked(this, new PackageProviderEventArgs(packageProvider));
            }            
        }
    }

    public class PackageProviderEventArgs : EventArgs
    {
        public PackageProviderEventArgs(IPackageProvider packageProvider)
        {
            PackageProvider = packageProvider;
        }

        public IPackageProvider PackageProvider
        {
            get;
            private set;
        }
    }
}

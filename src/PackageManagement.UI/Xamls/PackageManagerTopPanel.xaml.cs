using System;
using System.Windows;
using System.Windows.Controls;

namespace NuGet.PackageManagement.UI
{
    /// <summary>
    /// The panel which is located at the top of the package manager window.
    /// </summary>
    public partial class PackageManagerTopPanel : UserControl
    {
        private FilterLabel _selectedFilter;

        public PackageManagerTopPanel()
        {
            InitializeComponent();

            _labelBrowse.Selected = true;
            _selectedFilter = _labelBrowse;
        }

        // the control that is used as container for the search box.
        public Border SearchControlParent
        {
            get
            {
                return _searchControlParent;
            }
        }

        public CheckBox CheckboxPrerelease
        {
            get
            {
                return _checkboxPrerelease;
            }
        }

        public ComboBox SourceRepoList
        {
            get
            {
                return _sourceRepoList;
            }
        }

        public ToolTip SourceToolTip
        {
            get
            {
                return _sourceTooltip;
            }
        }

        public Filter Filter
        {
            get
            {
                return _selectedFilter.Filter;
            }
        }


        private void _checkboxPrerelease_Checked(object sender, RoutedEventArgs e)
        {
            // !!!
        }

        private void _checkboxPrerelease_Unchecked(object sender, RoutedEventArgs e)
        {
            // !!!
        }

        private void _sourceRepoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void _settingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsButtonClicked != null)
            {
                SettingsButtonClicked(this, EventArgs.Empty);
            }
        }

        private void FilterLabel_ControlSelected(object sender, EventArgs e)
        {
            if (_selectedFilter != null)
            {
                _selectedFilter.Selected = false;
            }

            _selectedFilter = (FilterLabel)sender;
            if (FilterChanged != null)
            {
                FilterChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler<EventArgs> FilterChanged;
        public event EventHandler<EventArgs> SettingsButtonClicked;
    }
}
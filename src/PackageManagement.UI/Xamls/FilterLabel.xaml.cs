using System;
using System.Collections.Generic;
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
    /// Represents the filter label. E.g. Browse, Installed, Update Available.
    /// </summary>
    public partial class FilterLabel : UserControl
    {
        // !!! need to draw the number of available updates

        public FilterLabel()
        {
            InitializeComponent();
        }

        public event EventHandler<EventArgs> ControlSelected;

        public Filter Filter
        {
            get;
            set;
        }
        public object Text
        {
            get
            {
                return _textBlock.Content;
            }
            set
            {
                _textBlock.Content = value;
            }
        }

        private bool _selected;

        public bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                _selected = value;
                if (_selected)
                {
                    _textBlock.Foreground = System.Windows.Media.Brushes.Blue;
                    _underline.Visibility = Visibility.Visible;

                    if (ControlSelected != null)
                    {
                        ControlSelected(this, EventArgs.Empty);
                    }
                }
                else
                {
                    _textBlock.Foreground = this.Foreground;
                    _underline.Visibility = Visibility.Hidden;
                }
            }
        }        

        private void _textBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            _textBlock.Foreground = System.Windows.Media.Brushes.Blue;
        }

        private void _textBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!_selected)
            {
                _textBlock.Foreground = this.Foreground;
            }
        }

        private void _textBlock_Click(object sender, RoutedEventArgs e)
        {
            if (_selected)
            {
                // already selected. Do nothing
                return;
            }
            else
            {
                Selected = true;
            }
        }
    }
}

using HLACaptionCompiler.Steam;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace HLACaptionCompiler
{
    /// <summary>
    /// Interaction logic for AddOnSelector.xaml
    /// </summary>
    public partial class AddOnSelector : Window
    {
        public static string GetAddOn()
        {
            string retVal = null;
            AddOnSelector win = new AddOnSelector();
            if (win.ShowDialog() == true)
            {
                retVal = win.SelectedAddOn;
            }
            return retVal;
        }
        public AddOnSelector()
        {
            LoadAddonList();
            InitializeComponent();
        }
        void LoadAddonList()
        {
            AddOns = new ObservableCollection<string>(SteamData.GetAddOnList());
        }
        public static readonly DependencyProperty AddOnsProperty =
            DependencyProperty.Register(nameof(AddOns), typeof(ObservableCollection<string>),
            typeof(AddOnSelector));
        public ObservableCollection<string> AddOns
        {
            get
            {
                return (ObservableCollection<string>)GetValue(AddOnsProperty);
            }
            set
            {
                this.SetValue(AddOnsProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedAddOnProperty =
            DependencyProperty.Register(nameof(SelectedAddOn), typeof(string),
            typeof(AddOnSelector));
        public string SelectedAddOn
        {
            get
            {
                return (string)GetValue(SelectedAddOnProperty);
            }
            set
            {
                this.SetValue(SelectedAddOnProperty, value);
            }
        }


        private void OnOK(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}

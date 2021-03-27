using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HLACaptionCompiler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnCrash(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //Put code here to handle crash
            MessageBox.Show("The application is crashing.  Please contact the developer with the following information:\r\n" + e.Exception.ToString());
        }
    }
}

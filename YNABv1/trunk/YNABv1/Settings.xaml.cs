using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace YNABv1
{
    public partial class Settings : PhoneApplicationPage
    {
        private static readonly IsolatedStorageSettings AppSettings = IsolatedStorageSettings.ApplicationSettings;

        public Settings()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!AppSettings.Contains(Constants.SYNC_KEY)) {
                AppSettings[Constants.SYNC_KEY] = true;
                AppSettings.Save();
            }
            SyncSwitch.IsChecked = (bool)AppSettings[Constants.SYNC_KEY];
        }

        private void SyncSwitch_Checked(object sender, EventArgs e)
        {
            AppSettings[Constants.SYNC_KEY] = SyncSwitch.IsChecked.Value;
            AppSettings.Save();
        }
    }
}
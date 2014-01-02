using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using YNABv1.Resources;
using YNABv1.Model;
using YNABv1.Helpers;
using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox;
using System.IO.IsolatedStorage;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace YNABv1
{
    public partial class MainPage : PhoneApplicationPage
    {
        private static readonly IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (DataContext == null) {
                InitializePageState();
            }
        }

        private void InitializePageState()
        {
            Datastore.TransactionsUpdated += Transactions_Updated;
            DataContext = Datastore.Transactions;
        }

        private void AddTransactionButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("//AddTransaction.xaml", UriKind.Relative));
        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            if (!DropboxHelper.IsSetup()) {
                DropboxHelper.Setup(this, false);
            } else {

            }

        }

        public void ExportButton_Click(object sender, EventArgs e)
        {
            ProgressBar.IsEnabled = true;
            ProgressBar.IsIndeterminate = true;
            ProgressBar.Visibility = Visibility.Visible;
            if (!DropboxHelper.IsSetup()) {
                DropboxHelper.Setup(this, true);
            } else {
                Dictionary<String, String> csvStrings = ExportHelper.GetCsvStrings();
                foreach (KeyValuePair<String, String> pair in csvStrings) {
                    String path = "YNABcompanion";
                    DropboxHelper.ExportTextFile(path, pair.Key + ".csv", pair.Value, delegate { ExportCompleted(); });
                }
            }
        }

        public void ExportCompleted()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => {
                Datastore.ClearAllTransactions();
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Visibility = System.Windows.Visibility.Collapsed;
                ProgressBar.IsEnabled = false;
                MessageBox.Show("Export complete to the YNABcompanion folder!");
            });
        }

        private void Transactions_Updated(object sender, EventArgs e)
        {
            DataContext = Datastore.Transactions;
        }
        
        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem item =
                TransactionListBox.ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
            Transaction t = item.DataContext as Transaction;
            PhoneApplicationService.Current.State["AddTransactionParam"] = t;
            NavigationService.Navigate(new Uri("//AddTransaction.xaml", UriKind.Relative));
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem item = 
                TransactionListBox.ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
            Transaction t = item.DataContext as Transaction;
            SaveResult result = Datastore.DeleteTransaction(t, delegate { MessageBox.Show(Constants.DELETE_MSG); });
            if (result.SaveSuccessful)
                Transactions_Updated(sender, e);
            else {
                string errorMessages = 
                    String.Join(Environment.NewLine + Environment.NewLine, result.ErrorMessages.ToArray());
                if (!String.IsNullOrEmpty(errorMessages))
                    MessageBox.Show(errorMessages, "Warning: Invalid Values", MessageBoxButton.OK);
            }
        }
    }
}
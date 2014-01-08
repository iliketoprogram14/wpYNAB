using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.IO.IsolatedStorage;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using YNABv1.Resources;
using YNABv1.Model;
using YNABv1.Helpers;
using System.Threading;

namespace YNABv1
{
    public partial class MainPage : PhoneApplicationPage
    {
        private static readonly IsolatedStorageSettings AppSettings = IsolatedStorageSettings.ApplicationSettings;

        /// <summary>
        /// 
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
        }

        private void InitializePageState()
        {
            Datastore.TransactionsUpdated += Transactions_Updated;
            Datastore.Transactions.SortByDate();
            DataContext = Datastore.Transactions;

            if (AppSettings.Contains(Constants.TUTORIAL_KEY)) {
                DefaultPivot.Visibility = Visibility.Visible;
                this.ApplicationBar.IsVisible = true;
                TutorialCanvas.Visibility = Visibility.Collapsed;
            } else {
                DefaultPivot.Visibility = Visibility.Collapsed;
                this.ApplicationBar.IsVisible = false;
                TutorialCanvas.Visibility = Visibility.Visible;
            }
        }

        #region Navigation Events
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (DataContext == null)
                InitializePageState();
            if (TransactionListBox.Items.Count > 0) {
                UpdateLayout();
                TransactionListBox.ScrollIntoView(TransactionListBox.Items.First());
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (MainBrowser.Visibility == Visibility.Visible) {
                MainBrowser.Visibility = Visibility.Collapsed;
                DefaultPivot.Visibility = Visibility.Visible;
                ApplicationBar.IsVisible = true;
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Visibility = Visibility.Collapsed;
                e.Cancel = true;
            }
        }
        #endregion

        #region Callbacks/Other Events
        /// <summary>
        /// Callback when all exports have completed
        /// </summary>
        private void ExportCompleted()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => {
                Datastore.ClearAllTransactions();
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Visibility = Visibility.Collapsed;
                MessageBox.Show("Export(s) complete to the YNABcompanion folder!");
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Transactions_Updated(object sender, EventArgs e)
        {
            Datastore.Transactions.SortByDate();
            DataContext = Datastore.Transactions;
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Returns a dictionary of the csv string equivalents of all the transactions
        /// </summary>
        /// <returns></returns>
        private Dictionary<String, String> GetCsvStrings()
        {
            Dictionary<String, String> dict = new Dictionary<String, String>();
            ObservableCollection<Transaction> transactions = Datastore.Transactions.TransactionHistory;
            List<Transaction> transList = transactions.ToList();

            foreach (Transaction t in transList)
                dict[t.Account] = (dict.ContainsKey(t.Account)) ?
                    dict[t.Account] + t.GetCsv() :
                    "Date,Payee,Category,Memo,Outflow,Inflow\n" + t.GetCsv();
            return dict;
        }
        #endregion

        #region UI events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddTransactionButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("//AddTransaction.xaml", UriKind.Relative));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddTransferButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("//AddTransfer.xaml", UriKind.Relative));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportButton_Click(object sender, EventArgs e)
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;
            if (!DropboxHelper.IsSetup()) {
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.IsIndeterminate = true;
                DropboxHelper.Setup(this, delegate { ImportButton_Click(sender, e); });
            } else
                NavigationService.Navigate(new Uri("//FileExplorer.xaml", UriKind.Relative));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (Datastore.Transactions.TransactionHistory.Count == 0) {
                MessageBox.Show("There are no transactions to export");
                return;
            }

            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;

            if (!DropboxHelper.IsSetup())
                DropboxHelper.Setup(this, delegate { ExportButton_Click(sender, e); });
            else {
                ThreadPool.QueueUserWorkItem(async context => {
                    int i = 0;
                    Dictionary<String, String> csvStrings = GetCsvStrings();
                    foreach (KeyValuePair<String, String> pair in csvStrings) {
                        bool success = await DropboxHelper.ExportTextFile(
                            "YNABcompanion", pair.Key + ".csv", pair.Value,
                            delegate { ExportCompleted(); }, ++i == csvStrings.Count);
                    }
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem item = TransactionListBox.ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
            Transaction t = item.DataContext as Transaction;
            if (t.Transfer) {
                PhoneApplicationService.Current.State[Constants.NAV_PARAM_TRANSFER] = t;
                NavigationService.Navigate(new Uri("//AddTransfer.xaml", UriKind.Relative));
            } else {
                PhoneApplicationService.Current.State[Constants.NAV_PARAM_TRANSACTION] = t;
                NavigationService.Navigate(new Uri("//AddTransaction.xaml", UriKind.Relative));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem item = TransactionListBox.ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
            Transaction t = item.DataContext as Transaction;
            SaveResult result = Datastore.DeleteTransaction(t, delegate { MessageBox.Show(Constants.MSG_DELETE); });
            if (result.SaveSuccessful)
                Transactions_Updated(sender, e);
            else {
                string errorMessages = String.Join(Environment.NewLine + Environment.NewLine, result.ErrorMessages.ToArray());
                if (!String.IsNullOrEmpty(errorMessages))
                    MessageBox.Show(errorMessages, "Warning: Invalid Values", MessageBoxButton.OK);
                return;
            }

            if (t.Transfer) { 
                Transaction inverseTransfer = t.InverseTransfer();
                result = Datastore.DeleteTransaction(inverseTransfer, delegate { MessageBox.Show(Constants.MSG_DELETE); });
                if (result.SaveSuccessful)
                    Transactions_Updated(sender, e);
                else {
                    string errorMessages = String.Join(Environment.NewLine + Environment.NewLine, result.ErrorMessages.ToArray());
                    if (!String.IsNullOrEmpty(errorMessages))
                        MessageBox.Show(errorMessages, "Warning: Invalid Values", MessageBoxButton.OK);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TutorialButton_Click(object sender, RoutedEventArgs e)
        {
            TutorialCanvas.Visibility = Visibility.Collapsed;
            DefaultPivot.Visibility = Visibility.Visible;
            this.ApplicationBar.IsVisible = true;

            AppSettings[Constants.TUTORIAL_KEY] = true;
            AppSettings.Save();
        }

        private void Help_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("//Help.xaml", UriKind.Relative));
        }
        #endregion
    }
}
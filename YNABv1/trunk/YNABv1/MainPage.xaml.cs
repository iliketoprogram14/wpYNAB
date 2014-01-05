﻿using System;
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
using System.IO.IsolatedStorage;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace YNABv1
{
    public partial class MainPage : PhoneApplicationPage
    {
        private static readonly IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;

        public MainPage()
        {
            InitializeComponent();
        }

        private void InitializePageState()
        {
            Datastore.TransactionsUpdated += Transactions_Updated;
            DataContext = Datastore.Transactions;
        }

        #region Navigation Events
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (DataContext == null)
                InitializePageState();
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
                ProgressBar.Visibility = Visibility.Collapsed;
                ProgressBar.IsIndeterminate = false;
                ProgressBar.IsEnabled = false;
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
            if (!DropboxHelper.IsSetup())
                DropboxHelper.Setup(this, delegate { ImportButton_Click(sender, e); });
            else
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

            ProgressBar.IsEnabled = true;
            ProgressBar.IsIndeterminate = true;
            ProgressBar.Visibility = Visibility.Visible;

            if (!DropboxHelper.IsSetup())
                DropboxHelper.Setup(this, delegate { ExportButton_Click(sender, e); });
            else {
                int i = 0;
                Dictionary<String, String> csvStrings = GetCsvStrings();
                foreach (KeyValuePair<String, String> pair in csvStrings)
                    DropboxHelper.ExportTextFile(
                        "YNABcompanion", pair.Key + ".csv", pair.Value, 
                        delegate { ExportCompleted(); }, ++i == csvStrings.Count);
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
        #endregion
    }
}
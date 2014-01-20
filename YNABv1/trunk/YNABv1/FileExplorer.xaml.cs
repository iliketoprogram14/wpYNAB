﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using YNABv1.Model;
using YNABv1.Helpers;
using Newtonsoft.Json;
using System.IO.IsolatedStorage;

namespace YNABv1
{
    public partial class FileExplorer : PhoneApplicationPage
    {
        private IsolatedStorageSettings AppSettings = IsolatedStorageSettings.ApplicationSettings;

        /// <summary>
        /// 
        /// </summary>
        public FileExplorer()
        {
            InitializeComponent();
        }

        #region Navigation Events
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            FillPage();
        }
        #endregion

        #region Helpers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        private async void FillPage(String path="")
        {
            if (DropboxListBox.Items.Count > 0)
                DropboxListBox.ScrollIntoView(DropboxListBox.Items[0]);

            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;

            String jsonMetadata = await DropboxHelper.GetMetaData(path);
            Metadata meta = new Metadata(jsonMetadata);
            DataContext = meta;

            ParentFolderTextBox.Visibility = (meta.Path != "/") ? Visibility.Visible : Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        private async void ImportCsvAndPopulateDataStructures(String path)
        {
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;

            String csvString = await DropboxHelper.ImportTextFile(path);
            if (csvString != "")
                Datastore.Parse(csvString);
            else
                MessageBox.Show("Import failed. The csv file may not be valid. Please export the budget and/or register again.");

            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;
            NavigationService.GoBack();
        }

        private void ImportBudget(String path)
        {
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;

            Ynab4Helper.ImportBudget(path);

            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;
            NavigationService.GoBack();
        }
        #endregion

        #region UI Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Metadata m = DropboxListBox.SelectedItem as Metadata;
            if (m != null) {
                if (m.IsDir) {
                    if ((bool)AppSettings[Constants.SYNC_KEY] && m.Name.Contains(".ynab4"))
                        ImportBudget(m.Path);
                    else
                        FillPage(m.Path);
                } else if (m.Name.Contains(".csv")) {
                    ImportCsvAndPopulateDataStructures(m.Path);
                } else {
                    if ((bool)AppSettings[Constants.SYNC_KEY])
                        MessageBox.Show("Please select a .ynab4 file.");
                    else
                        MessageBox.Show("Please select a CSV file.");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Parent_Tapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Metadata m = DataContext as Metadata;
            string[] parts = m.Path.Split('/');
            String parent = "";
            for (int i = 0; i < parts.Length - 1; i++)
                parent += "/" + parts[i];
            FillPage(parent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }
        #endregion
    }
}
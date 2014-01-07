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

namespace YNABv1
{
    public partial class FileExplorer : PhoneApplicationPage
    {
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

            ProgressBar.IsIndeterminate = true;

            String jsonMetadata = await DropboxHelper.GetMetaData(path);
            Metadata meta = new Metadata(jsonMetadata);
            DataContext = meta;

            ParentFolderTextBox.Visibility = (meta.Path != "/") ? Visibility.Visible : Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        private async void ImportCsvAndPopulateDataStructures(String path)
        {
            ProgressBar.IsIndeterminate = true;
            String csvString = await DropboxHelper.ImportTextFile(path);
            if (csvString != "")
                Datastore.Parse(csvString);
            else
                MessageBox.Show("Import failed.  Please close the app and try again in a bit.");
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
                if (m.IsDir)
                    FillPage(m.Path);
                else if (m.Name.Contains("csv"))
                    ImportCsvAndPopulateDataStructures(m.Path);
                else
                    MessageBox.Show("Please select a CSV file.");
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
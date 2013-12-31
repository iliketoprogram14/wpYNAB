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

        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (!appSettings.Contains(Constants.DROPBOX_ACCESS_TOKEN)) {
                string url = "https://www.dropbox.com/1/oauth2/authorize?response_type=code&client_id=jybzacqc9ldijvb";
                MainBrowser.Visibility = System.Windows.Visibility.Visible;
                MainBrowser.IsScriptEnabled = true;
                DefaultPivot.Visibility = System.Windows.Visibility.Collapsed;
                MainBrowser.Navigate(new Uri(url, UriKind.Absolute));
            } else {
                // do the export
            }
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

        private void MainBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            string html = MainBrowser.SaveToString();
            if (html.Contains("Enter this code into")) {
                // THIS IS HARD-CODED SO BEWARE
                string[] parts = html.Split(new string[] { "auth-code" }, StringSplitOptions.None);
                string code = parts[1].Split('<')[0].Trim('\\').Trim('"').Trim('>');
                GetAccessToken(code);
                MainBrowser.Visibility = System.Windows.Visibility.Collapsed;
                DefaultPivot.Visibility = Visibility.Visible;
            }
        }

        private void GetAccessToken(string code)
        {
            var request = (WebRequest)WebRequest.Create(new Uri("https://api.dropbox.com/1/oauth2/token"));
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.BeginGetRequestStream(new AsyncCallback(r => {
                var request2 = (HttpWebRequest)r.AsyncState;
                Stream postStream = request2.EndGetRequestStream(r);

                // Create the post data
                string postData = "code=" + code + "&grant_type=authorization_code&client_id=" + 
                    Constants.DROPBOX_KEY + "&client_secret=" + Constants.DROPBOX_SECRET;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                // Add the post data to the web request
                postStream.Write(byteArray, 0, byteArray.Length);
                postStream.Close();

                request2.BeginGetResponse(new AsyncCallback(r2 => {
                    var request3 = (HttpWebRequest)r2.AsyncState;
                    HttpWebResponse response = (HttpWebResponse)request3.EndGetResponse(r2);
                    using (StreamReader httpwebStreamReader = new StreamReader(response.GetResponseStream())) {
                        string result = httpwebStreamReader.ReadToEnd();
                        Debug.WriteLine(result);
                        result = result.Trim('}').Trim('{').Replace(" ", "");
                        string[] parts = result.Split(new char[] { ':', ',' }, StringSplitOptions.None);
                        string accessToken = parts[1].Trim('\"'); // parts[0] == "access_token"
                        string tokenType = parts[3].Trim('\"'); // parts[2] == "token_type"
                        string userID = parts[5].Trim('\"'); // parts[4] == "uid"
                        appSettings[Constants.DROPBOX_ACCESS_TOKEN] = accessToken;
                        appSettings[Constants.DROPBOX_UID] = userID;
                        Debug.WriteLine("OMG\n");
                        /**** DO THE EXPORT HERE OR SOMETHING ****/
                    }
                }), request2);
            }), request);
        }
    }
}
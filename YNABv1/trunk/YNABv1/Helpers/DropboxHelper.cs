using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace YNABv1.Helpers
{
    public static class DropboxHelper
    {
        private static readonly IsolatedStorageSettings appSettings = 
            IsolatedStorageSettings.ApplicationSettings;
        private static MainPage mainPage;
        private static bool exportAfterCompletedSetup = false;

        public static bool IsSetup()
        {
            bool herp = appSettings.Contains(Constants.DROPBOX_ACCESS_TOKEN);
            return appSettings.Contains(Constants.DROPBOX_ACCESS_TOKEN);
        }

        public static void Setup(MainPage p, bool export)
        {
            string url = "https://www.dropbox.com/1/oauth2/authorize?response_type=code&client_id=jybzacqc9ldijvb";
            p.MainBrowser.Visibility = System.Windows.Visibility.Visible;
            p.MainBrowser.IsScriptEnabled = true;
            p.DefaultPivot.Visibility = System.Windows.Visibility.Collapsed;
            p.MainBrowser.Navigated += DropboxMainBrowser_Navigated;
            p.MainBrowser.Navigate(new Uri(url, UriKind.Absolute));
            mainPage = p;
            exportAfterCompletedSetup = export;
        }

        public async static void ExportTextFile(String path, String filename, String data, Action callback, bool last)
        {
            String url = "https://api-content.dropbox.com/1/files_put/dropbox/" + path + "/" + filename;
            url += "?overwrite=false";

            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Put, new Uri(url));
            request.Headers.Add("Authorization", "Bearer " + appSettings[Constants.DROPBOX_ACCESS_TOKEN]);
            request.Content = new StringContent(data);
            var response = await client.SendAsync(request);

            String result = response.Content.ReadAsStringAsync().Result;
            Debug.WriteLine(result);
            if (last)
                callback();
        }

        private static void DropboxMainBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            string html = mainPage.MainBrowser.SaveToString();
            if (html.Contains("Enter this code into")) {
                // THIS IS HARD-CODED SO BEWARE
                string[] parts = html.Split(new string[] { "auth-code" }, StringSplitOptions.None);
                string code = parts[1].Split('<')[0].Trim('\\').Trim('"').Trim('>');
                GetAccessToken(code);
                mainPage.MainBrowser.Visibility = System.Windows.Visibility.Collapsed;
                mainPage.DefaultPivot.Visibility = System.Windows.Visibility.Visible;}
        }

        private async static Task GetAccessToken(string code)
        {
            string postData = "code=" + code + "&grant_type=authorization_code&client_id=" +
                Constants.DROPBOX_KEY + "&client_secret=" + Constants.DROPBOX_SECRET;
            
            HttpClient client = new HttpClient();
            HttpContent content = new StringContent(postData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var response = await client.PostAsync(new Uri("https://api.dropbox.com/1/oauth2/token"), content);

            String result = await response.Content.ReadAsStringAsync();
            result = result.Trim('}').Trim('{').Replace(" ", "");
            string[] parts = result.Split(new char[] { ':', ',' }, StringSplitOptions.None);
            string accessToken = parts[1].Trim('\"'); // parts[0] == "access_token"
            string tokenType = parts[3].Trim('\"'); // parts[2] == "token_type"
            string userID = parts[5].Trim('\"'); // parts[4] == "uid"

            appSettings[Constants.DROPBOX_ACCESS_TOKEN] = accessToken;
            appSettings[Constants.DROPBOX_UID] = userID;
            appSettings.Save();

            if (exportAfterCompletedSetup) {
                exportAfterCompletedSetup = false;
                mainPage.ExportButton_Click(null, new RoutedEventArgs());
            }
        }

        public async static Task<String> GetMetaData(string path)
        {
            String url = "https://api.dropbox.com/1/metadata/dropbox/" + path;
            HttpClient c = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            request.Headers.Add("Authorization", "Bearer " + appSettings[Constants.DROPBOX_ACCESS_TOKEN]);
            var response = await c.SendAsync(request);
            String result = response.Content.ReadAsStringAsync().Result;
            return result;
        }
    }
}

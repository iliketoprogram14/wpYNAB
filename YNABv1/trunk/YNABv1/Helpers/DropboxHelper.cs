using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
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

        public static void ExportTextFile(String path, String filename, String data, Action callback, bool last)
        {
            String url = "https://api-content.dropbox.com/1/files_put/dropbox/" + path + "/" + filename;
            url += "?overwrite=false";
            var uri = new Uri(url);

            var request = (WebRequest)WebRequest.Create(new Uri(url));
            request.Method = "PUT";
            request.ContentLength = Encoding.UTF8.GetByteCount(data);
            request.Headers["Authorization"] = "Bearer " + appSettings[Constants.DROPBOX_ACCESS_TOKEN];

            request.BeginGetRequestStream(new AsyncCallback(r => {
                var request2 = (HttpWebRequest)r.AsyncState;
                Stream postStream = request2.EndGetRequestStream(r);

                // Create the post data
                string postData = data;
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
                        if (last)
                            callback();
                    }
                }), request2);

            }), request);

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

        private static void GetAccessToken(string code)
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
                        appSettings.Save();                        

                        if (exportAfterCompletedSetup) {
                            exportAfterCompletedSetup = false;
                            mainPage.ExportButton_Click(null, new RoutedEventArgs());
                        }
                    }
                }), request2);
            }), request);
        }

    }
}

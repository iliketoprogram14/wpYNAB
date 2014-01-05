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
using Newtonsoft.Json.Linq;

namespace YNABv1.Helpers
{
    public static class DropboxHelper
    {
        private static readonly IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;
        private static MainPage mainPage;
        private static Action callbackAfterSetup = null;

        #region Public Interface
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool IsSetup()
        {
            return appSettings.Contains(Constants.DROPBOX_ACCESS_TOKEN);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="callback"></param>
        public static void Setup(MainPage p, Action callback)
        {
            if (IsSetup())
                return;

            string url = "https://www.dropbox.com/1/oauth2/authorize?response_type=code&client_id=jybzacqc9ldijvb";
            p.DefaultPivot.Visibility = System.Windows.Visibility.Collapsed;
            /****** Later on, create mainbrowser in here and add it to mainPage */
            p.MainBrowser.Visibility = System.Windows.Visibility.Visible;
            p.MainBrowser.IsScriptEnabled = true;
            p.MainBrowser.Navigated += DropboxMainBrowser_Navigated;
            p.MainBrowser.Navigate(new Uri(url, UriKind.Absolute));
            mainPage = p;
            callbackAfterSetup = callback;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async static Task<String> ImportTextFile(String path)
        {
            String url = "https://api-content.dropbox.com/1/files/dropbox/" + path;
            String result = await MakeRequest(url, HttpMethod.Get, true);            
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        /// <param name="last"></param>
        public async static void ExportTextFile(String path, String filename, String data, Action callback, bool last)
        {
            String url = "https://api-content.dropbox.com/1/files_put/dropbox/" + path + "/" + filename + "?overwrite=false";
            await MakeRequest(url, HttpMethod.Put, true, data);
            if (last)
                callback();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async static Task<String> GetMetaData(string path)
        {
            String url = "https://api.dropbox.com/1/metadata/dropbox/" + path;
            String result = await MakeRequest(url, HttpMethod.Get, true);
            return result;
        }
        #endregion

        #region Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void DropboxMainBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            string html = mainPage.MainBrowser.SaveToString();
            if (html.Contains("Enter this code into")) {
                // THIS IS HARD-CODED SO BEWARE
                string[] parts = html.Split(new string[] { "auth-code" }, StringSplitOptions.None);
                string code = parts[1].Split('<')[0].Trim('\\').Trim('"').Trim('>');

                mainPage.MainBrowser.Visibility = System.Windows.Visibility.Collapsed;
                mainPage.DefaultPivot.Visibility = System.Windows.Visibility.Visible;

                GetAccessToken(code);
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private async static Task GetAccessToken(string code)
        {
            String url = "https://api.dropbox.com/1/oauth2/token";
            string postData = "code=" + code + "&grant_type=authorization_code&client_id=" +
                Constants.DROPBOX_KEY + "&client_secret=" + Constants.DROPBOX_SECRET;

            String result = await MakeRequest(url, HttpMethod.Post, false, postData, true);
            JObject obj = JObject.Parse(result);
            string accessToken = (string)obj["access_token"];
            string tokenType = (string)obj["token_type"];
            string userID = (string)obj["uid"];

            appSettings[Constants.DROPBOX_ACCESS_TOKEN] = accessToken;
            appSettings[Constants.DROPBOX_UID] = userID;
            appSettings.Save();

            Action callback = callbackAfterSetup;
            callbackAfterSetup = null;
            callback();
        }

        /// <summary>
        /// Generalized method for making an authorized request.  
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private async static Task<String> MakeRequest(String url, HttpMethod method, bool authorized, String content="", bool contentTypeUrl=false)
        {
            HttpClient c = new HttpClient();
            var request = new HttpRequestMessage(method, new Uri(url));
            if (authorized)
                request.Headers.Add("Authorization", "Bearer " + appSettings[Constants.DROPBOX_ACCESS_TOKEN]);
            if (content != "")
                request.Content = new StringContent(content);
            if (contentTypeUrl == true)
                request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = await c.SendAsync(request);
            return response.Content.ReadAsStringAsync().Result;

        }
        #endregion
    }
}

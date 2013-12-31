using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YNABv1.Helpers
{
    public static class DropboxHelper
    {
        private static string key = DropboxApi.KEY;
        private static string secret = DropboxApi.SECRET;
        private static string oauthToken = "";
        private static string oauthTokenSecret = "";
        private static string accessToken = "";
        private static string accessTokenSecret = "";
        private static MainPage p;

        public static void Setup(MainPage page)
        {
            p = page;
            var uri = new Uri("https://api.dropbox.com/1/oauth/request_token");

            // Generate a signature
            OAuthBase oAuth = new OAuthBase();
            string nonce = oAuth.GenerateNonce();
            string timeStamp = oAuth.GenerateTimeStamp();
            string parameters;
            string normalizedUrl;
            string signature = oAuth.GenerateSignature(uri, key, secret,
                String.Empty, String.Empty, "GET", timeStamp, nonce, OAuthBase.SignatureTypes.HMACSHA1,
                out normalizedUrl, out parameters);

            signature = Uri.EscapeUriString(signature);

            StringBuilder requestUri = new StringBuilder(uri.ToString());
            requestUri.AppendFormat("?oauth_consumer_key={0}&", key);
            requestUri.AppendFormat("oauth_nonce={0}&", nonce);
            requestUri.AppendFormat("oauth_timestamp={0}&", timeStamp);
            requestUri.AppendFormat("oauth_signature_method={0}&", "HMAC-SHA1");
            requestUri.AppendFormat("oauth_version={0}&", "1.0");
            requestUri.AppendFormat("oauth_signature={0}", signature);

            var request = (HttpWebRequest)WebRequest.Create(new Uri(requestUri.ToString()));
            request.Method = "GET";

            request.BeginGetResponse(r => {
                var response = request.EndGetResponse(r);

                var queryString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                var parts = queryString.Split('&');
                oauthToken = parts[1].Substring(parts[1].IndexOf('=') + 1);
                oauthTokenSecret = parts[0].Substring(parts[0].IndexOf('=') + 1);
                //p.DropboxSetupCallback();
            }, null);
        }

        public static string GetAuthUrl()
        {
            string queryString = String.Format("oauth_token={0}", oauthToken);
            string authorizeUrl = "https://www.dropbox.com/1/oauth/authorize?" + queryString + "&style=mobile";
            return authorizeUrl;
        }

        public static void Finish(string uri)
        {
            OAuthBase oAuth = new OAuthBase();

            var nonce = oAuth.GenerateNonce();           
            var timeStamp = oAuth.GenerateTimeStamp();
            string parameters;
            string normalizedUrl;
            var signature = oAuth.GenerateSignature(new Uri(uri), key, secret,
                oauthToken, oauthTokenSecret, "GET", timeStamp, nonce, 
                OAuthBase.SignatureTypes.HMACSHA1, out normalizedUrl, out parameters);
            signature = HttpUtility.UrlEncode(signature);

            //  Now you can send the request.
            var requestUri = new StringBuilder(uri);
            requestUri.AppendFormat("?oauth_consumer_key={0}&", key);
            requestUri.AppendFormat("oauth_token={0}&", oauthToken);
            requestUri.AppendFormat("oauth_nonce={0}&", nonce);
            requestUri.AppendFormat("oauth_timestamp={0}&", timeStamp);
            requestUri.AppendFormat("oauth_signature_method={0}&", "HMAC-SHA1");
            requestUri.AppendFormat("oauth_version={0}&", "1.0");
            requestUri.AppendFormat("oauth_signature={0}", signature);

            var request = (HttpWebRequest) WebRequest.Create(requestUri.ToString());
            request.Method = "GET";

            // Parsing the response will give you a return value which resembles this:
            // oauth_token_secret=95grkd9na7hm&oauth_token=ccl4li5n1q9b
            request.BeginGetResponse(r => {
                var response = request.EndGetResponse(r);
                var reader = new StreamReader(response.GetResponseStream());
                var accessTokenAndSecret = reader.ReadToEnd();

                var parts = accessTokenAndSecret.Split('&');
                accessToken = parts[1].Substring(parts[1].IndexOf('=') + 1);
                accessTokenSecret = parts[0].Substring(parts[0].IndexOf('=') + 1);
                //p.DropboxFinishCallback();
            }, null);
        }

        public static Tuple<string, string> GetAccessTokenAndSecret()
        {
            return new Tuple<string, string>(accessToken, accessTokenSecret);
        }
    }
}

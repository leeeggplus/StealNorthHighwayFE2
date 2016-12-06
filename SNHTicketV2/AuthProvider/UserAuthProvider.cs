using System;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.IO;
using System.Collections;

namespace SNHTicketV2.Authentication
{
    public class UserAuthProvider : AuthProvider
    {
        private string ps_MainEntryUrl;
        private string ps_random13DigitStamp;
        private Uri puri_shop48cnAuth;

        // static member
        private static string ps_shop48cn = "shop.48.cn";
        private static string ps_httpshop48cn = "http://shop.48.cn";

        public override bool Authenticate()
        {
            return MainEntryAuth();
        }

        public override bool IsAuthComplete()
        {
            throw new NotImplementedException();
        }

        public UserAuthProvider(string userName, string userPwd) : base(userName, userPwd)
        {
            Random random = new Random();
            string rnd1 = random.Next(14700000, 14900000).ToString();
            string rnd2 = random.Next(10000, 99999).ToString();
            string rnd = rnd1 + rnd2;

            this.ps_random13DigitStamp = rnd.ToString();
            this.ps_MainEntryUrl = string.Format(ConfigurationManager.AppSettings["MainEntryUrl"], this.ps_UserName, this.ps_UserPwd, this.ps_random13DigitStamp);
        }        

        // Main Entry Auth
        // Request Type: GET
        // URL format
        // Request Protocol: http://
        // user.snh48.com/vip48.php?callback=jQuery18006247164210821841_1479118420647&username=yukanana7&password=angel0416&act=login&_=1479118433288
        private bool MainEntryAuth()
        {
            // Main Entry Request - GET
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.ps_MainEntryUrl);
            request.Referer = ps_StartPointReferer;
            request.UserAgent = ps_UserAgent;
            request.KeepAlive = true;
            request.Method = "GET";
            request.Accept = "*/*";
            request.Headers.Add("Accept-Language", ps_AcceptLanguage);
            request.Headers.Add("Accept-Encoding", ps_AcceptEncoding);

            // Main Entry Response
            bool result = false;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringContainer = new StreamReader(response.GetResponseStream()).ReadToEnd().Trim();
                // substring 1 -> stringContainer`s size - 2
                // purpose: exclude the "(" at first and ")" at the end of the response. 
                string responseString = stringContainer.Substring(1, stringContainer.Length - 2);

                // Deserialize to hashtable
                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
                object o = jsSerializer.Deserialize(responseString, typeof(Hashtable));

                // desc value
                Hashtable jsonResult = o as Hashtable;
                string endPointBlocksString = jsonResult["desc"].ToString().Trim();

                // Get endPointsUri
                string startTag = "src=";
                string endTag = "reload=";

                int startIndex = endPointBlocksString.IndexOf(startTag);
                int endIndex = endPointBlocksString.IndexOf(endTag);

                while (startIndex > 0 && endIndex > 0 &&
                       startIndex < endPointBlocksString.Length && endIndex < endPointBlocksString.Length)
                {
                    int len = endIndex - 2 - (startIndex + startTag.Length + 1);
                    Uri endpoint = new Uri(endPointBlocksString.Substring(startIndex + startTag.Length + 1, len));
                    authEndPoints.Add(endpoint);

                    // shop48cn endpoint auth Uri
                    if (endpoint.AbsoluteUri.Contains(ps_shop48cn))
                    {
                        puri_shop48cnAuth = endpoint;
                        result = true;
                    }

                    // Move to the next Uri
                    startIndex = endPointBlocksString.IndexOf(startTag, endIndex);
                    if (startIndex > 0)
                        endIndex = endPointBlocksString.IndexOf(endTag, startIndex);
                }
            }

            return result;
        }
    }
}

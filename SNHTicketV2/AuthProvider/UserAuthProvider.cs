using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;


using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace SNHTicketV2.Authentication
{
    public class UserAuthProvider : AuthProvider
    {
        private string ps_MainEntryUrl;
        private string ps_random13DigitStamp;
        private Uri puri_shop48cnAuthUrl;

        // static member - URLs
        private static string ps_shop48cn = "shop.48.cn";
        private static string ps_httpshop48cn = "http://shop.48.cn";

        // static member - cookie names
        private static string ps_shop48cn_routeCookieName = "route";
        private static string ps_shop48cn_aspNetAppCookieName = ".AspNet.ApplicationCookie";
        private static string ps_shop48cn_csrfCookieName = "__RequestVerificationToken";

        // static member - cookie regex pattern
        private static string routeCookieRegexPat = @"route=([a-z]|[A-Z]|\d)+;{1}";
        private static string aspNetAppCookieRegexPat = @".AspNet.ApplicationCookie=([a-z]|[A-Z]|\d|_|-)+;{1}";
        private static string csrfCookieRegexPat = @"__RequestVerificationToken=([a-z]|[A-Z]|\d|_|-)+;{1}";


        /// <summary>
        /// ctor
        /// </summary>
        /// <returns>Null</returns>
        public UserAuthProvider(string userName, string userPwd) : base(userName, userPwd)
        {
            Random random = new Random();
            string rnd1 = random.Next(14700000, 14900000).ToString();
            string rnd2 = random.Next(10000, 99999).ToString();
            string rnd = rnd1 + rnd2;

            this.ps_random13DigitStamp = rnd.ToString();
            this.ps_MainEntryUrl = string.Format(ConfigurationManager.AppSettings["MainEntryUrl"], this.ps_UserName, this.ps_UserPwd, this.ps_random13DigitStamp);
        }


        /// <summary>
        /// override abstract method.
        /// trigger the authenticate for common users.
        /// </summary>
        /// <returns>bool</returns>
        public override bool Authenticate()
        {
            bool authResult = MainEntryAuth() && AuthShop48cn_S1() && AuthShop48cn_S2();
            this.pb_authComplete = authResult;

            return authResult;
        }


        /// <summary>
        /// override abstract method.
        /// check if authentication procress is complete
        /// </summary>
        /// <returns>bool</returns>
        public override bool IsAuthComplete()
        {
            return this.pb_authComplete;
        }


        /// <summary>
        /// Main Entry Auth
        /// Request Type: GET
        /// URL format
        /// Request Protocol: http://
        /// user.snh48.com/vip48.php?callback=jQuery18006247164210821841_1479118420647&username=yukanana7&password=angel0416&act=login&_=1479118433288
        /// </summary>
        /// <returns>bool</returns>
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
                        puri_shop48cnAuthUrl = endpoint;
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

        /// <summary>
        /// Auth Shop48cn Step #1: http://shop.48.cn/api/uc.ashx
        /// two cookies
        /// cookie #1: route cookie, length=32
        /// route=7317c4b93b94800f61ea7f54b5887351
        ///
        /// cookie #2: .aspnet.application cookie
        /// .AspNet.ApplicationCookie=1_knOFvdfdWyAqbvZhaWSAVvOufmZ2UUHiO3Etn-Rvk2PLZVzpbqi8DI5aI_3w1pA_dm7rMQavQ-4SnOjZTSr_gNfLzD5MsFA2uPr9309H_
        ///                           AXa926E9iqxtl6RDmTsr8APdDPRF084Yep169REJvhR0NIDUUiwoG2pRNNsrIJoTn459y8baiI-NmlMaRFuOm-vDiDQE5Kzz-67CJpI8_Hq
        ///                           3lMn7UQL_h70LklFPG3dNMKsdpkbWU1cLABaBymbsv_6mhEXiooLemPWFt49vHusrJXqk2NLwjBth_PkXaawB8-AsSfvxfRPE8SYCgO3t5B
        ///                           rU3iDot2Lcu-wrxRu8PyA2tnWAjpF94dcHM98VzsHkJ1x8Zc0u27bywzgWF_dO2HxCOjt5yLf7SEZINwPe1ROOTn3G8swgh6cy-lmeW6uBj
        ///                           G1E85BAQAr2h6QlD9xkTej4_CdkHqRFNAzkxiUMv5OGl3EEm21WpnKX3mJo8sO1YvkZlL21VsZuiT4b9Ir__J3MLWXzIiKI6zm97ntYg3DB
        ///                           Fj2-ye7OQt2TWr0Suo2w;
        /// </summary>
        /// <returns>bool</returns>
        private bool AuthShop48cn_S1()
        {
            bool result = false;

            // Start the request for route and .AspNet.ApplicationCookie cookies
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.puri_shop48cnAuthUrl.AbsoluteUri);
            request.Referer = ps_StartPointReferer;
            request.UserAgent = ps_UserAgent;
            request.KeepAlive = true;
            request.Method = "GET";
            request.Accept = "*/*";
            request.Headers.Add("Accept-Language", ps_AcceptLanguage);
            request.Headers.Add("Accept-Encoding", ps_AcceptEncoding);

            // send request            
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string setCookies = response.Headers["Set-Cookie"];

                Match routeMatch = Regex.Match(setCookies, routeCookieRegexPat);
                Match aspNetAppMatch = Regex.Match(setCookies, aspNetAppCookieRegexPat);

                if (routeMatch.Success && aspNetAppMatch.Success)
                {

                    string routeCookieValue = routeMatch.Value.Replace(ps_shop48cn_routeCookieName + "=", string.Empty).Replace(";", string.Empty);
                    string aspNetAppCookieValue = aspNetAppMatch.Value.Replace(ps_shop48cn_aspNetAppCookieName + "=", string.Empty).Replace(";", string.Empty);

                    cookies.Add(ps_shop48cn_routeCookieName, routeCookieValue);
                    cookies.Add(ps_shop48cn_aspNetAppCookieName, aspNetAppCookieValue);
                    result = true;
                }
            }

            return result;
        }


        /// <summary>
        /// Auth Shop48cn Step #2: http://shop.48.cn/
        /// get cookie: __RequestVerificationToken, Preventing Cross-Site Request Forger Cookie
        /// __RequestVerificationToken=5PFBlCSx8Y3r1qEkLmRsUINmhzfhH0U3m-UrUpW1peAvBpuEFB7rfi9Ey8KwbOdlzc3IvAaNyW-7AYAtd6d8HhbK6kpC5ILF8P6yv2alBZA1
        /// </summary>
        /// <returns>bool</returns>
        private bool AuthShop48cn_S2()
        {
            bool result = false;

            // Start the request for route and .AspNet.ApplicationCookie cookies
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ps_httpshop48cn);
            request.UserAgent = ps_UserAgent;
            request.KeepAlive = true;
            request.Method = "GET";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.Headers.Add("Accept-Language", ps_AcceptLanguage);
            request.Headers.Add("Accept-Encoding", ps_AcceptEncoding);
            request.Headers.Add("Upgrade-Insecure-Requests", "1");

            if (request.CookieContainer == null)
                request.CookieContainer = new CookieContainer();

            // route cookie
            request.CookieContainer.Add(new Cookie(ps_shop48cn_routeCookieName, cookies[ps_shop48cn_routeCookieName], "/", ps_shop48cn));
            // aspNetApp cookie
            request.CookieContainer.Add(new Cookie(ps_shop48cn_aspNetAppCookieName, cookies[ps_shop48cn_aspNetAppCookieName], "/", ps_shop48cn));

            // send request           
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string setCookies = response.Headers["Set-Cookie"];
                Match csrfMatch = Regex.Match(setCookies, csrfCookieRegexPat);

                if (csrfMatch.Success)
                {
                    string csrfCookieValue = csrfMatch.Value.Replace(ps_shop48cn_csrfCookieName + "=", string.Empty).Replace(";", string.Empty);
                    cookies.Add(ps_shop48cn_csrfCookieName, csrfCookieValue);
                    result = true;
                }
            }

            return result;
        }
    }
}
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNHTicketV2.Authentication
{
    public class VipAuthProvider : AuthProvider
    {
        // private static string ps_www48cn_uchome_reward_log_CookieName = "uchome_reward_log";
        private static string ps_www48cn_uchome_auth_CookieName = "uchome_auth";
        private static string ps_www48cn_uchome_loginuser_CookieName = "uchome_loginuser";

        // protected static string uchomeRewardLogCookieRegexPat = @"route=([a-z]|[A-Z]|\d)+;{1}";
        protected static string uchomeAuthCookieRegexPat = @"uchome_auth=([a-z]|[A-Z]|%|\d)+;{1}";

        public VipAuthProvider(string userName, string userPwd) : base(userName, userPwd)
        {
        }

        public override bool Authenticate()
        {
            bool shopAuthResult = UserSnh48Com_Vip48Php_Auth() && Www48cn_Api_UcPhp_Auth();
            this.pb_authComplete = shopAuthResult;

            return shopAuthResult;
        }

        public override bool IsAuthComplete()
        {
            return this.pb_authComplete;
        }


        /// <summary>
        /// Auth Www48cn: http://www.48.cn/api/uc.php
        /// two cookies
        /// cookie #1: uchome_auth cookie
        /// uchome_auth = ae9dbVlQCPZ5b%2Bfr2DCvIz10qWV2i89SdS3zJ2Du4j4uiqYaAaZ5bufwxDLraafdM5hB5d%2FrXdQZSe6PUt4W%2BDjKFrU
        ///
        /// cookie #2: uchome_loginuser cookie
        /// uchome_loginuser = <login_name> for vip user
        /// </summary>
        /// <returns>bool</returns>
        protected bool Www48cn_Api_UcPhp_Auth()
        {
            bool result = false;

            // Start the request for route and .AspNet.ApplicationCookie cookies
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.puri_www48cnAuthUrl.AbsoluteUri);
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

                // Match uchomeRewardLogMatch = Regex.Match(setCookies, uchomeRewardLogCookieRegexPat);
                Match uchomeAuthMatch = Regex.Match(setCookies, uchomeAuthCookieRegexPat);

                if (uchomeAuthMatch.Success)
                {

                    // string uchomeRewardLogCookieValue = uchomeRewardLogMatch.Value.Replace(ps_www48cn_uchome_reward_log_CookieName + "=", string.Empty).Replace(";", string.Empty);
                    string uchomeAuthValue = uchomeAuthMatch.Value.Replace(ps_www48cn_uchome_auth_CookieName + "=", string.Empty).Replace(";", string.Empty);

                    // cookies.Add(ps_www48cn_uchome_reward_log_CookieName, uchomeRewardLogCookieValue);
                    cookies.Add(ps_www48cn_uchome_auth_CookieName, uchomeAuthValue);
                    cookies.Add(ps_www48cn_uchome_loginuser_CookieName, this.ps_UserName);

                    result = true;
                }
            }

            return result;
        }
    }
}

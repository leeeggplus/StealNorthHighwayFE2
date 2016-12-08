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
        // static member - cookie names
        // private static string ps_www48cn_uchome_reward_log_CookieName = "uchome_reward_log";
        private static string ps_www48cn_uchome_auth_CookieName = "uchome_auth";
        private static string ps_www48cn_uchome_loginuser_CookieName = "uchome_loginuser";

        // static member - URLs
        private static string ps_dot48cn = ".48.cn";

        // protected static string uchomeRewardLogCookieRegexPat = @"route=([a-z]|[A-Z]|\d)+;{1}";
        protected static string uchomeAuthCookieRegexPat = @"uchome_auth=([a-z]|[A-Z]|%|\d)+;{1}";

        public VipAuthProvider(string userName, string userPwd) : base(userName, userPwd)
        {
        }

        public override bool Authenticate()
        {
            bool shopAuthResult = false;

            // auth for common user consists of four steps:
            // step#1: UserSnh48Com_Vip48Php_Auth()
            // call:   http://user.snh48.com/vip48.php?username=yukanana7&password=angel0416&act=login&_=1479118433288
            // cookie: no cookie returned
            shopAuthResult = UserSnh48Com_Vip48Php_Auth();

            // step#2: Www48cn_Api_UcPhp_Auth()
            // call:   http://www.48.cn/api/uc.php
            // cookie: uchome_auth, uchome_loginuser
            if (shopAuthResult)
                shopAuthResult = shopAuthResult && Www48cn_Api_UcPhp_Auth();

            // step#3: Shop48cn_Api_UcAshx_Auth() 
            // call:   http://shop.48.cn/api/uc.ashx
            // cookie: route, .AspNet.ApplicationCookie
            if (shopAuthResult)
                shopAuthResult = shopAuthResult && Shop48cn_Api_UcAshx_Auth();

            // step#4: Shop48cn_Auth()
            // call:   http://shop.48.cn/
            // cookie: __RequestVerificationToken
            if (shopAuthResult)
                shopAuthResult = shopAuthResult && Shop48cn_Auth();

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
        private bool Www48cn_Api_UcPhp_Auth()
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

        /// <summary>
        /// VIP AUTH PROVIDER NEED TO OVERRIDE THIS FUNCTION
        /// Auth Shop48cn: http://shop.48.cn/        
        /// get cookie: __RequestVerificationToken, Preventing Cross-Site Request Forger Cookie
        /// __RequestVerificationToken=5PFBlCSx8Y3r1qEkLmRsUINmhzfhH0U3m-UrUpW1peAvBpuEFB7rfi9Ey8KwbOdlzc3IvAaNyW-7AYAtd6d8HhbK6kpC5ILF8P6yv2alBZA1
        /// </summary>
        /// <returns>bool</returns>
        protected override bool Shop48cn_Auth()
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
            // uchome_auth cookie
            request.CookieContainer.Add(new Cookie(ps_www48cn_uchome_auth_CookieName, cookies[ps_www48cn_uchome_auth_CookieName], "/", ps_dot48cn));
            // aspNetApp cookie
            request.CookieContainer.Add(new Cookie(ps_www48cn_uchome_loginuser_CookieName, cookies[ps_www48cn_uchome_loginuser_CookieName], "/", ps_dot48cn));

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

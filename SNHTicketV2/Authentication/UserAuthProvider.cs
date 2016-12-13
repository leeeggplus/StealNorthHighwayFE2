using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections;
using System.Configuration;

using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using SNHTicketV2.OrderManagement;

namespace SNHTicketV2.Authentication
{
    public class UserAuthProvider : AuthProvider
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <returns>Null</returns>
        public UserAuthProvider(Order order) : base(order)
        {            
        }    

        /// <summary>
        /// override abstract method.
        /// trigger the authenticate for common users.
        /// </summary>
        /// <returns>bool</returns>
        public override bool Authenticate()
        {
            bool shopAuthResult = false;

            // auth for common user consists of three steps:
            // step#1: UserSnh48Com_Vip48Php_Auth()
            // call:   http://user.snh48.com/vip48.php?username=yukanana7&password=angel0416&act=login&_=1479118433288
            // cookie: no cookie returned
            shopAuthResult = UserSnh48Com_Vip48Php_Auth();

            // step#2: Shop48cn_Api_UcAshx_Auth() 
            // call:   http://shop.48.cn/api/uc.ashx
            // cookie: route, .AspNet.ApplicationCookie
            if (shopAuthResult)
                shopAuthResult = shopAuthResult && Shop48cn_Api_UcAshx_Auth();

            // step#3: Shop48cn_Auth()
            // call:   http://shop.48.cn/
            // cookie: __RequestVerificationToken
            if (shopAuthResult)
                shopAuthResult = shopAuthResult && Shop48cn_Auth();

            this.pb_authComplete = shopAuthResult;
            return shopAuthResult;
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
        /// Header for Submit Order and Check Order is same
        /// POST /TOrder/add HTTP/1.1
        /// Host: shop.48.cn
        /// Connection: keep-alive
        /// Content-Length: 56
        /// Accept: application/json, text/javascript, */*; q=0.01
        /// Origin: http://shop.48.cn
        /// X-Requested-With: XMLHttpRequest
        /// User-Agent: Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36
        /// Content-Type: application/x-www-form-urlencoded; charset=UTF-8
        /// Referer: http://shop.48.cn/tickets/item/{showId}?seat_type={seatType}
        /// Accept-Encoding: gzip, deflate
        /// Accept-Language: en-US,en;q=0.8
        /// Cookie: pgv_pvi=1006842880; 
        ///         tencentSig=9715923968; 
        ///         route=72b22cfe13b6559dd934c0716c55a35b; 
        ///         __RequestVerificationToken=5IejCZnHwDm97EuDD-bAs-FboRtAf8c-15xiuQvEJHw4wwwCVPr8IFt2ZzkQA1EI6HanH8dJfW5xdKFGI124IMQnx-lREVoRd6348xsEd2Y1; 
        ///         IESESSION=alive;
        ///         pgv_si=s5236094976; 
        ///         .AspNet.ApplicationCookie=CsumVisBfPgM95oh7smlBhp3PBfASLjSMIcF84XZXwumB6sEr0Xe-q3ZKXYUj7iOum96KYkstMu-yOjWxi6MaKpvVapzKn454GS3EXngqvGr5dzYbjH-uy4uaKYulDNavqWYr0nnhoVuobs5KZJtCT
        ///                                   yhY07MOX1g7vZB3JyUEnx3OYNo0wAQ11qNQEuFj0ovVtwLWIPYSZ8KR6ZQzkiCUYU2YT2a1At_DrEucj2ozUn50FoH-ZRwx8gMGHP4mUOmPOh-CKBGMP86gfdMBUoQy4e7m5P-1mJbApp12jS1Njx8
        ///                                   XVTCJHDToS52u6EPfTx3edujbqvGrIn0PimwRxZnbk6HekkA44aPantuzUeLyzQqWUtEz-lT2dWXM0EHp68ztCzMep0Su5KfJlp_oSnrkVH6yDyS9Hxe162fE6Vi8D9eLmz_9jDq6Zyc7O-p4v-map
        ///                                   -pUHKNEAS_5lMAYvAuhtLErWgODtcMtN73GXSwqgtZgkAdEJe6g_Dus1RKKiIpoNols3PapAkpt-P7QuOZCg; uchome_loginuser=leeegg1; _qddamta_4006176598=3-0; _qddaz=QD.bb0
        ///                                   2qt.w01t0a.ivhvf6xn; _qdda=3-1.2dkq5o; _qddab=3-83te1g.ivkv5drz
        /// </summary>
        /// <returns>HttpWebRequest</returns>
        protected override HttpWebRequest ComposeSubmitOrderRequestHeader(string requestType)
        {
            int showID = this.p_order.ShowID;
            int seatType = this.p_order.SeatType;

            string requestUrl = ps_shop48cn_orderSubmitUri;
            string showDetailsUri = string.Format(ps_shop48cn_showUri, showID.ToString(), seatType.ToString());
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);

            request.Method = requestType;
            request.Accept = "application/json, text/javascript, */*; q=0.01";
            request.Headers.Add("Accept-Language", ps_AcceptLanguage);
            request.Headers.Add("Accept-Encoding", ps_AcceptEncoding);
            request.UserAgent = ps_UserAgent;
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");

            request.Headers.Add("Origin", ps_httpshop48cn);
            request.Referer = showDetailsUri;
            request.KeepAlive = true;

            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            // Compose Cookie
            if (request.CookieContainer == null)
                request.CookieContainer = new CookieContainer();

            // route cookie
            request.CookieContainer.Add(new Cookie(ps_shop48cn_routeCookieName, cookies[ps_shop48cn_routeCookieName], "/", ps_shop48cn));
            // aspNetApp cookie
            request.CookieContainer.Add(new Cookie(ps_shop48cn_aspNetAppCookieName, cookies[ps_shop48cn_aspNetAppCookieName], "/", ps_shop48cn));
            // csrf cookie
            request.CookieContainer.Add(new Cookie(ps_shop48cn_csrfCookieName, cookies[ps_shop48cn_csrfCookieName], "/", ps_shop48cn));

            return request;
        }


        protected override HttpWebRequest ComposeCheckOrderRequestHeader(string requestType)
        {
            Random rnd = new Random();
            int showID = this.p_order.ShowID;
            int seatType = this.p_order.SeatType;            

            // Compose Post Data
            string postDataFormat = "?id={0}&r={1}";
            string postData = string.Format(postDataFormat, showID.ToString(), rnd.NextDouble().ToString());

            string requestUrl = ps_shop48cn_orderCheckUri + postData;
            string showDetailsUri = string.Format(ps_shop48cn_showUri, showID.ToString(), seatType.ToString());
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);

            request.Method = requestType;
            request.Accept = "application/json, text/javascript, */*; q=0.01";
            request.Headers.Add("Accept-Language", ps_AcceptLanguage);
            request.Headers.Add("Accept-Encoding", ps_AcceptEncoding);
            request.UserAgent = ps_UserAgent;
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");

            request.Headers.Add("Origin", ps_httpshop48cn);
            request.Referer = showDetailsUri;
            request.KeepAlive = true;

            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            // Compose Cookie
            if (request.CookieContainer == null)
                request.CookieContainer = new CookieContainer();

            // route cookie
            request.CookieContainer.Add(new Cookie(ps_shop48cn_routeCookieName, cookies[ps_shop48cn_routeCookieName], "/", ps_shop48cn));
            // aspNetApp cookie
            request.CookieContainer.Add(new Cookie(ps_shop48cn_aspNetAppCookieName, cookies[ps_shop48cn_aspNetAppCookieName], "/", ps_shop48cn));
            // csrf cookie
            request.CookieContainer.Add(new Cookie(ps_shop48cn_csrfCookieName, cookies[ps_shop48cn_csrfCookieName], "/", ps_shop48cn));

            return request;
        }
    }
}
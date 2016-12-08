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

namespace SNHTicketV2.Authentication
{
    public class UserAuthProvider : AuthProvider
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <returns>Null</returns>
        public UserAuthProvider(string userName, string userPwd) : base(userName, userPwd)
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
    }
}
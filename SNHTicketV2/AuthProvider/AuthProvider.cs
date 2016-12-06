using System;
using System.Collections.Generic;

using System.Net;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNHTicketV2.Authentication
{
    public abstract class AuthProvider
    {
        protected string ps_UserName;
        protected string ps_UserPwd;        
        protected bool   pb_authComplete;

        protected static string ps_StartPointReferer = "http://vip.48.cn/Home/Login/index.html";
        protected static string ps_UserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36";
        protected static string ps_AcceptLanguage = "en-US,en;q=0.8";
        protected static string ps_AcceptEncoding = "gzip, deflate, sdch";

        protected List<Uri> authEndPoints;
        protected Dictionary<string, string> cookies;

        /* construscor for base */
        public AuthProvider(string userName, string userPwd)
        {
            if (userName == string.Empty || userPwd == string.Empty)
                throw new NullReferenceException("username or password is empty");            

            this.ps_UserName = userName;
            this.ps_UserPwd = userPwd;
            this.pb_authComplete = false;
            
            this.authEndPoints = new List<Uri>();   
            this.cookies = new Dictionary<string, string>();
        }        

        public abstract bool IsAuthComplete();

        public abstract bool Authenticate();
    }
}
 
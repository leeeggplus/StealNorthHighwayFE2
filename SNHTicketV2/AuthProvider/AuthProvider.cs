using System;
using System.Configuration;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNHTicketV2.Authentication
{
    public abstract class AuthProvider
    {
        protected string ps_UserName;
        protected string ps_UserPwd;
        protected string ps_MainEntryUrl;
        protected string ps_random13DigitStamp;
        protected bool   b_authComplete;

        protected List<Uri> endPoints = new List<Uri>();
        protected Dictionary<string, string> shop48cnCookies = new Dictionary<string, string>();

        /* construscor for base */
        public AuthProvider(string userName, string userPwd)
        {
            if (userName == string.Empty || userPwd == string.Empty)
                throw new NullReferenceException("username or password is empty");

            Random random = new Random();
            string rnd1 = random.Next(14700000, 14900000).ToString();
            string rnd2 = random.Next(10000, 99999).ToString();
            string rnd = rnd1 + rnd2;

            this.ps_UserName = userName;
            this.ps_UserPwd = userPwd;
            this.ps_random13DigitStamp = rnd.ToString();
            this.ps_MainEntryUrl = string.Format(ConfigurationManager.AppSettings["MainEntryUrl"], this.ps_UserName, this.ps_UserPwd, this.ps_random13DigitStamp);
        }

        public abstract bool IsAuthComplete();

        public abstract bool Authenticate();
    }
}
 
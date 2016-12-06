using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNHTicketV2.Authentication
{
    public class VipAuthProvider : AuthProvider
    {
        private string ps_MainEntryUrl;
        private string ps_random13DigitStamp;

        public VipAuthProvider(string userName, string userPwd) : base(userName, userPwd)
        {
            Random random = new Random();
            string rnd1 = random.Next(14700000, 14900000).ToString();
            string rnd2 = random.Next(10000, 99999).ToString();
            string rnd = rnd1 + rnd2;

            this.ps_random13DigitStamp = rnd.ToString();
            this.ps_MainEntryUrl = string.Format(ConfigurationManager.AppSettings["MainEntryUrl"], this.ps_UserName, this.ps_UserPwd, this.ps_random13DigitStamp);
        }

        public override bool Authenticate()
        {
            throw new NotImplementedException();
        }

        public override bool IsAuthComplete()
        {
            throw new NotImplementedException();
        }
    }
}

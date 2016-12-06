using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNHTicketV2.Authentication
{
    public class UserAuthProvider : AuthProvider
    {
        public UserAuthProvider(string userName, string userPwd) : base(userName, userPwd)
        {

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNHTicketV2.Authentication;

namespace SNHTicketV2
{
    class Program
    {
        static void Main(string[] args)
        {
            AuthProvider uap = new UserAuthProvider("a", "b");
            AuthProvider vap = new VipAuthProvider("a", "b");
        }
    }
}

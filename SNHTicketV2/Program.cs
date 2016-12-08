﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNHTicketV2.Authentication;
using SNHTicketV2.OrderManagement;

namespace SNHTicketV2
{
    class Program
    {
        static void Main(string[] args)
        {
            OrderHandler orderHandler = new OrderHandler();
            orderHandler.Call();

            Console.Read();

            //AuthProvider uap = new UserAuthProvider("leeegg1", "angel0416");
            // AuthProvider vap = new VipAuthProvider("kandik11", "kandi000");
            // vap.Authenticate();
        }
    }
}

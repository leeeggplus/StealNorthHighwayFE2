using System;
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
        }
    }
}

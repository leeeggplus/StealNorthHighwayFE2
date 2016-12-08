using System;
using System.IO;
using System.Xml;
using System.Threading;
using System.Collections.Generic;
using SNHTicketV2.Authentication;

using System.Threading.Tasks;

namespace SNHTicketV2.OrderManagement
{
    public class OrderHandler
    {
        // Members
        private List<Order> p_Orders;

        // static members
        private static string ps_userType_user = "user";
        private static string ps_userType_vip = "vip";

        /// <summary>
        /// .ctor
        /// </summary>
        /// <returns>null</returns>
        public OrderHandler()
        {
            p_Orders = new List<Order>();
            this.LoadOrders();
        }

        /// <summary>
        /// Load Order Requests from XML
        /// Max 10 requests/threads in one Process.
        /// </summary>
        /// <returns>void</returns>
        private void LoadOrders()
        {
            string filePath = Path.Combine(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory), @"Orders.xml");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);
            XmlNodeList nodeList = xmlDoc.SelectNodes("/Orders/Order");

            // Has Orders
            if (nodeList.Count > 0)
            {
                foreach (XmlNode node in nodeList)
                {
                    int showId, seatType, ticketNo;
                    string internalOrderName, username, pwd, strUserType;
                    UserType userType = UserType.Undefined;

                    try
                    {
                        internalOrderName = node.Attributes["Name"].Value.Trim();
                        username = node.Attributes["Username"].Value.Trim();
                        pwd = node.Attributes["Pwd"].Value.Trim();
                        strUserType = node.Attributes["UserType"].Value.Trim();

                        showId = int.Parse(node.Attributes["ShowID"].Value);
                        seatType = int.Parse(node.Attributes["SeatType"].Value);
                        ticketNo = int.Parse(node.Attributes["TicketNO"].Value);

                        if (strUserType == ps_userType_user)
                            userType = UserType.UnRealNameAuthedUser;
                        else if (strUserType == ps_userType_vip)
                            userType = UserType.VipUser;
                        else
                            continue;

                        Order order = new Order(internalOrderName, username, pwd, userType, showId, seatType, ticketNo);
                        p_Orders.Add(order);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }                    
                }
            }
        }

        // Multi-thread Call
        public void Call()
        {
            int nWorkThreads;
            int nCompletionPortThreads;
            ThreadPool.GetMaxThreads(out nWorkThreads, out nCompletionPortThreads);

            for (int i = 0; i < this.p_Orders.Count; i++)
            {
                ThreadPool.QueueUserWorkItem(FuncWaitCallback, this.p_Orders[i]);
            }
        }

        static void FuncWaitCallback(object parmOrder)
        {
            Order order = parmOrder as Order;
            AuthProvider authProvider = null;

            if (order == null)
            {
                // errors here
            }

            // initialize auth provider
            if (order.UserType == UserType.UnRealNameAuthedUser)
                authProvider = new UserAuthProvider(order);
            else
                authProvider = new VipAuthProvider(order);

            if (authProvider != null)
            {
                authProvider.Authenticate();
                if (authProvider.IsAuthComplete())
                    authProvider.SubmitOrder();
            }
        }
    }
}

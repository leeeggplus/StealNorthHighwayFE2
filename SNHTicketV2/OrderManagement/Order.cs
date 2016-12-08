using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNHTicketV2.Resource;
using SNHTicketV2.Authentication;

namespace SNHTicketV2.OrderManagement
{
    public class Order
    {
        // Request information
        private string   ps_OrderInternalName;
        private string   ps_Username;
        private string   ps_Pwd;        
        private int      pi_ShowID;
        private int      pi_SeatType;
        private int      pi_TicketNO;
        private UserType p_UserType;

        // Check before submit
        // Only submit if request is logically valid/correct
        private bool pb_isValid;
        private AuthProvider p_authProvider;

        // response from SNH48
        private bool    pb_OrderState;
        private string  ps_OrderSNHSysName;
        private string  ps_Message;

        // public attributes
        public string OrderInternalName
        {
            get { return this.ps_OrderInternalName; }
        }

        public string UserName
        {
            get { return this.ps_Username; }
        }

        public string Password
        {
            get { return this.ps_Pwd; }
        }

        public UserType UserType
        {
            get { return this.p_UserType; }
        }

        public int ShowID
        {
            get { return this.pi_ShowID; }
        }

        public int SeatType
        {
            get { return this.pi_SeatType; }
        }

        public int TicketNO
        {
            get { return this.pi_TicketNO; }
        }

        public bool IsValid
        {
            get { return this.pb_isValid; }
        }

        public bool OrderState
        {
            get { return this.pb_OrderState; }
            set { this.pb_OrderState = value; }
        }

        public string OrderSNHSysName
        {
            get { return this.ps_OrderSNHSysName; }
            set { this.ps_OrderSNHSysName = value; }
        }

        public string OrderMessage
        {
            get { return this.ps_Message; }
            set { this.ps_Message = value; }
        }

        public AuthProvider AuthProvider
        {
            get { return this.p_authProvider; }
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <returns>null</returns>
        public Order(string orderInternalName, string userName, string pwd, UserType userType, int showID, int seatType, int ticketNO)
        {
            this.pb_isValid = true;
            this.ps_OrderInternalName = orderInternalName;
            this.ps_Username = userName;
            this.ps_Pwd = pwd;
            this.pi_ShowID = showID;
            this.pi_SeatType = seatType;
            this.pi_TicketNO = ticketNO;
            this.p_UserType = userType;

            // Known Seat Type 2(VIP, only one), 3(seat, more than one), 4(stand, only one)
            if (this.pi_SeatType < 2 || this.pi_SeatType > 4)
                this.pb_isValid = false;

            if (this.pi_TicketNO <= 0)
                this.pb_isValid = false;

            // Seat Type 4/2 - only can buy one each account
            if (this.pi_TicketNO > 1 && (this.pi_SeatType == 4 || this.pi_SeatType == 2))
                this.pb_isValid = false;

            // initialize internal state and auth provider
            if (this.pb_isValid)
            {
                this.pb_OrderState = false;
                this.ps_OrderSNHSysName = string.Empty;
                this.ps_Message = string.Empty;

                // initialize auth provider
                if (this.p_UserType == UserType.UnRealNameAuthedUser)
                    this.p_authProvider = new UserAuthProvider(this.ps_Username, this.ps_Pwd);
                else
                    this.p_authProvider = new VipAuthProvider(this.ps_Username, this.ps_Pwd);                          
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNHTicketV2.Authentication
{
    public class SubmitCheckOrderResponse
    {
        private bool pb_HasError;
        private object po_ErrorCode;
        private object po_Message;
        private object po_ReturnObject;

        public bool HasError
        {
            get { return this.pb_HasError; }
        }

        public object ErrorCode
        {
            get { return this.po_ErrorCode; }
        }

        public object Message
        {
            get { return this.po_Message; }
        }

        public object ReturnObject
        {
            get { return this.po_ReturnObject; }
        }

        public SubmitCheckOrderResponse(bool hasError, object errorCode, object message, object returnObject)
        {
            this.pb_HasError = hasError;
            this.po_ErrorCode = errorCode;
            this.po_Message = message;
            this.po_ReturnObject = returnObject;
        }
    }
}

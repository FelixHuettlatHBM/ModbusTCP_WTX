using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hbm.Wt.CommonNetLib.Utils
{
        /// <summary>
        /// Class to define the information from the register of the device. 
        /// The message's type is ushort[], it is an array. 
        /// The publisher publishs this message, filled with the information from the register.
        /// to the Subscripter (the Device: WTX120). 
        /// </summary>
        public class MessageEvent<T> : EventArgs           // <T> Standard. 
        {
            private ushort[] message;

            // Constructor: 
            public MessageEvent(ushort[] param)
            {
                this.message = param;
            }

            public ushort[] Message
            {
                get { return message; }
                set { message = value; }
            }

            // The following methods show the bit masking and shifting (left and right shift) in general.

            public int left_shifted_value(int word, ushort bits)
            {
                return (this.message[word] << bits);
            }

            public int right_shifted_value(int word, ushort bits)
            {
                return (this.message[word] >> bits);
            }

            public int masked_value(int word, ushort adress)
            {
                return (this.message[word] & adress);
            }
        }
}

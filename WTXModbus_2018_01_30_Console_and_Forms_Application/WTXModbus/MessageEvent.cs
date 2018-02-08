/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
 * 
 * TCP/MODBUS Interface for WTX120 | 01/2018
 * 
 * Author : Felix Huettl 
 * 
 *  */

using System;

namespace Hbm.Devices.WTXModbus
{
    /// <summary>
    /// Class to define the information from the register of the device. 
    /// The message's type is ushort[], it is an array. 
    /// The publisher publishs this message, filled with the information from the register.
    /// to the Subscripter (the Device: WTX120). 
    /// </summary>
    public class MessageEvent : EventArgs
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
            set { message = value;}
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

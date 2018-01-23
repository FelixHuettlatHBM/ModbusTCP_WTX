/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
 * 
 * TCP/MODBUS Interface for WTX120 | 01/2018
 * 
 * Author : Felix Huettl
 * 
 *  */

using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WTXModbus
{
    /// <summary>
    /// This interface is used to specify the methods for the connection to the device more precise. It inherits from interface
    /// "Communication_Interface", which contains the basic methods to realize a communication to a device like the WTX120.
    /// 
    /// This interface has automatic-properties for the start adress, number of points, IP adress, sending/timer interval and the port,
    /// which are necessary for the connection. 
    /// 
    /// The methods connect(), Is_connected(), ReadRegister(), WriteRegister(data) and Close() are inherited from interface "Communication_Interface"
    /// and therefore not implemented in this interface. These methods are given in an comment to show how to implement them in this interface.
    /// The method ReadRegisterPublishing(MessageEvent e) publishes the event (MessageEvent) and read the register, 
    /// afterwards the message(from the register) will be sent back to WTX120.
    /// 
    /// The class Modbus_TCP inherits from this interface and from interface "Communication_Interface". 
    /// </summary>
    interface ICommunicationDevice : ICommunication 
    {
        event EventHandler<MessageEvent> RaiseDataEvent;

        ushort StartAdress { get; set; }
        ushort NumOfPoints { get; set; }
        string IP_Adress { get; set; }
        int Sending_interval { get; set; }
        int Port { get; set; }
        

        // new void Connect();          // The commented methods are given to show how to implement them in this interface.      
        // new bool Is_connected();
        // new void ReadRegister();

        void ReadRegisterPublishing(MessageEvent e);        

        // new void WriteRegister(ushort data);
        // new void Close();

    }
}

/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
 * 
 * TCP/MODBUS Interface for WTX120 | 01/2018
 * 
 * Author : Felix Huettl 
 * 
 *  */

using Modbus.Device;
using System;
using System.Net.Sockets;
using System.Threading;

namespace Hbm.Devices.WTXModbus
{
    /// <summary>
    /// This class establishs the communication to the device(here: WTX120), starts/ends the connection,
    /// read and write the register and shows the status of the connection and closes the connection to
    /// the device (here: WTX120). 
    /// Once a button event is clicked in class GUI, an asynchronous call in class WTX120 is started
    /// and finally in this class "Modbus_TCP" the register (of the device) is read or written. 
    /// The data exchange for reading a register between class "Modbus_TCP" and class "WTX_120" is event-based. 
    /// This class publishes the event (MessageEvent) and read the register, afterwards it will be sent back to WTX120. 
    /// </summary>
    public class ModbusTCP : ICommunicationDevice
    {
        private ModbusIpMaster master;
        private TcpClient client;
        private ushort[] data;

        private ushort startAdress;
        private ushort numOfPoints;
        private string iP_Adress;
        private int sending_interval;  // Timer1.Interval = Sending Interval 
        private int port;
        private bool connected;

        // Declaration of the event Eventhandler. For the message information from the register.
        public event EventHandler<MessageEvent> RaiseDataEvent;

        public ModbusTCP(string ipAddress)
        {
            this.connected = false;
            this.port = 502;
            this.iP_Adress = ipAddress; //IP-address to establish a successful connection to the device

            this.numOfPoints = 38;
            this.startAdress = 0;
            sending_interval = 5;       // Timer1.Interval = Sending Interval 
        }

        // This method is called from the device class "WTX120" and calls the method ReadRegisterPublishing(e:MessageEvent)
        // to create a new MessageEvent to read the register of the device. 
        public void ReadRegister()
        {
            if (this.connected == true)
                this.ReadRegisterPublishing(new MessageEvent(this.data/*, this.StartAdress, this.NumOfPoints*/));
        }

        // This method publishes the event (MessageEvent) and read the register, afterwards the message(from the register) will be sent back to WTX120.  
        // This method is declared as a virtual method to allow derived class to override the event call.
        public virtual void ReadRegisterPublishing(MessageEvent e)
        {
            // copy of the event to avoid that a race condition is prevented, if the former subscriber directly logs off after the last
            // condition( and after if(handler!=null) ) and before the event is triggered. 
            EventHandler<MessageEvent> handler = RaiseDataEvent;

            // If a subscriber exists: 
            if (handler != null)
            {
                try
                {
                    // Read the data: e.Message's type - ushort[]  
                    e.Message = master.ReadHoldingRegisters(this.StartAdress, this.NumOfPoints);
                    this.connected = true;
                }
                catch (System.ArgumentException)
                {
                    Console.WriteLine("\nNumber of points has to be between 1 and 125.\n");
                }
                catch (System.InvalidOperationException)
                {
                    this.connected = false;

                    //for (int index = 0; index < this.data.Length; index++)
                    //    this.data[index] = e.BootingMessage;

                    //handler(this, e);
                    
                    this.Connect();
                    Thread.Sleep(100);
                }

                this.data = e.Message;

                // After the read of the register the event is triggered in the following:
                // Thus the HandleDataEvent(object sender, MessageEvent e) in class WTX120 is called to 
                // process the read data. In Parameter e, e.Message, the data from the register is hold.
                handler(this, e);
            }
        }

        // This method establishs a connection to the device. Therefore an IP address and the port number
        // for the TcpClient is need. The client itself is used for the implementation of the ModbusIpMaster. 
        public void Connect()
        {
            try
            {
                client = new TcpClient(this.iP_Adress, this.port);
                master = ModbusIpMaster.CreateIp(client);
                this.connected = true;
            }
            catch (Exception)
            {
                this.connected = false;   // If the connection establishment has not been successful - connected=false. 
            }
        }

        // This method closes the connection to the device.
        public void Close()
        {
            client.Close();
        }

        // This method writes a command to the register of the device.
        // @param: command - ushort. Command in hex from the GUI. 
        public void WriteRegister(ushort wordNumber, ushort commandParam)
        {
            this.master.WriteSingleRegister(wordNumber, commandParam);
        }

        public void WriteArray2Reg(ushort wordNumber, ushort []commandParam)
        {
            this.master.WriteMultipleRegisters(wordNumber, commandParam);
        }



        // Getter/Setter for the IP_Adress, StartAdress, NumofPoints, Sending_interval, Port, Is_connected()
        public string IP_Adress
        {
            get { return this.iP_Adress; }
            set { this.iP_Adress = value; }
        }

        public ushort StartAdress
        {
            get { return this.startAdress; }
            set { this.startAdress = value; }
        }

        public ushort NumOfPoints
        {
            get { return this.numOfPoints; }
            set { this.numOfPoints = value; }
        }

        public int Sending_interval
        {
            get { return this.sending_interval; }
            set { this.sending_interval = value; }
        }

        public int Port
        {
            get { return this.port; }
            set { this.port = value; }
        }

        public bool is_connected
        {
            get { return this.connected; }
        }


    }

}

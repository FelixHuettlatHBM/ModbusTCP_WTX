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

namespace WTXModbus
{
    /// <summary>
    /// This class establishs the communication to the device(here: device WTX120 ), starts/ends the connection,
    /// read and write the register and shows the status of the connection and closes the connection to
    /// the device (here: WTX120). 
    /// Once a button event is clicked in class GUI, an asynchronous call in class WTX120 is started
    /// and finally in this class "Modbus_TCP" the register (of the device) is read or written. 
    /// The data exchange for reading a register between class "ModbusConnection" and class "WTX120" is event-based. 
    /// This class publishes the event (MessageEvent) and read the register, afterwards it will be sent back to WTX120. 
    /// </summary>
    public class ModbusTCPConnection : IModbusConnection
    {
        private ModbusIpMaster master;
        private TcpClient client;
        private ushort[] data;
        private ushort[] previousData;

        private ushort startAddress;
        private ushort numOfPoints;
        private string iP_address;
        private int sending_interval;  
        private int port;
        private bool connected;

        // Declaration of the event Eventhandler. For the message information from the register.
        public event EventHandler<NetConnectionEventArgs<ushort[]>> RaiseDataEvent;

        public ModbusTCPConnection(string ipAddress)
        {
            this.connected = false;
            this.port = 502;
            this.iP_address = ipAddress; //IP-address to establish a successful connection to the device

            this.numOfPoints = 38;
            this.startAddress = 0;
            sending_interval = 5;      

            previousData = new ushort[59];
            data = new ushort[59];

            for(int i=0;i<59;i++)
            {
                previousData[i] = 1;    // Initializing the array 'previousData' and 'Data' with different values, ...
                data[i] = 0;            // ... that the first output on the console/GUI is guaranteed.  
            }
        }

        /* 
         * This method is called from the device class "WTX120" and calls the method ReadRegisterPublishing(e:NetConnectionEventArgs)
         * to create a new MessageEvent to read the register of the device. 
         */
        public void Read()
        {
            if (this.connected == true)
                this.ReadRegisterPublishing(new NetConnectionEventArgs<ushort[]>(EventArgType.Data, this.data));

        }

        /* 
         * This method publishes the event (NetConnectionEventArgs, type Data) and read the register, afterwards the data(from the register) will be sent back to WTX120.  
         * This method is declared as a virtual method to allow derived class to override the event call.
         */
        protected virtual void ReadRegisterPublishing(NetConnectionEventArgs<ushort[]> e)
        {
            // copy of the event to avoid that a race condition is prevented, if the former subscriber directly logs off after the last
            // condition( and after if(handler!=null) ) and before the event is triggered.

            EventHandler<NetConnectionEventArgs<ushort[]>> handler = RaiseDataEvent;

            // If a subscriber exists: 
            if (handler != null)
            {
                try
                {
                    // Save the previous data before reading the new, actual one:
                    this.previousData = this.data;
                    // Read the actual data: e.Message's type - ushort[]  
                    e.Args = master.ReadHoldingRegisters(this.startAddress, this.getRegisterCount);
                    this.connected = true;
                }
                catch (System.ArgumentException)
                {
                    Console.WriteLine("\nNumber of points has to be between 1 and 125.\n");
                }
                catch (System.InvalidOperationException)
                {
                    this.connected = false;
                    this.Connect();
                }
                
                this.data = e.Args;

                // After the read of the register the event is triggered in the following:
                // Thus the HandleDataEvent(object sender, MessageEvent e) in class WTX120 is called to 
                // process the read data. In Parameter e, e.Message, the data from the register is hold.
                handler(this, e);
            }
        }

        // This method establishs a connection to the device. Therefore an IP address and the port number
        // for the TcpClient is need. The client itself is used for the implementation of the ModbusIpMaster. 
        public virtual void Connect()
        {
            try
            {
                client = new TcpClient(this.iP_address, this.port);
                master = ModbusIpMaster.CreateIp(client);
                this.connected = true;
            }
            catch (Exception)
            {
                this.connected = false;   // If the connection establishment has not been successful - connected=false. 
            }
        }

        // This method closes the connection to the device.
        public void ResetDevice()
        {
            client.Close();
        }

        // This method writes a command to the register of the device, to a single register. 
        // @param: command - ushort. Command in hex from the GUI or console application. 
        public void Write(ushort index, ushort data)
        {
            this.master.WriteSingleRegister(index, data);
        }

        // This method writes a command to the register of the device, to a single register. 
        // @param: command - ushort. Command in hex from the GUI. 
        public void Write(ushort index, ushort[] data)
        {
            this.master.WriteMultipleRegisters(index, data);
        }


        public ushort[] getPreviousData
        {
            get { return this.previousData; }
        }

        // Auto-properties (get and set) for the IP_Adress, StartAdress, NumofPoints, Sending_interval, Port, Is_connected()
        public string IP_Address
        {
            get { return this.iP_address; }
            set { this.iP_address = value; }
        }

        public ushort StartAdress
        {
            get { return this.startAddress; }
            set { this.startAddress = value; }
        }

        public ushort getRegisterCount
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

        public virtual bool is_connected
        {
            get { return this.connected; }
        }


    }

}

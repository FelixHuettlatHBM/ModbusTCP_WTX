
using HBM.WT.API;
using HBM.WT.API.WTX;

using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace HBM.WT.API.WTX.Modbus
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
    public class ModbusTCPConnection : INetConnection    //IModbusConnection
    {
        private ModbusIpMaster master;
        private TcpClient client;
        private ushort[] data;

        private ushort startAdress;
        private ushort numOfPoints;
        private string iPAddress;
        private int sendingInterval;  // Timer1.Interval = Sending Interval 
        private int port;
        private bool connected;    

        // Declaration of the event Eventhandler. For the message information from the register.
        // public event EventHandler<MessageEvent<ushort>> RaiseDataEvent;

        public event EventHandler BusActivityDetection;

        public virtual event EventHandler<NetConnectionEventArgs<ushort[]>> RaiseDataEvent; // virtual new due to tesing - 3.5.2018

        public ModbusTCPConnection(string ipAddress)
        {
            this.connected = false;
            this.port = 502;
            this.iPAddress = ipAddress; //IP-address to establish a successful connection to the device

            this.numOfPoints = 38;
            this.startAdress = 0;
            sendingInterval = 5;       // Timer1.Interval = Sending Interval 
        }

        // Alternative : Without Generics 

        // This method is called from the device class "WTX120" and calls the method ReadRegisterPublishing(e:MessageEvent)
        // to create a new MessageEvent to read the register of the device. 
        
        public void ReadRegister()
        {
            if (this.connected == true)
                this.ReadRegisterPublishing(new NetConnectionEventArgs<ushort[]>(EventArgType.Data, this.data));
        }
        
        /*
        public T Read<T>(object index)
        {
            if (this.connected == true)
                this.ReadRegisterPublishing(new NetConnectionEventArgs<ushort[]>(EventArgType.Data, this.data));

            return (T)Convert.ChangeType(0, typeof(T));

        }
        */

        // This method publishes the event (MessageEvent) and read the register, afterwards the message(from the register) will be sent back to WTX120.  
        // This method is declared as a virtual method to allow derived class to override the event call.
        //protected virtual void ReadRegisterPublishing(MessageEvent<ushort> e)

        public virtual void ReadRegisterPublishing(NetConnectionEventArgs<ushort[]> e)  // 25.4 Comment : 'virtual' machte hier probleme beim durchlaufen :o 
        {   // virtual new due to tesing - 3.5.2018
            try
                {
                // Read the data: e.Message's type - ushort[]  
                //e.Args = masterParam.ReadHoldingRegisters(this.StartAdress, this.getNumOfPoints);

                e.Args = master.ReadHoldingRegisters(this.StartAdress, this.NumOfPoints);
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
                    Thread.Sleep(100);
                }

                //this.data = e.Message;
                this.data = e.Args;

            // copy of the event to avoid that a race condition is prevented, if the former subscriber directly logs off after the last
            // condition( and after if(handler!=null) ) and before the event is triggered. 

            //RaiseDataEvent?.Invoke(this, e);

            EventHandler<NetConnectionEventArgs<ushort[]>> handler = RaiseDataEvent;

            //If a subscriber exists: 
            if (handler != null)
            {
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
                client = new TcpClient(this.iPAddress, this.port);
                master = ModbusIpMaster.CreateIp(client);
                this.connected = true;
            }
            catch (Exception)
            {
                this.connected = false;   // If the connection establishment has not been successful - connected=false. 
            }
        }

        // This method closes the connection to the device.
        public void DisconnectDevice()
        {
            client.Close();
        }


        public virtual ushort[] getAllRegisters  
        {
            get
            {
                return this.data; 
            }
        }

        public void WriteArray2Reg(ushort index, ushort[] data)
        {
            this.master.WriteMultipleRegisters(index, data);
        }

        public void WriteWord2Reg(ushort index, ushort data)
        {
            this.master.WriteSingleRegister(index, data);
        }

        // Getter/Setter for the IP_Adress, StartAdress, NumofPoints, Sending_interval, Port, Is_connected()
        public virtual string IPAddress    
        {
            get { return this.iPAddress; }
            set { this.iPAddress = value; }
        }

        public virtual ushort StartAdress  
        {
            get { return this.startAdress; }
            set { this.startAdress = value; }
        }

        public virtual ushort NumOfPoints  
        {
            get { return this.numOfPoints; }
            set { this.numOfPoints = value; }
        }

        public virtual int SendingInterval 
        {
            get { return this.sendingInterval; }
            set { this.sendingInterval = value; }
        }

        public virtual int Port 
        {
            get { return this.port; }
            set { this.port = value; }
        }

        public virtual bool isConnected
        {
            get { return this.connected; }
        }

        // Only for the purpose to fulfill the methods from the interface 'INetConnection', which are implemented by 'JetbusConnection'. 
        public T Read<T>(object index)
        {
            throw new NotImplementedException();
        }
        public void Write<T>(object index, T data)
        {
            throw new NotImplementedException();
        }
    }
}

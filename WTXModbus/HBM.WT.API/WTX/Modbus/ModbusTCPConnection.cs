using Modbus.Device;
using System;
using System.Net.Sockets;
using System.Threading;

namespace HBM.WT.API.WTX.Modbus
{

    /// <summary>
    ///     This class establishs the communication to the device(here: WTX120), starts/ends the connection,
    ///     read and write the register and shows the status of the connection and closes the connection to
    ///     the device (here: WTX120).
    ///     Once a button event is clicked in class GUI, an asynchronous call in class WTX120 is started
    ///     and finally in this class "Modbus_TCP" the register (of the device) is read or written.
    ///     The data exchange for reading a register between class "Modbus_TCP" and class "WTX_120" is event-based.
    ///     This class publishes the event (MessageEvent) and read the register, afterwards it will be sent back to WTX120.
    /// </summary>
    public class ModbusTcpConnection : INetConnection //IModbusConnection
    {
        private TcpClient _client;
        private bool _connected;
        private ushort[] _data;
        private string _iPAddress;
        private ModbusIpMaster _master;
        private ushort _numOfPoints;
        private int _port;
        private int _sendingInterval; // Timer1.Interval = Sending Interval 
        private ushort _startAdress;


        public ModbusTcpConnection(string ipAddress)
        {
            _connected = false;
            _port = 502;
            _iPAddress = ipAddress; //IP-address to establish a successful connection to the device

            _numOfPoints = 38;
            _startAdress = 0;
            _sendingInterval = 5; // Timer1.Interval = Sending Interval 
        }


        public virtual ushort[] GetAllRegisters => _data;

        // Getter/Setter for the IP_Adress, StartAdress, NumofPoints, Sending_interval, Port, Is_connected()
        public virtual string IpAddress
        {
            get { return _iPAddress; }
            set { _iPAddress = value; }
        }

        public virtual ushort StartAdress
        {
            get { return _startAdress; }
            set { _startAdress = value; }
        }

        public virtual ushort NumOfPoints
        {
            get { return _numOfPoints; }
            set { _numOfPoints = value; }
        }

        public virtual int SendingInterval
        {
            get { return _sendingInterval; }
            set { _sendingInterval = value; }
        }

        public virtual int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        public bool IsConnected => _connected;

        // Declaration of the event Eventhandler. For the message information from the register.
        // public event EventHandler<MessageEvent<ushort>> RaiseDataEvent;

        public event EventHandler BusActivityDetection;

        public virtual event EventHandler<DataEvent> RaiseDataEvent; // virtual new due to tesing - 3.5.2018


        // This method is called from the device class "WTX120" and calls the method ReadRegisterPublishing(e:MessageEvent)
        // to create a new MessageEvent to read the register of the device. 
        public int Read(object index)
        {
            if (_connected)
                ReadRegisterPublishing(new DataEvent(_data));

            return 0; 
        }

        public void Write(object index, int data)
        {
            _master.WriteSingleRegister((ushort)Convert.ToInt32(index), (ushort)data);

            BusActivityDetection?.Invoke(this, new LogEvent("Data(ushort) have been written successfully to the register"));
        }

        public void WriteArray(ushort index, ushort[] data)
        {
            _master.WriteMultipleRegisters(index, data);

            BusActivityDetection?.Invoke(this, new LogEvent("Data(ushort array) have been written successfully to multiple registers"));
        }

        // This method publishes the event (MessageEvent) and read the register, afterwards the message(from the register) will be sent back to WTX120.  
        // This method is declared as a virtual method to allow derived class to override the event call.
        //protected virtual void ReadRegisterPublishing(MessageEvent<ushort> e)

        public virtual void ReadRegisterPublishing(DataEvent e) // 25.4 Comment : 'virtual' machte hier probleme beim durchlaufen :o 
        {
            // virtual new due to tesing - 3.5.2018
            try
            {
                // Read the data: e.Message's type - ushort[]  
                //e.Args = masterParam.ReadHoldingRegisters(this.StartAdress, this.getNumOfPoints);

                e.Args = _master.ReadHoldingRegisters(StartAdress, NumOfPoints);
                _connected = true;

                BusActivityDetection?.Invoke(this, new LogEvent("Registers have been read"));
            }
            catch (ArgumentException)
            {
                Console.WriteLine("\nNumber of points has to be between 1 and 125.\n");
            }
            catch (InvalidOperationException)
            {
                _connected = false;

                Connect();
                Thread.Sleep(100);
            }

            //this.data = e.Message;
            _data = e.Args;

            // copy of the event to avoid that a race condition is prevented, if the former subscriber directly logs off after the last
            // condition( and after if(handler!=null) ) and before the event is triggered. 

            //RaiseDataEvent?.Invoke(this, e);

            var handler = RaiseDataEvent;

            //If a subscriber exists: 
            if (handler != null) handler(this, e);
        }

        // This method establishs a connection to the device. Therefore an IP address and the port number
        // for the TcpClient is need. The client itself is used for the implementation of the ModbusIpMaster. 
        public void Connect()
        {
            try
            {
                _client = new TcpClient(_iPAddress, _port);
                _master = ModbusIpMaster.CreateIp(_client);
                _connected = true;

                BusActivityDetection?.Invoke(this, new LogEvent("Connection has been established successfully"));
            }
            catch (Exception)
            {
                _connected = false; // If the connection establishment has not been successful - connected=false. 

                BusActivityDetection?.Invoke(this, new LogEvent("Connection has NOT been established successfully"));
            }
        }

        // This method closes the connection to the device.
        public void Disconnect()
        {
            _client.Close();
        }


    }
}
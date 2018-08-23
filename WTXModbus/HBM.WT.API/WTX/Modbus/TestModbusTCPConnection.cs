
namespace HBM.WT.API.WTX.Modbus
{
    using HBM.WT.API;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public enum Behavior
    {

         ConnectionFail, 
         ConnectionSuccess,

         DisconnectionFail,
         DisconnectionSuccess,
         
         ReadFail,
         ReadSuccess,

         WriteFail,
         WriteSuccess,

         WriteSyncFail,
         WriteSyncSuccess,

         WriteArrayFail,
         WriteArraySuccess,

         MeasureZeroFail,
         MeasureZeroSuccess,

         TareFail,
         TareSuccess,

         AsyncWriteBackgroundworkerFail,
         AsyncWriteBackgroundworkerSuccess,

    }

    public class TestModbusTCPConnection : INetConnection, IDisposable
    {
        private Behavior behavior;

        private ushort arrayElement1;
        private ushort arrayElement2;

        private bool _connected;
        public ushort[] _data;
        public int command; 

        public event EventHandler BusActivityDetection;
        public event EventHandler<DataEvent> RaiseDataEvent;

        private string IP;
        private int interval;

        private int numPoints; 

        public TestModbusTCPConnection(Behavior behavior,string ipAddress) 
        {
            _data = new ushort[59];

            this.behavior = behavior;
        }


        public void Connect()
        {
            switch(this.behavior)
            {
                case Behavior.ConnectionFail:
                    _connected = false;
                    break;

                case Behavior.ConnectionSuccess:
                    _connected = true;
                    break;

                default:
                    _connected = false;
                    break; 
            }
    }

        public bool IsConnected
        {
            get
            {
                return this._connected;
            }
            set
            {
                this._connected = value;
            }
        }

        public new void Disconnect()
        {
            switch (this.behavior)
            {
                case Behavior.DisconnectionFail:
                    _connected = true;
                    break;

                case Behavior.DisconnectionSuccess:
                    _connected = false;
                    break;

                default:
                    _connected = true;
                    break;
            }
        }

        public int Read(object index)
        {
            if (_connected)
                ReadRegisterPublishing(new DataEvent(_data));

            return 0;
        }

        public void ReadRegisterPublishing(DataEvent e) // 25.4 Comment : 'virtual' machte hier probleme beim durchlaufen :o 
        {
            // Behavoir : Kann in Standard oder Filler Mode sein, kann unterschiedliche "NumInputs" haben. Dementsprechend abhängig
            // ist die Anzahl der eingelesenen Werte. Erstmal vom einfachen Fall ausgehen! 

            switch (this.behavior)
            {

                case Behavior.MeasureZeroFail:

                    _data[0] = 16995;       // Net value
                    _data[1] = 16995;       // Gross value
                    break;

                case Behavior.MeasureZeroSuccess:

                    _data[0] = 0;       // Net value
                    _data[1] = 0;       // Gross value
                    break;

                case Behavior.ReadFail:

                    // If there is a connection fail, all data attributes get 0 as value.
                    
                    for (int index = 0; index < _data.Length; index++)
                    {
                        _data[index] = 0;
                    }
                    BusActivityDetection?.Invoke(this, new LogEvent("Read failed : Registers have not been read"));
                    
                    break;

                case Behavior.ReadSuccess:

                    // The most important data attributes from the WTX120 device: 

                    _data[0] = 17000;       // Net value
                    _data[1] = 17000;       // Gross value
                    _data[2] = 0;           // General weight error
                    _data[3] = 0;           // Scale alarm triggered
                    _data[4] = 0;           // Limit status
                    _data[5] = 0;           // Weight moving
                    _data[6] = 1;           // Scale seal is open
                    _data[7] = 0;           // Manual tare
                    _data[8] = 0;           // Weight type
                    _data[9] = 0;           // Scale range
                    _data[10] = 0;          // Zero required/True zero
                    _data[11] = 0;          // Weight within center of zero 
                    _data[12] = 0;          // weight in zero range
                    _data[13] = 0;          // Application mode = 0
                    _data[14] = 4;          // Decimal Places
                    _data[15] = 2;          // Unit
                    _data[16] = 0;          // Handshake
                    _data[17] = 0;          // Status

                    BusActivityDetection?.Invoke(this, new LogEvent("Read successful: Registers have been read"));

                    break;

                case Behavior.WriteSyncSuccess:
                    _data[16] = 1;
                    break;

                case Behavior.WriteSyncFail:
                    _data[16] = 0;
                    break;

                default:
                    
                    for (int index = 0; index < _data.Length; index++)
                    {
                        _data[index] = 0;
                    }
                    BusActivityDetection?.Invoke(this, new LogEvent("Read failed : Registers have not been read"));
                    
                    break; 
            }

            var handler = RaiseDataEvent;

            //If a subscriber exists: 
            if (handler != null) handler(this, new DataEvent(_data));
        }

        public int getCommand
        {
            get { return this.command; }
        }

        public void Write(object index, int data)
        {
            command = data;

            switch (this.behavior)
            {
                case Behavior.WriteSyncSuccess:
                    _data[16] = 1;
                    break;

                case Behavior.WriteSyncFail:
                    _data[16] = 0;
                    break;
            }

            switch(this.behavior)
            {
                case Behavior.WriteFail:
                    command = 0;
                    break;

                case Behavior.WriteSuccess:
                    command = 2;
                    break;
            }

            switch (this.behavior)
            {
                case Behavior.WriteSyncFail:
                    command = 0;
                    break;

                case Behavior.WriteSyncSuccess:
                    command = 0x100;
                    break;
            }

        }

        public void WriteArray(ushort index, ushort[] data)
        {

            switch (this.behavior)
            {
                case Behavior.WriteArrayFail:
                    this.arrayElement1 = 0;
                    this.arrayElement2 = 0;

                    break;

                case Behavior.WriteArraySuccess:
                    this.arrayElement1 = data[0];
                    this.arrayElement2 = data[1];

                    break;

                case Behavior.MeasureZeroSuccess:

                    _data[0] = 0;
                    _data[0] = 0; 
                    this.arrayElement1 = data[0];
                    this.arrayElement2 = data[1];

                    break;

                case Behavior.MeasureZeroFail:

                    _data[0] = 1111;
                    _data[0] = 555;
                    this.arrayElement1 = 0;
                    this.arrayElement2 = 0;

                    break;
                default:
                    break; 
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ushort getArrElement1
        {
            get
            {
                return this.arrayElement1;
            }
        }

        public ushort getArrElement2
        {
            get
            {
                return this.arrayElement2;
            }
        }

        public int NumofPoints
        {
            get
            {
                return this.numPoints;
            }
            set
            {
                this.numPoints = value; 
            }
        }

        public bool IsConnection { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        
        public string IpAddress
        {
            get
            {
                return this.IP;
            }
            set
            {
                this.IP = value; 
            }
        }

        public int SendingInterval
        {
            get
            {
                return this.interval;
            }
            set
            {
                this.interval = value;
            }
        }

    }
}

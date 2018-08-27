
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

         HandshakeFail,
         HandshakeSuccess,

         CalibrationFail,
         CalibrationSuccess,

    }

    public class TestModbusTCPConnection : INetConnection, IDisposable
    {
        private Behavior behavior;

        private ushort arrayElement1;
        private ushort arrayElement2;
        private ushort arrayElement3;
        private ushort arrayElement4;

        private bool _connected;
        private ushort[] _datafromWTX;
        public int command; 

        public event EventHandler BusActivityDetection;
        public event EventHandler<DataEvent> RaiseDataEvent;

        private string IP;
        private int interval;

        private int numPoints; 

        public TestModbusTCPConnection(Behavior behavior,string ipAddress) 
        {
            _datafromWTX = new ushort[15];

            this.behavior = behavior;

            this.numPoints = 6;

            for (int index = 0; index < _datafromWTX.Length; index++)
                _datafromWTX[index] = 0x0000; 

            _datafromWTX[0] = 0x0000;
            _datafromWTX[1] = 0x2710;
            _datafromWTX[2] = 0x0000;
            _datafromWTX[3] = 0x2710;
            _datafromWTX[4] = 0x0000;
            _datafromWTX[5] = 0x0000;
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
                ReadRegisterPublishing(new DataEvent(_datafromWTX));

            return 0;
        }

        public void ReadRegisterPublishing(DataEvent e) // 25.4 Comment : 'virtual' machte hier probleme beim durchlaufen :o 
        {
            // Behavoir : Kann in Standard oder Filler Mode sein, kann unterschiedliche "NumInputs" haben. Dementsprechend abhängig
            // ist die Anzahl der eingelesenen Werte. Erstmal vom einfachen Fall ausgehen! 

            switch (this.behavior)
            {

                case Behavior.MeasureZeroFail:

                    // Net value in hexadecimal: 
                    _datafromWTX[0] = 0x00;     
                    _datafromWTX[1] = 0x2710;

                    // Gross value in hexadecimal:
                    _datafromWTX[2] = 0x00;
                    _datafromWTX[3] = 0x2710;
                    break;

                case Behavior.MeasureZeroSuccess:

                    // Net value in hexadecimal: 
                    _datafromWTX[0] = 0x00;
                    _datafromWTX[1] = 0x00;

                    // Gross value in hexadecimal:
                    _datafromWTX[2] = 0x00;
                    _datafromWTX[3] = 0x00;
                    break;

                case Behavior.ReadFail:

                    // If there is a connection fail, all data attributes get 0 as value.
                    
                    for (int index = 0; index < _datafromWTX.Length; index++)
                    {
                        _datafromWTX[index] = 0x0000;
                    }
                    BusActivityDetection?.Invoke(this, new LogEvent("Read failed : Registers have not been read"));
                    
                    break;

                case Behavior.ReadSuccess:

                    // The most important data attributes from the WTX120 device: 

                    _datafromWTX[0] = 0x0000;
                    _datafromWTX[1] = 0x4040;
                    _datafromWTX[2] = 0x0000;
                    _datafromWTX[3] = 0x4040;
                    _datafromWTX[4] = 0x0000;
                    _datafromWTX[5] = 0x0000;
                    
                    BusActivityDetection?.Invoke(this, new LogEvent("Read successful: Registers have been read"));
                    break;


                default:
                    /*
                    for (int index = 0; index < _datafromWTX.Length; index++)
                    {
                        _datafromWTX[index] = 0;
                    }
                    BusActivityDetection?.Invoke(this, new LogEvent("Read failed : Registers have not been read"));
                    */
                    break; 
            }

            RaiseDataEvent?.Invoke(this, new DataEvent(this._datafromWTX));

            /*
            var handler = RaiseDataEvent;

            //If a subscriber exists: 
            if (handler != null) handler(this, new DataEvent(_datafromWTX));
            */
        }

        public int getCommand
        {
            get { return this.command; }
        }

        public void Write(object index, int data)
        {
            
            switch (this.behavior)
            {
                case Behavior.CalibrationFail:
                    command = 0;
                    break;

                case Behavior.CalibrationSuccess:
                    command = data;
                    break;

                case Behavior.WriteSyncSuccess:
                    command = data;
                    _datafromWTX[5] = 0x4040;
                    break;

                case Behavior.WriteSyncFail:
                    command = 0;
                    _datafromWTX[5] = 0x40;
                    break;

                case Behavior.WriteFail:
                    command = 0;
                    break;

                case Behavior.WriteSuccess:
                    command = data;
                    break;

                case Behavior.HandshakeSuccess:
                    // Change the handshake bit : bit .14 from 0 to 1.
                    _datafromWTX[5] = 0x4040;
                   
                    break;

                case Behavior.HandshakeFail:
                    
                    _datafromWTX[5] = 0x40;

                    break;
            }

        }

        public void WriteArray(ushort index, ushort[] data)
        {

            switch (this.behavior)
            {

            case Behavior.CalibrationFail:
                    this.arrayElement1 = 0;
                    this.arrayElement2 = 0;

            break;

                case Behavior.CalibrationSuccess:

                    if ((int)index == 48)       // According to the index 48 (=wordnumber) the preload is written. 
                    {
                        this.arrayElement1 = data[0];
                        this.arrayElement2 = data[1];
                    }
                    else
                    if ((int)index == 50)       // According to the index 50 (=wordnumber) the nominal load is written. 
                    {
                        this.arrayElement3 = data[0];
                        this.arrayElement4 = data[1];
                    }
                        break;

                case Behavior.WriteArrayFail:
                    this.arrayElement1 = 0;
                    this.arrayElement2 = 0;

                    break;

                case Behavior.WriteArraySuccess:
                    this.arrayElement1 = data[0];
                    this.arrayElement2 = data[1];

                    break;

                case Behavior.MeasureZeroSuccess:

                    _datafromWTX[0] = 0;
                    _datafromWTX[0] = 0; 
                    this.arrayElement1 = data[0];
                    this.arrayElement2 = data[1];

                    break;

                case Behavior.MeasureZeroFail:
                    
                    _datafromWTX[0] = 555;
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

        public ushort getArrElement3
        {
            get
            {
                return this.arrayElement3;
            }
        }

        public ushort getArrElement4
        {
            get
            {
                return this.arrayElement4;
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

        public ushort[] getData {

            get
            {
                return this._datafromWTX;
            }
            set
            {
                this._datafromWTX = value; 
            }

        }
    }
}

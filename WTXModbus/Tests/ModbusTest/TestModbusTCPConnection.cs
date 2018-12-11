
namespace HBM.Weighing.API.WTX.Modbus
{
    using HBM.Weighing.API;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

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

         InStandardMode,
         InFillerMode,

         LogEvent_Fail,
         LogEvent_Success,

         t_UnitValue_Fail,
         t_UnitValue_Success,
         kg_UnitValue_Success,
         kg_UnitValue_Fail,
         g_UnitValue_Success,
         g_UnitValue_Fail,
         lb_UnitValue_Success,
         lb_UnitValue_Fail,

         NetGrossValueStringComment_0D_Fail,
         NetGrossValueStringComment_0D_Success,
         NetGrossValueStringComment_1D_Fail,
         NetGrossValueStringComment_1D_Success,
         NetGrossValueStringComment_2D_Fail,
         NetGrossValueStringComment_2D_Success,
         NetGrossValueStringComment_3D_Fail,
         NetGrossValueStringComment_3D_Success,
         NetGrossValueStringComment_4D_Fail,
         NetGrossValueStringComment_4D_Success,
         NetGrossValueStringComment_5D_Fail,
         NetGrossValueStringComment_5D_Success,
         NetGrossValueStringComment_6D_Fail,
         NetGrossValueStringComment_6D_Success,

         ScaleRangeStringComment_Range1_Fail,
         ScaleRangeStringComment_Range1_Success,
         ScaleRangeStringComment_Range2_Fail,
         ScaleRangeStringComment_Range2_Success,
         ScaleRangeStringComment_Range3_Fail,
         ScaleRangeStringComment_Range3_Success,

         LimitStatusStringComment_Case0_Fail,
         LimitStatusStringComment_Case0_Success,
         LimitStatusStringComment_Case1_Fail,
         LimitStatusStringComment_Case1_Success,
         LimitStatusStringComment_Case2_Fail,
         LimitStatusStringComment_Case2_Success,
         LimitStatusStringComment_Case3_Fail,
         LimitStatusStringComment_Case3_Success,

         WeightMovingStringComment_Case0_Fail,
         WeightMovingStringComment_Case0_Success,
         WeightMovingStringComment_Case1_Fail,
         WeightMovingStringComment_Case1_Success,

         WeightTypeStringComment_Case0_Fail,
         WeightTypeStringComment_Case0_Success,
         WeightTypeStringComment_Case1_Fail,
         WeightTypeStringComment_Case1_Success,

         WriteHandshakeTestSuccess,
         WriteHandshakeTestFail,

         GrosMethodTestSuccess,
         GrosMethodTestFail, 

         TareMethodTestSuccess,
         TareMethodTestFail,

         ZeroMethodTestSuccess,
         ZeroMethodTestFail,

         AdjustingZeroMethodSuccess,
         AdjustingZeroMethodFail,

         AdjustNominalMethodTestSuccess,
         AdjustNominalMethodTestFail,

         ActivateDataMethodTestSuccess,
         ActivateDataMethodTestFail,

         ManualTaringMethodTestSuccess,
         ManualTaringMethodTestFail,

         ClearDosingResultsMethodTestSuccess,
         ClearDosingResultsMethodTestFail,

         AbortDosingMethodTestSuccess,
         AbortDosingMethodTestFail,

         StartDosingMethodTestSuccess,
         StartDosingMethodTestFail,
 
         RecordWeightMethodTestSuccess,
         RecordWeightMethodTestFail,

         ManualRedosingMethodTestSuccess,
         ManualRedosingMethodTestFail,

         WriteS32ArrayTestSuccess,
         WriteS32ArrayTestFail,
 
         WriteU16ArrayTestSuccess,
         WriteU16ArrayTestFail,

         ResetTimerTestSuccess,
        //ResetTimerTestFail,

         WriteU08ArrayTestSuccess,
         WriteU08ArrayTestFail,

         UpdateOutputTestSuccess,
         UpdateOutputTestFail,
    }

    public class TestModbusTCPConnection : INetConnection, IDisposable
    {
        private Behavior behavior;

        private ushort arrayElement1;
        private ushort arrayElement2;
        private ushort arrayElement3;
        private ushort arrayElement4;

        private bool _connected;

        private ushort[] _dataWTX;

        public int command; 

        public event EventHandler BusActivityDetection;
        public event EventHandler<DataEvent> RaiseDataEvent;

        private string IP;
        private int interval;
        private int wordNumberIndex; 

        private int numPoints;

        public LogEvent _logObj;

        public TestModbusTCPConnection(Behavior behavior,string ipAddress) 
        {
            _dataWTX = new ushort[38];
            // size of 38 elements for the standard and filler application mode.            

            this.behavior = behavior;

            this.numPoints = 6;

            for (int index = 0; index < _dataWTX.Length; index++)
                _dataWTX[index] = 0x00;

            _dataWTX[0] = 0x00;
            _dataWTX[1] = 0x2710;
            _dataWTX[2] = 0x00;
            _dataWTX[3] = 0x2710;
            _dataWTX[4] = 0x00;
            _dataWTX[5] = 0x00;
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
                    _connected = true;
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

        public void Disconnect()
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
            //if (_connected)
                ReadRegisterPublishing(new DataEvent(_dataWTX, new string[0]));

            return 0;
        }

        public void ReadRegisterPublishing(DataEvent e) 
        {
            // Behavoir : Kann in Standard oder Filler Mode sein, kann unterschiedliche "NumInputs" haben. Dementsprechend abhängig
            // ist die Anzahl der eingelesenen Werte. Erstmal vom einfachen Fall ausgehen! 

            switch (this.behavior)
            {
                case Behavior.WriteHandshakeTestSuccess:

                    if (_dataWTX[4] == 0x0000)
                        _dataWTX[4] = 0x4000;
                    else
                        if (_dataWTX[4] == 0x4000)
                        _dataWTX[4] = 0x0000;
                    break;
            
                case Behavior.InFillerMode:

                    //data word for a application mode being in filler mode: Bit .0-1 = 1 || 2 (2 is the given value for filler mode according to the manual, but actually it is 1.)
                    _dataWTX[5] = 0x1;
                    break;

                case Behavior.InStandardMode:

                    //data word for a application mode being in standard mode, not in filler mode: Bit .0-1 = 0
                    _dataWTX[5] = 0x00;

                    break;

                case Behavior.CalibrationFail:

                    //Handshake bit:

                    if (_dataWTX[5] >> 14 == 0)
                        _dataWTX[5] = 0x4000;
                    else if (_dataWTX[5] >> 14 == 1)
                        _dataWTX[5] = 0x0000;

                    break;

                case Behavior.CalibrationSuccess:

                    //Handshake bit:

                    if (_dataWTX[5] >> 14 == 0)
                        _dataWTX[5] = 0x4000;
                    else if (_dataWTX[5] >> 14 == 1)
                        _dataWTX[5] = 0x0000;

                    break;

                case Behavior.MeasureZeroFail:

                    // Net value in hexadecimal: 
                    _dataWTX[0] = 0x00;     
                    _dataWTX[1] = 0x2710;

                    // Gross value in hexadecimal:
                    _dataWTX[2] = 0x00;
                    _dataWTX[3] = 0x2710;

                    //Handshake bit:
                    if (_dataWTX[5] >> 14 == 0)
                        _dataWTX[5] = 0x4000;

                    else if (_dataWTX[5] >> 14 == 1)
                        _dataWTX[5] = 0x0000;

                    break;

                case Behavior.MeasureZeroSuccess:

                    // Net value in hexadecimal: 
                    _dataWTX[0] = 0x00;
                    _dataWTX[1] = 0x00;

                    // Gross value in hexadecimal:
                    _dataWTX[2] = 0x00;
                    _dataWTX[3] = 0x00;

                    //Handshake bit:
                    if (_dataWTX[5] >> 14 == 0)
                        _dataWTX[5] = 0x4000;
                    else if (_dataWTX[5] >> 14 == 1)
                        _dataWTX[5] = 0x0000; 

                    break;

                case Behavior.ReadFail:

                    // If there is a connection fail, all data attributes get 0 as value.
                    
                    for (int index = 0; index < _dataWTX.Length; index++)
                    {
                        _dataWTX[index] = 0x0000;
                    }

                    _logObj = new LogEvent("Read failed : Registers have not been read");

                    BusActivityDetection?.Invoke(this, _logObj);
                    
                    break;

                case Behavior.ReadSuccess:

                    // The most important data attributes from the WTX120 device: 

                    _dataWTX[0] = 0x0000;
                    _dataWTX[1] = 0x4040;
                    _dataWTX[2] = 0x0000;
                    _dataWTX[3] = 0x4040;
                    _dataWTX[4] = 0x0000;
                    _dataWTX[5] = 0x0000;
                    
                    _logObj = new LogEvent("Read successful: Registers have been read");
                    BusActivityDetection?.Invoke(this, _logObj);
                    break;


                case Behavior.t_UnitValue_Success:
                    _dataWTX[5] = 0x100;
                    break;
                case Behavior.t_UnitValue_Fail:
                    _dataWTX[5] = 0x0000;
                    break;

                case Behavior.kg_UnitValue_Success:
                    _dataWTX[5] = 0x0000;
                    break;

                case Behavior.kg_UnitValue_Fail:
                    _dataWTX[5] = 0xFFFF;
                    break;

                case Behavior.g_UnitValue_Success:
                    _dataWTX[5] = 0x80;
                    break;

                case Behavior.g_UnitValue_Fail:
                    _dataWTX[5] = 0x0000;
                    break;

                case Behavior.lb_UnitValue_Success:
                    _dataWTX[5] = 0x180;
                    break;


                case Behavior.lb_UnitValue_Fail:
                    _dataWTX[5] = 0x0000;
                    break;

                case Behavior.NetGrossValueStringComment_0D_Success:
                    _dataWTX[5] = 0x0000;
                    break;

                case Behavior.NetGrossValueStringComment_1D_Success:
                    _dataWTX[5] = 0x10;
                    break;

                case Behavior.NetGrossValueStringComment_2D_Success:
                    _dataWTX[5] = 0x20;
                    break;

                case Behavior.NetGrossValueStringComment_3D_Success:
                    _dataWTX[5] = 0x30;
                    break;

                case Behavior.NetGrossValueStringComment_4D_Success:
                    _dataWTX[5] = 0x40;
                    break;

                case Behavior.NetGrossValueStringComment_5D_Success:
                    _dataWTX[5] = 0x50;
                    break;

                case Behavior.NetGrossValueStringComment_6D_Success:
                    _dataWTX[5] = 0x60;
                    break;

                // Simulate for testing 'Scale range': 

                case Behavior.ScaleRangeStringComment_Range1_Fail:
                    _dataWTX[4] = 0x200;
                    break;

                case Behavior.ScaleRangeStringComment_Range2_Fail:
                    _dataWTX[4] = 0x0000;
                    break;

                case Behavior.ScaleRangeStringComment_Range3_Fail:
                    _dataWTX[4] = 0x100;
                    break;

                case Behavior.ScaleRangeStringComment_Range1_Success:
                    _dataWTX[4] = 0x0000;
                    break;

                case Behavior.ScaleRangeStringComment_Range2_Success:
                    _dataWTX[4] = 0x100;
                    break;

                case Behavior.ScaleRangeStringComment_Range3_Success:
                    _dataWTX[4] = 0x200;
                    break;

                // Simulate for testing 'Limit status': 

                case Behavior.LimitStatusStringComment_Case0_Fail:
                    _dataWTX[4] = 0xC;
                    break;
                case Behavior.LimitStatusStringComment_Case1_Fail:
                    _dataWTX[4] = 0x8;
                    break;
                case Behavior.LimitStatusStringComment_Case2_Fail:
                    _dataWTX[4] = 0x0000;
                    break;
                case Behavior.LimitStatusStringComment_Case3_Fail:
                    _dataWTX[4] = 0x4;
                    break;

                case Behavior.LimitStatusStringComment_Case0_Success:
                    _dataWTX[4] = 0x0000;
                    break;
                case Behavior.LimitStatusStringComment_Case1_Success:
                    _dataWTX[4] = 0x4;
                    break;
                case Behavior.LimitStatusStringComment_Case2_Success:
                    _dataWTX[4] = 0x8;
                    break;
                case Behavior.LimitStatusStringComment_Case3_Success:
                    _dataWTX[4] = 0xC;
                    break;

                // Simulate for testing 'Weight moving': 
                case Behavior.WeightMovingStringComment_Case0_Fail:
                    _dataWTX[4] = 0x0010;
                    break;
                case Behavior.WeightMovingStringComment_Case1_Fail:
                    _dataWTX[4] = 0x0000;
                    break;
                case Behavior.WeightMovingStringComment_Case0_Success:
                    _dataWTX[4] = 0x0000;
                    break;
                case Behavior.WeightMovingStringComment_Case1_Success:
                    _dataWTX[4] = 0x0010;
                    break;

                // Simulate for testing 'Weight type': 
                case Behavior.WeightTypeStringComment_Case0_Fail:
                    _dataWTX[4] = 0x0080;
                    break;
                case Behavior.WeightTypeStringComment_Case1_Fail:
                    _dataWTX[4] = 0x0000;
                    break;

                case Behavior.WeightTypeStringComment_Case0_Success:
                    _dataWTX[4] = 0x0000;
                    break;
                case Behavior.WeightTypeStringComment_Case1_Success:
                    _dataWTX[4] = 0x0080;
                    break;

                case Behavior.LogEvent_Fail:

                    _logObj = new LogEvent("Read failed : Registers have not been read");
                    BusActivityDetection?.Invoke(this, _logObj);

                    break;

                case Behavior.LogEvent_Success:

                    _logObj = new LogEvent("Read successful: Registers have been read");
                    BusActivityDetection?.Invoke(this, _logObj);

                    break;

                default:
                    /*
                    for (int index = 0; index < _dataWTX.Length; index++)
                    {
                        _dataWTX[index] = 0;
                    }
                    _logObj = new LogEvent("Read failed : Registers have not been read");
                    BusActivityDetection?.Invoke(this, _logObj);
                    */
                    break; 
            }

            RaiseDataEvent?.Invoke(this, new DataEvent(this._dataWTX, new string[0]));

            /*
            var handler = RaiseDataEvent;

            //If a subscriber exists: 
            if (handler != null) handler(this, new DataEvent(_dataWTX));
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
                case Behavior.UpdateOutputTestSuccess:
                    this.command = 0x800;
                    break;

                case Behavior.UpdateOutputTestFail:
                    this.command = 0x00; 
                    break;

                case Behavior.WriteU08ArrayTestSuccess:
                    this.wordNumberIndex = (ushort)index;
                    this.arrayElement1 = (ushort)data;
                    break;

                case Behavior.WriteU08ArrayTestFail:
                    this.wordNumberIndex = 0;
                    this.arrayElement1 = 0;
                    break;
                case Behavior.WriteU16ArrayTestSuccess:
                    this.wordNumberIndex = (ushort)index;
                    this.arrayElement1 = (ushort)data;
                    break;
                case Behavior.WriteU16ArrayTestFail:
                    this.wordNumberIndex = 0;
                    this.arrayElement1 = 0;
                    break;
                case Behavior.GrosMethodTestSuccess:
                    this.command = data;
                    break;
                case Behavior.GrosMethodTestFail:
                    this.command = 0;
                    break;
                case Behavior.TareMethodTestSuccess:
                    this.command = data;
                    break;
                case Behavior.TareMethodTestFail:
                    this.command = 0;
                    break;

                case Behavior.AdjustingZeroMethodSuccess:
                    this.command = data;
                    break;
                case Behavior.AdjustingZeroMethodFail:
                    this.command = 0;
                    break;
                case Behavior.AdjustNominalMethodTestSuccess:
                    this.command = data;
                    break;
                case Behavior.AdjustNominalMethodTestFail:
                    this.command = 0;
                    break;
                case Behavior.ActivateDataMethodTestSuccess:
                    this.command = data;
                    break;
                case Behavior.ActivateDataMethodTestFail:
                    this.command = 0;
                    break;
                case Behavior.ManualTaringMethodTestSuccess:
                    this.command = data;
                    break;
                case Behavior.ManualTaringMethodTestFail:
                    this.command = 0;
                    break;
                case Behavior.ClearDosingResultsMethodTestSuccess:
                    this.command = data;
                    break;
                case Behavior.ClearDosingResultsMethodTestFail:
                    this.command = 0;
                    break;
                case Behavior.AbortDosingMethodTestSuccess:
                    this.command = data;
                    break;
                case Behavior.AbortDosingMethodTestFail:
                    this.command = 0;
                    break;
                case Behavior.StartDosingMethodTestSuccess:
                    command = data;
                    break;
                case Behavior.StartDosingMethodTestFail:
                    this.command = 0;
                    break;
                case Behavior.RecordWeightMethodTestSuccess:
                    this.command = data;
                    break;
                case Behavior.RecordWeightMethodTestFail:
                    this.command = 0;
                    break;
                case Behavior.ManualRedosingMethodTestSuccess:
                    this.command = data;
                    break;
                case Behavior.ManualRedosingMethodTestFail:
                    this.command = 0;
                    break;
                case Behavior.WriteHandshakeTestSuccess:

                    if(_dataWTX[4]==0x0000)
                    {
                        this.command = data;
                        _dataWTX[4] = 0x4000;
                    }
                    else
                    if(_dataWTX[4]==0x4000)
                    {
                        this.command = 0x0;
                        _dataWTX[4] = 0x0000;
                    }
                   
                    break;

                case Behavior.WriteHandshakeTestFail:
                    _dataWTX[4] = 0x0000;                  
                    break;

                case Behavior.InFillerMode:
                    //data word for a application mode being in filler mode: Bit .0-1 = 1 || 2 (2 is the given value for filler mode according to the manual, but actually it is 1.)
                    _dataWTX[5] = 0x1;
                    break;

                case Behavior.InStandardMode:
                    //data word for a application mode being in standard mode, not in filler mode: Bit .0-1 = 0
                    _dataWTX[5] = 0x00;
                    break;

                case Behavior.CalibrationFail:
                    this.command = 0;
                    break;

                case Behavior.CalibrationSuccess:
                    this.command = data;
                    break;

                case Behavior.WriteSyncSuccess:

                    this.command = data;

                    // Change the handshake bit : bit .14 from 0 to 1.
                    if (_dataWTX[5] == 0x0000)
                        _dataWTX[5] = 0x4000;
                    else
                        if (_dataWTX[5] == 0x4000)
                        _dataWTX[5] = 0x0000;

                    break;

                case Behavior.WriteSyncFail:

                    this.command = 0x100;

                    // Change the handshake bit : bit .14 from 0 to 1.
                    if (_dataWTX[5] == 0x0000)
                        _dataWTX[5] = 0x4000;
                    else
                        if (_dataWTX[5] == 0x4000)
                            _dataWTX[5] = 0x0000;

                    break;

                case Behavior.WriteFail:
                    this.command = 0x2;
                    break;

                case Behavior.WriteSuccess:
                    command = 0;
                    break;

                case Behavior.HandshakeSuccess:
                    // Change the handshake bit : bit .14 from 0 to 1.
                    if (_dataWTX[5] == 0x0000)
                        _dataWTX[5] = 0x4000;
                    else
                    if (_dataWTX[5] == 0x4000)
                        _dataWTX[5] = 0x0000;
                    break;

                case Behavior.HandshakeFail:

                    if (_dataWTX[5] == 0x0000)
                        _dataWTX[5] = 0x4000;
                    else
                    if (_dataWTX[5] == 0x4000)
                        _dataWTX[5] = 0x4000;       // Error simulation : Handshake bit is not change to 0. 
                    break;
            }

        }

        public void WriteArray(ushort index, ushort[] data)
        {

            switch (this.behavior)
            {
                case Behavior.UpdateOutputTestSuccess:
                        this.arrayElement1 = data[0];
                        this.arrayElement2 = data[1];
                    break;

                case Behavior.UpdateOutputTestFail:
                    this.arrayElement1 = 0;
                    this.arrayElement2 = 0;
                    break;

                case Behavior.WriteS32ArrayTestSuccess:
                        this.wordNumberIndex = index;
                        this.arrayElement1 = data[0];
                        this.arrayElement2 = data[1];
                    break;

                case Behavior.WriteS32ArrayTestFail:
                        this.wordNumberIndex = 0;
                        this.arrayElement1 = 0;
                        this.arrayElement2 = 0;
                    break;

                case Behavior.CalibrationFail:
                        this.arrayElement1 = 0;
                        this.arrayElement2 = 0;
                   break;

                case Behavior.CalibrationSuccess:

                    if ((int)index == 48 || (int) index== 46)       // According to the index 48 (=wordnumber) the preload is written. 
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

                    _dataWTX[0] = 0;
                    _dataWTX[0] = 0; 
                    this.arrayElement1 = data[0];
                    this.arrayElement2 = data[1];

                    break;

                case Behavior.MeasureZeroFail:
                    
                    _dataWTX[0] = 555;
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

        Dictionary<string, int> INetConnection.getData()
        {
            throw new NotImplementedException();
        }

        public async Task<ushort[]> ReadAsync()
        {
            ushort[] value = new ushort[1];

            return value;
        }

        public async Task<int> WriteAsync(ushort index, ushort commandParam)
        {
            switch (behavior)
            {
                case Behavior.ZeroMethodTestSuccess:
                    this.command = commandParam;
                    break;
                case Behavior.ZeroMethodTestFail:
                    this.command = 0x00;
                    break;
            }

            this.command = commandParam;

            return this.command;
        }

        public int getWordNumber
        {
            get
            {
                return this.wordNumberIndex;
            }
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
                return this._dataWTX;
            }
            set
            {
                this._dataWTX = value; 
            }

        }

        public Dictionary<string, JToken> getDataBuffer => throw new NotImplementedException();
    }
}

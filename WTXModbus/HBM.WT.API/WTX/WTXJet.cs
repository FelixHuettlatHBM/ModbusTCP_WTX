using System;
using System.ComponentModel;
using System.Threading.Tasks;
using HBM.WT.API.WTX.Jet;
using HBM.WT.API.WTX.Modbus;

namespace HBM.WT.API.WTX
{
    // Als Ansatz. Eventuell mit enum oder String-Liste ParameterKeys
    public enum ParameterEnum : uint
    {
        MeasuredValue                = 0x601A/01,
        MeasuredValueStatus          = 0x602001,
        DecimalPoint                 = 0x211003,

        DeviceIdentification         = 0x252001,

    };

    public class WtxJet : BaseWtDevice
    {
        private string[] _dataStrArr;
        private ushort[] _data;
        private INetConnection _connection;
        private bool _dataReceived;
        
        public override event EventHandler<DataEvent> DataUpdateEvent;

        private bool _isCalibrating;

        private double dPreload;
        private double dNominalLoad;
        private double multiplierMv2D;

        public struct ParameterKeys
        {
            public const string MEASURED_VALUE = "601A/01";      // _601A_01 
            
            public const string GROSS_VALUE = "6144/00";
            public const string ZERO_VALUE  = "6142/00";
            public const string TARE_VALUE  = "6143/00";
            public const string DECIMALS   = "6013/01";

            public const string DOSING_COUNTER= "NDS";
            public const string DOSING_STATUS = "SDO";
            public const string DOSING_RESULT = "FRS1";

            public const string WEIGHT_MOVING_DETECTION = "6153/00";
        }


        /*
        public ParameterProperty (INetConnection connection) : base (connection){ }
      
        public override int MeasureValue { get { return m_Connection.Read<int>(ParameterKeys.MesaureValue); } }
        public override int MeasureValueType { get { return m_Connection.Read<int>(ParameterEnum.MeasuredValueStatus.ToString()); } }

        public override string DeviceIdentification {
            get { return m_Connection.Read<string>(ParameterEnum.DeviceIdentification.ToString()); }
            set { m_Connection.Write<string>(ParameterEnum.DeviceIdentification.ToString(), value); }
        }

        public override int DecimalPonit {
            get { return m_Connection.Read<int>(ParameterEnum.DecimalPoint.ToString()); }
            set { m_Connection.Write<int>(ParameterEnum.DecimalPoint.ToString(), value); }
        }

        public int TestValue {
            get;
        }
        
        */

        public WtxJet(INetConnection connection) : base(connection)  // ParameterProperty umändern 
        {
            if(connection is JetBusConnection)
            {
                _connection = (JetBusConnection)connection;
            }

            if (connection is TestJetbusConnection)
            {
                _connection = (TestJetbusConnection)connection;
            }
            
            _dataReceived = false;
            _dataStrArr = new string[59];
            _data = new ushort[59];

            this._isCalibrating = false;

            for (int index = 0; index < 59; index++)
                _data[index] = 0x00;

            this._connection.RaiseDataEvent += this.UpdateEvent;   // Subscribe to the event.
        }


        public override void initialize_timer(int timerInterval)
        {
            throw new NotImplementedException();
        }


        public override bool IsDataReceived
        {
            get
            {
                return this._dataReceived;
            }
            set
            {
                this._dataReceived = value;
            }
        }
        public override void Calibration(ushort command)
        {
        }

        public override void UpdateEvent(object sender, DataEvent e) { }

        
        public override void Async_Call(ushort commandParam, Action<IDeviceData> callbackParam)
        {
           throw new NotImplementedException();
        }

        public override void SyncCall(ushort wordNumber, ushort commandParam, Action<IDeviceData> callbackParam)
        {
            throw new NotImplementedException();
        }

        public override void ReadDoWork(object sender, DoWorkEventArgs doworkAsynchronous)
        {
            throw new NotImplementedException();
        }

        public override IDeviceData AsyncReadData(BackgroundWorker worker)
        {
            throw new NotImplementedException();
        }

        public override IDeviceData SyncReadData()
        {
            throw new NotImplementedException();
        }

        public override void ReadCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
           throw new NotImplementedException();
        }

        public override void WriteDoWork(object sender, DoWorkEventArgs e)
        {
            //throw new NotImplementedException();
        }

        public override void WriteCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //throw new NotImplementedException();
        }
        
        public override ushort[] GetValuesAsync()
        {
            throw new NotImplementedException();
        }

        //public override void UpdateEvent(object sender, MessageEvent<ushort> e) { }

        public override ushort[] GetDataUshort { get { return new ushort[1]; } set { this._data = value;    } }

        public override int NetValue
        {
            get
            {
                return this._connection.Read(ParameterKeys.MEASURED_VALUE);
            }
        }
        
        public override int GrossValue
        {
            get
            {
                return this._connection.Read(ParameterKeys.GROSS_VALUE);        // GrossValue = "6144/00";
            }           
        }

        public override int GeneralWeightError { get { return 1; } }       // data[3]
        public override int ScaleAlarmTriggered { get { return 1; } }      // data[4]
        public override int LimitStatus { get { return 1; } }               // data[5]

        public override int WeightMoving
        {
            get
            {
                return this._connection.Read(ParameterKeys.WEIGHT_MOVING_DETECTION);
            }
        }              // data[6]


        public override int ScaleSealIsOpen { get { return 1; } }         // data[7]
        public override int ManualTare { get { return 1; } }                // data[8]
        public override int WeightType { get { return 1; } }                // data[9]
        public override int ScaleRange { get { return 1; } }                // data[10]
        public override int ZeroRequired { get { return 1; } }              // data[11]
        public override int WeightWithinTheCenterOfZero { get { return 1; } }   // data[12]
        public override int WeightInZeroRange { get { return 1; } }               // data[13]
        public override int ApplicationMode { get { return 1; } }           // data[14]

        public override int Decimals
        {
            get
            {
                return this._connection.Read(ParameterKeys.DECIMALS);   // Decimals = "DPT";
            }
        }     // data[15]

        public override int Unit { get { return 1; }}                       // data[16]
        public override int Handshake { get { return 1; }}                  // data[17]
        public override int Status { get { return 1; }}                     // data[18]

        public override int Input1 { get { return 1; } }            // data[19]    // IS1
        public override int Input2 { get { return 1; } }            // data[20]    // IS2
        public override int Input3 { get { return 1; } }            // data[21]    // IS3
        public override int Input4 { get { return 1; } }            // data[22]    // IS4 
        public override int Output1 { get { return 1; } }           // data[23]    // OS1
        public override int Output2 { get { return 1; } }           // data[24]    // OS2
        public override int Output3 { get { return 1; } }           // data[25]    // OS3
        public override int Output4 { get { return 1; } }           // data[26]    // OS4 

        public override int LimitStatus1 { get { return 1; } }       // data[27]
        public override int LimitStatus2 { get { return 1; } }       // data[28]
        public override int LimitStatus3 { get { return 1; } }       // data[29]
        public override int LimitStatus4 { get { return 1; } }       // data[30]

        public override int WeightMemDay { get { return 1; } }          // data[31]
        public override int WeightMemMonth { get { return 1; } }        // data[32]
        public override int WeightMemYear { get { return 1; } }         // data[33]
        public override int WeightMemSeqNumber { get { return 1; } }   // data[34]
        public override int WeightMemGross { get { return 1; } }        // data[35]
        public override int WeightMemNet { get { return 1; } }          // data[36]

        public override int CoarseFlow { get { return 1; } }                // data[37]
        public override int FineFlow { get { return 1; } }                  // data[38]
        public override int Ready { get { return 1; } }                      // data[39]
        public override int ReDosing { get { return 1; } }                  // data[40]
        public override int Emptying { get { return 1; } }                   // data[41]
        public override int FlowError { get { return 1; } }                 // data[42]
        public override int Alarm { get { return 1; } }                      // data[43]
        public override int AdcOverUnderload { get { return 1; } }     // data[44]
        public override int MaxDosingTime { get { return 1; } }            // data[45]
        public override int LegalTradeOp { get { return 1; } }  // data[46]
        public override int ToleranceErrorPlus { get { return 1; } }       // data[47]
        public override int ToleranceErrorMinus { get { return 1; } }      // data[48]
        public override int StatusInput1 { get { return 1; } }     // data[49]
        public override int GeneralScaleError { get { return 1; } }        // data[50]

        public override int FillingProcessStatus
        {
            get
            {             
                return this._connection.Read(ParameterKeys.DOSING_STATUS);
            }
        }             // data[51]

        public override int NumberDosingResults
        {
            get
            {
                return this._connection.Read(ParameterKeys.DOSING_COUNTER);
            }
        }            // data[52]

        public override int DosingResult
        {
            get
            {
                return this._connection.Read(ParameterKeys.DOSING_RESULT);
            }
        }           // data[53]

        public override int MeanValueDosingResults { get { return 1; } }           // data[54]
        public override int StandardDeviation { get { return 1; } }                // data[55]
        public override int TotalWeight { get { return 1; } }                      // data[56]
        public override int FineFlowCutOffPoint { get { return 1; } }              // data[57]
        public override int CoarseFlowCutOffPoint { get { return 1; } }            // data[58]
        public override int CurrentDosingTime { get { return 1; } }                // data[59]
        public override int CurrentCoarseFlowTime { get { return 1; } }            // data[60]
        public override int CurrentFineFlowTime { get { return 1; } }              // data[61]
        public override int ParameterSetProduct { get { return 1; } }              // data[62]

        public override int ManualTareValue { get; set; }
        public override int LimitValue1Input { get; set; }
        public override int LimitValue1Mode { get; set; }
        public override int LimitValue1ActivationLevelLowerBandLimit { get; set; }
        public override int LimitValue1HysteresisBandHeight { get; set; }

        // Output words for the standard application: Not used so far

        public override int LimitValue2Source { get; set; }
        public override int LimitValue2Mode { get; set; }
        public override int LimitValue2ActivationLevelLowerBandLimit { get; set; }
        public override int LimitValue2HysteresisBandHeight { get; set; }
        public override int LimitValue3Source { get; set; }
        public override int LimitValue3Mode { get; set; }
        public override int LimitValue3ActivationLevelLowerBandLimit { get; set; }
        public override int LimitValue3HysteresisBandHeight { get; set; }
        public override int LimitValue4Source { get; set; }
        public override int LimitValue4Mode { get; set; }
        public override int LimitValue4ActivationLevelLowerBandLimit { get; set; }
        public override int LimitValue4HysteresisBandHeight { get; set; }

        // Output words for the filler application: Not used so far


        public override int ResidualFlowTime { get; set; }
        public override int TargetFillingWeight { get; set; }
        public override int CoarseFlowCutOffPointSet { get; set; }
        public override int FineFlowCutOffPointSet { get; set; }
        public override int MinimumFineFlow { get; set; }
        public override int OptimizationOfCutOffPoints { get; set; }
        public override int MaximumDosingTime { get; set; }
        public override int StartWithFineFlow { get; set; }
        public override int CoarseLockoutTime { get; set; }
        public override int FineLockoutTime { get; set; }
        public override int TareMode { get; set; }
        public override int UpperToleranceLimit { get; set; }
        public override int LowerToleranceLimit { get; set; }
        public override int MinimumStartWeight { get; set; }
        public override int EmptyWeight { get; set; }
        public override int TareDelay { get; set; }
        public override int CoarseFlowMonitoringTime { get; set; }
        public override int CoarseFlowMonitoring { get; set; }
        public override int FineFlowMonitoring { get; set; }
        public override int FineFlowMonitoringTime { get; set; }
        public override int DelayTimeAfterFineFlow { get; set; }
        public override int ActivationTimeAfterFineFlow { get; set; }
        public override int SystematicDifference { get; set; }
        public override int DownardsDosing { get; set; }
        public override int ValveControl { get; set; }
        public override int EmptyingMode { get; set; }



        public override BaseWtDevice GetDeviceAbstract { get; }

        
        public override IDeviceData DeviceValues { get; }

        public override string[] GetDataStr
        {
            get
            {
                return this._dataStrArr;
            }
            set
            {
                this._dataStrArr = value;
            }
        }


        public override INetConnection getModbusConnection => throw new NotImplementedException();

        public override INetConnection getJetBusConnection
        {
            get { return _connection; }
        }
        

        /* 
*In the following methods the different options for the single integer values are used to define and
*interpret the value. Finally a string should be returned from the methods to write it onto the GUI Form. 
*/
        public string NetGrossValueStringComment(int value, int decimals)
        {
            double dvalue = value / Math.Pow(10, decimals);
            string returnvalue = "";

            switch (decimals)
            {
                case 0: returnvalue = dvalue.ToString(); break;
                case 1: returnvalue = dvalue.ToString("0.0"); break;
                case 2: returnvalue = dvalue.ToString("0.00"); break;
                case 3: returnvalue = dvalue.ToString("0.000"); break;
                case 4: returnvalue = dvalue.ToString("0.0000"); break;
                case 5: returnvalue = dvalue.ToString("0.00000"); break;
                case 6: returnvalue = dvalue.ToString("0.000000"); break;
                default: returnvalue = dvalue.ToString(); break;
            }
            return returnvalue;
        }


        public string UnitStringComment()
        {
            switch (this.Unit)
            {
                case 0:
                    return "kg";
                case 1:
                    return "g";
                case 2:
                    return "t";
                case 3:
                    return "lb";
                default:
                    return "error";
            }
        }

        /*
                public string weightMovingStringComment()
                {
                    if (this.weightMoving == 0)
                        return "0=Weight is not moving.";
                    else
                        if (this.weightMoving == 1)
                        return "1=Weight is moving";
                    else
                        return "Error";
                }
                public string limitStatusStringComment()
                {
                    switch (this.limitStatus)
                    {
                        case 0:
                            return "Weight within limits.";
                        case 1:
                            return "W1  U n d e r l o a d.";            // Alternative : "Lower than minimum"
                        case 2:
                            return "W1  O v e r l o a d.";  // Alternative : "Higher than maximum capacity" 
                        case 3:
                            return "Higher than safe load limit.";
                        default:
                            return "Error.";
                    }
                }
                public string weightTypeStringComment()
                {
                    if (this.weightType == 0)
                    {
                        this.isNet = false;
                        return "gross";
                    }
                    else
                        if (this.weightType == 1)
                    {
                        this.isNet = true;
                        return "net";
                    }
                    else

                        return "error";
                }
                public string scaleRangeStringComment()
                {
                    switch (this.scaleRange)
                    {
                        case 0:
                            return "Range 1";
                        case 1:
                            return "Range 2";
                        case 2:
                            return "Range 3";
                        default:
                            return "error";
                    }
                }
                public string applicationModeStringComment()
                {
                    if (this.applicationMode == 0)
                        return "Standard";
                    else

                        if (this.applicationMode == 2 || this.applicationMode == 1)  // Will be changed to '2', so far '1'. 
                        return "Filler";
                    else

                        return "error";
                }
        */

        public string StatusStringComment()
        {
            if (this.Status == 1)
                return "Execution OK!";
            else
                if (this.Status != 1)
                return "Execution not OK!";
            else
                return "error.";

        }
        public override void Disconnect(Action<bool> DisconnectCompleted)
        {
            throw new NotImplementedException();
        }

        public override bool isConnected
        {
            get
            {
                return _connection.IsConnected;
            }
            set
            {
                _connection.IsConnected = value;
            }
        }



        public override void Connect(Action<bool> completed, double timeoutMs)
        {
            //_connection.ConnectOnPeer((int)timeoutMs);
            _connection.Connect();
        }


        // This method sets the value for the nominal weight in the WTX.
        public void Calibrate(int calibrationValue, string calibrationWeightStr)
        {
            calibrationValue = 1100000;

            _connection.Write("6152/00", calibrationValue);

            this._isCalibrating = true;
        }

        private void Write_DataReceived(IDeviceData obj)
        {
            //throw new NotImplementedException();
        }


        public double getDPreload
        {
            get
            {
                return dPreload;
            }
        }

        public double getDNominalLoad
        {
            get
            {
                return dNominalLoad;
            }
        }


        // Calculates the values for deadload and nominal load in d from the inputs in mV/V
        // and writes the into the WTX registers.
        public void Calculate(double preload, double capacity)
        {          
            dPreload = 0;
            dNominalLoad = 0;

            multiplierMv2D = 500000; //   2 / 1000000; // 2mV/V correspond 1 million digits (d)

            dPreload = preload * multiplierMv2D;
            dNominalLoad = dPreload + (capacity * multiplierMv2D);
            

            // write path 6112/01 - scale minimum dead load         

            _connection.Write("6112/01",Convert.ToInt32(preload/*dPreload*/));

            /*
            //write reg 48, DPreload;     
            this.WriteOutputWordS32(Convert.ToInt32(dPreload), 48, Write_DataReceived);
            this.SyncCall(0, 0x80, Write_DataReceived);
            */

            // write path 6113/01 - scale maximum capacity        

            _connection.Write("6113/01", Convert.ToInt32(capacity /*dNominalLoad*/));

            /*
            //write reg 50, DNominalLoad; 
            this.WriteOutputWordS32(Convert.ToInt32(dNominalLoad), 50, Write_DataReceived);
            this.SyncCall(0, 0x100, Write_DataReceived);
            */

            this._isCalibrating = true;
            
        }

        public void MeasureZero()
        {
            //write "calz" 0x7A6C6163 ( 2053923171 ) to path(ID)=6002/01

            _connection.Write("6002/01", 2053923171);

            /*
            this.WriteOutputWordS32(0x7FFFFFFF, 48, Write_DataReceived);         
            this.SyncCall(0, 0x80, Write_DataReceived);
            */
        }

        public void zeroing()
        {
            _connection.Write("6002/01", 1869768058);
        }

        public void gross()
        {
            _connection.Write("6002/01", 1936683623);
        }

        public void taring()
        {
            _connection.Write("6002/01", 1701994868);
        }

    }
}

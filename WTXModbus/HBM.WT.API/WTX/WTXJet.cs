using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using HBM.WT.API.WTX.Jet;
using HBM.WT.API.WTX.Modbus;

namespace HBM.WT.API.WTX
{
    public class WtxJet : BaseWtDevice
    {
        private INetConnection _connection;
        private bool _dataReceived;
        
        public override event EventHandler<DataEvent> DataUpdateEvent;

        private bool _isCalibrating;

        private double dPreload;
        private double dNominalLoad;
        private double multiplierMv2D;

        private string[] _dataStrArr;
        private ushort[] _dataUshort;

        private int _ID_value;

        public struct ID_keys
        {
            public const string NET_VALUE =   "601A/01";        
            public const string GROSS_VALUE = "6144/00";

            public const string ZERO_VALUE  = "6142/00";
            public const string TARE_VALUE  = "6143/00";

            public const string DECIMALS   = "6013/01";
            public const string DOSING_COUNTER= "NDS";
            public const string DOSING_STATUS = "SDO";
            public const string DOSING_RESULT = "FRS1";           

            public const string WEIGHING_DEVICE_1_WEIGHT_STATUS = "6012/01";

            public const string SCALE_COMMAND = "6002/01";

            public const string LDW_DEAD_WEIGHT   = "2110/06";
            public const string LWT_NOMINAL_VALUE = "2110/07";

            public const string LFT_SCALE_CALIBRATION_WEIGHT = "6152/00";

            public const string UNIT_PREFIX_FIXED_PARAMETER = "6014/01";

        }

        public struct command_values
        {
            public const int CALIBRATE_ZERO = 2053923171;
            public const int CALIBRATE_NOMINAL_WEIGHT = 1852596579;
            public const int CALIBRATE_EXIT = 1953069157;
            public const int TARING = 1701994868;
            public const int PEAK = 1801545072;
            public const int ZEROING = 1869768058;
            public const int GROSS = 1936683623;
        }

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
            _dataStrArr = new string[185];
            _dataUshort = new ushort[185];
            _ID_value = 0;

            for(int index=0; index < _dataStrArr.Length; index++)
            {
                _dataStrArr[index] = "";
                _dataUshort[index] = 0; 
            }

            this._isCalibrating = false;

            this._connection.RaiseDataEvent += this.UpdateEvent;   // Subscribe to the event.
        }


        public override void UpdateEvent(object sender, DataEvent e)
        {
            // values from _mTokenBuffer as an array: 

            this._dataStrArr = new string[e.strArgs.Length];

            this._dataReceived = true;

            // Do something with the data, like in the class WTXModbus.cs           
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


        public string[] GetDataString
        {
            get
            {
                return this._dataStrArr;
            }
        }

        public override int NetValue
        {
            get
            {
                return this._connection.Read(ID_keys.NET_VALUE);     // Net value = measured value = "601A/01"
            }
        }
        
        public override int GrossValue
        {
            get
            {
                return this._connection.Read(ID_keys.GROSS_VALUE);        // GrossValue = "6144/00";
            }           
        }

        public override int Decimals
        {
            get
            {
                return this._connection.Read(ID_keys.DECIMALS);   // Decimals = "DPT";
            }
        }

        public override int FillingProcessStatus
        {
            get
            {
                return this._connection.Read(ID_keys.DOSING_STATUS);
            }
        }

        public override int NumberDosingResults
        {
            get
            {
                return this._connection.Read(ID_keys.DOSING_COUNTER);
            }
        }

        public override int DosingResult
        {
            get
            {
                return this._connection.Read(ID_keys.DOSING_RESULT);
            }
        }

        public override int GeneralWeightError
        {
            get
            {
                int _ID_value = this._connection.Read(ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS);
                return (_ID_value & 0x1);
            }
        }

        public override int ScaleAlarmTriggered
        {
            get
            {
                _ID_value = this._connection.Read(ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS);
                return (_ID_value & 0x2) >> 1;
            }
        }

        public override int LimitStatus
        {
            get
            {
                _ID_value = this._connection.Read(ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS);
                return (_ID_value & 0xC) >> 2;
            }
        }

        public override int WeightMoving
        {
            get
            {
                _ID_value = this._connection.Read(ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS);
                return (_ID_value & 0x10) >> 4;
            }
        }

        public override int ScaleSealIsOpen
        {
            get
            {
                _ID_value = this._connection.Read(ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS);
                return (_ID_value & 0x20) >> 5;
            }
        }

        public override int ManualTare
        {
            get
            {
                _ID_value = this._connection.Read(ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS);
                return (_ID_value & 0x40) >> 6;
            }
        }
        public override int WeightType
        {
            get
            {
                _ID_value = this._connection.Read(ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS);
                return (_ID_value & 0x80) >> 7;
            }
        }

        public override int ScaleRange
        {
            get
            {
                _ID_value = this._connection.Read(ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS);
                return (_ID_value & 0x300) >> 8;
            }
        }

        public override int ZeroRequired
        {
            get
            {
                _ID_value = this._connection.Read(ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS);
                return (_ID_value & 0x400) >> 10;
            }
        }

        public override int WeightWithinTheCenterOfZero
        {
            get
            {
                _ID_value = this._connection.Read(ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS);
                return (_ID_value & 0x800) >> 11;
            }
        }

        public override int WeightInZeroRange
        {
            get
            {
                _ID_value = this._connection.Read(ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS);
                return (_ID_value & 0x1000) >> 12;
            }
        }

        public override int Unit
        {
            get
            {
                _ID_value = this._connection.Read(ID_keys.UNIT_PREFIX_FIXED_PARAMETER);
                return (_ID_value & 0xFF0000)>> 16;
            }
        }


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
                case 0:  returnvalue = dvalue.ToString(); break;
                case 1:  returnvalue = dvalue.ToString("0.0"); break;
                case 2:  returnvalue = dvalue.ToString("0.00"); break;
                case 3:  returnvalue = dvalue.ToString("0.000"); break;
                case 4:  returnvalue = dvalue.ToString("0.0000"); break;
                case 5:  returnvalue = dvalue.ToString("0.00000"); break;
                case 6:  returnvalue = dvalue.ToString("0.000000"); break;
                default: returnvalue = dvalue.ToString(); break;
            }
            return returnvalue;
        }


        public string UnitStringComment()
        {
            switch (this.Unit)
            {
                case 0x02:
                    return "kg";
                case 0x4B:
                    return "g";
                case 0x4C:
                    return "t";
                case 0XA6:
                    return "lb";
                default:
                    return "error";
            }
        }

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
            _connection.Write(ID_keys.LFT_SCALE_CALIBRATION_WEIGHT, calibrationValue);          // LFT_SCALE_CALIBRATION_WEIGHT = "6152/00" 

            _connection.Write(ID_keys.SCALE_COMMAND, command_values.CALIBRATE_NOMINAL_WEIGHT);  // CALIBRATE_NOMINAL_WEIGHT = 1852596579 // SCALE_COMMAND = "6002/01"
                       
            this._isCalibrating = true;
        }

        private void Write_DataReceived(IDeviceData obj)
        {
            // callback function : Do something 
        }


        public override ushort[] GetDataUshort
        {
            get
            {
                return this._dataUshort;
            }

            set
            {
                this._dataUshort = value;
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


            // write path 2110/06 - dead load = LDW_DEAD_WEIGHT 

            _connection.Write(ID_keys.LDW_DEAD_WEIGHT, Convert.ToInt32(dPreload));         // Zero point = LDW_DEAD_WEIGHT= "2110/06"

            // write path 2110/07 - capacity/span = Nominal value = LWT_NOMINAL_VALUE        

            _connection.Write(ID_keys.LWT_NOMINAL_VALUE, Convert.ToInt32(dNominalLoad));    // Nominal value = LWT_NOMINAL_VALUE = "2110/07" ; 

            this._isCalibrating = true;           
        }

        public void MeasureZero()
        {
            //write "calz" 0x7A6C6163 ( 2053923171 ) to path(ID)=6002/01

            _connection.Write(ID_keys.SCALE_COMMAND, command_values.CALIBRATE_ZERO);       // SCALE_COMMAND = "6002/01"
        }

        public void zeroing(Action<IDeviceData> WriteDataCompleted)
        {
            _connection.Write(ID_keys.SCALE_COMMAND, command_values.ZEROING);       // SCALE_COMMAND = "6002/01"
        }

        public void gross(Action<IDeviceData> WriteDataCompleted)
        {
            _connection.Write(ID_keys.SCALE_COMMAND, command_values.GROSS);       // SCALE_COMMAND = "6002/01"
        }

        public void taring(Action<IDeviceData> WriteDataCompleted)
        {
            _connection.Write(ID_keys.SCALE_COMMAND, command_values.TARING);       // SCALE_COMMAND = "6002/01"
        }

        /*
        // Input values : To implement these you have to get the ID's from the manual and set them like:
        // this._connection.Read(ParameterKeys.GROSS_VALUE);
        */

        public override int ApplicationMode { get { return 1; } }                                  
        public override int Handshake { get { return 1; } }
        public override int Status { get { return 1; } }

        public override int Input1 { get { return 1; } }           
        public override int Input2 { get { return 1; } }          
        public override int Input3 { get { return 1; } }          
        public override int Input4 { get { return 1; } }           
        public override int Output1 { get { return 1; } }          
        public override int Output2 { get { return 1; } }         
        public override int Output3 { get { return 1; } }          
        public override int Output4 { get { return 1; } }         

        public override int LimitStatus1 { get { return 1; } }   
        public override int LimitStatus2 { get { return 1; } }    
        public override int LimitStatus3 { get { return 1; } }   
        public override int LimitStatus4 { get { return 1; } }  

        public override int WeightMemDay { get { return 1; } }      
        public override int WeightMemMonth { get { return 1; } }    
        public override int WeightMemYear { get { return 1; } }      
        public override int WeightMemSeqNumber { get { return 1; } }
        public override int WeightMemGross { get { return 1; } }       
        public override int WeightMemNet { get { return 1; } }          

        public override int CoarseFlow { get { return 1; } } 
        public override int FineFlow { get { return 1; } }  
        public override int Ready { get { return 1; } }    
        public override int ReDosing { get { return 1; } } 
        public override int Emptying { get { return 1; } } 
        public override int FlowError { get { return 1; } } 
        public override int Alarm { get { return 1; } }   
        public override int AdcOverUnderload { get { return 1; } }    
        public override int MaxDosingTime { get { return 1; } }     
        public override int LegalTradeOp { get { return 1; } }  
        public override int ToleranceErrorPlus { get { return 1; } }  
        public override int ToleranceErrorMinus { get { return 1; } } 
        public override int StatusInput1 { get { return 1; } }   
        public override int GeneralScaleError { get { return 1; } }         

        public override int MeanValueDosingResults { get { return 1; } } 
        public override int StandardDeviation { get { return 1; } }     
        public override int TotalWeight { get { return 1; } }          
        public override int FineFlowCutOffPoint { get { return 1; } }  
        public override int CoarseFlowCutOffPoint { get { return 1; } }
        public override int CurrentDosingTime { get { return 1; } }    
        public override int CurrentCoarseFlowTime { get { return 1; } }
        public override int CurrentFineFlowTime { get { return 1; } }   
        public override int ParameterSetProduct { get { return 1; } }   

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

        public override IDeviceData DeviceValues { get; }
    }
}

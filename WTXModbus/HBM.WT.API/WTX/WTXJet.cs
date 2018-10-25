// <copyright file="WTXJet.cs" company="Hottinger Baldwin Messtechnik GmbH">
//
// HBM.WT.API, a library to communicate with HBM weighing technology devices  
//
// The MIT License (MIT)
//
// Copyright (C) Hottinger Baldwin Messtechnik GmbH
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// </copyright>

using System;
using HBM.WT.API.WTX.Jet;

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
            public const string NET_VALUE = "601A/01";
            public const string GROSS_VALUE = "6144/00";

            public const string ZERO_VALUE = "6142/00";
            public const string TARE_VALUE = "6143/00";

            public const string DECIMALS = "6013/01";
            public const string DOSING_COUNTER = "NDS";
            public const string DOSING_STATUS = "SDO";
            public const string DOSING_RESULT = "FRS1";

            public const string WEIGHING_DEVICE_1_WEIGHT_STATUS = "6012/01";

            public const string SCALE_COMMAND = "6002/01";
            public const string SCALE_COMMAND_STATUS = "6002/02";

            public const string LDW_DEAD_WEIGHT = "2110/06";
            public const string LWT_NOMINAL_VALUE = "2110/07";

            public const string LFT_SCALE_CALIBRATION_WEIGHT = "6152/00";

            public const string UNIT_PREFIX_FIXED_PARAMETER = "6014/01";

            public const string FUNCTION_DIGITAL_INPUT_1 = "IM1";
            public const string FUNCTION_DIGITAL_INPUT_2 = "IM2";
            public const string FUNCTION_DIGITAL_INPUT_3 = "IM3";
            public const string FUNCTION_DIGITAL_INPUT_4 = "IM4";

            public const string FUNCTION_DIGITAL_OUTPUT_1 = "OM1";
            public const string FUNCTION_DIGITAL_OUTPUT_2 = "OM2";
            public const string FUNCTION_DIGITAL_OUTPUT_3 = "OM3";
            public const string FUNCTION_DIGITAL_OUTPUT_4 = "OM4";

            public const string STATUS_DIGITAL_OUTPUT_1 = "OS1";
            public const string STATUS_DIGITAL_OUTPUT_2 = "OS2";
            public const string STATUS_DIGITAL_OUTPUT_3 = "OS3";
            public const string STATUS_DIGITAL_OUTPUT_4 = "OS4";

            public const string COARSE_FLOW_TIME = "CFT";
            public const string FINE_FLOW_TIME = "FFT";
            public const string TARE_MODE = "TMD";
            public const string UPPER_TOLERANCE_LIMIT = "UTL";
            public const string LOWER_TOLERANCE_LOMIT = "LTL";
            public const string MINIMUM_START_WEIGHT = "MSW";
            public const string EMPTY_WEIGHT = "EWT";
            public const string TARE_DELAY = "TAD";
            public const string COARSE_FLOW_MONITORING_TIME = "CBT";
            public const string COARSE_FLOW_MONITORING = "CBK";
            public const string FINE_FLOW_MONITORING = "FBK";
            public const string FINE_FLOW_MONITORING_TIME = "FBT";
            public const string SYSTEMATIC_DIFFERENCE = "SYD";
            public const string VALVE_CONTROL = "VCT";
            public const string EMPTYING_MODE = "EMD";
            public const string COARSE_FLOW_CUT_OFF_POINT = "CFD";
            public const string FINE_FLOW_CUT_OFF_POINT = "FFD";
            public const string MEAN_VALUE_DOSING_RESULTS = "SDM";
            public const string STANDARD_DEVIATION = "SDS";
            public const string RESIDUAL_FLOW_TIME = "RFT";

            public const string MAXIMUM_DOSING_TIME = "MDT";
            public const string MINIMUM_FINE_FLOW = "FFM";
            public const string OPTIMIZATION = "OSN";

            public const string FINEFLOW_PHASE_BEFORE_COARSEFLOW = "FFL";
            public const string DELAY1_DOSING = "DL1";

            public const string LIMIT_VALUE = "2020/25";
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


        public WtxJet(INetConnection connectionParameter) : base(connectionParameter)  // ParameterProperty umändern 
        {
            _connection = connectionParameter;
            
            _dataReceived = false;
            _dataStrArr = new string[185];
            _dataUshort = new ushort[185];
            _ID_value = 0;

            for (int index = 0; index < _dataStrArr.Length; index++)
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

            DataUpdateEvent?.Invoke(this, e);

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
        
        public override int NetValue
        {
            get
            {
                return _connection.getData()[ID_keys.NET_VALUE];
            }
        }

        public override int GrossValue
        {
            get
            {
                return _connection.getData()[ID_keys.GROSS_VALUE];
                // Alternative : return Convert.ToInt32(_connection.getData()[ID_keys.GROSS_VALUE]);
            }
        }

        public override int Decimals
        {
            get
            {
                return _connection.getData()[ID_keys.DECIMALS];
                // Alternative : return Convert.ToInt32(_connection.getDataBuffer[ID_keys.DECIMALS]);
            }
        }

        public override int FillingProcessStatus
        {
            get
            {
                return _connection.getData()[ID_keys.DOSING_STATUS];
                // Alternative : return Convert.ToInt32(_connection.getDataBuffer[ID_keys.DOSING_STATUS]);
            }
        }

        public override int NumberDosingResults
        {
            get
            {
                return _connection.getData()[ID_keys.DOSING_COUNTER];
            }
        }

        public override int DosingResult
        {
            get
            {
                return _connection.getData()[ID_keys.DOSING_RESULT];
            }
        }

        public override int GeneralWeightError
        {
            get
            {
                _ID_value = _connection.getData()[ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS];
                return (_ID_value & 0x1);
            }
        }

        public override int ScaleAlarmTriggered
        {
            get
            {
                _ID_value = _connection.getData()[ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS];
                return (_ID_value & 0x2) >> 1;
            }
        }

        public override int LimitStatus
        {
            get
            {
                _ID_value = _connection.getData()[ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS];
                return (_ID_value & 0xC) >> 2;
            }
        }

        public override int WeightMoving
        {
            get
            {
                _ID_value = _connection.getData()[ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS];
                return (_ID_value & 0x10) >> 4;
            }
        }

        public override int ScaleSealIsOpen
        {
            get
            {
                _ID_value = _connection.getData()[ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS];
                return (_ID_value & 0x20) >> 5;
            }
        }

        public override int ManualTare
        {
            get
            {
                _ID_value = _connection.getData()[ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS];
                return (_ID_value & 0x40) >> 6;
            }
        }
        public override int WeightType
        {
            get
            {
                _ID_value = _connection.getData()[ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS];
                return (_ID_value & 0x80) >> 7;
            }
        }

        public override int ScaleRange
        {
            get
            {
                _ID_value = _connection.getData()[ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS];
                return (_ID_value & 0x300) >> 8;
            }
        }

        public override int ZeroRequired
        {
            get
            {
                _ID_value = _connection.getData()[ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS];
                return (_ID_value & 0x400) >> 10;
            }
        }

        public override int WeightWithinTheCenterOfZero
        {
            get
            {
                _ID_value = _connection.getData()[ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS];
                return (_ID_value & 0x800) >> 11;
            }
        }

        public override int WeightInZeroRange
        {
            get
            {
                _ID_value = _connection.getData()[ID_keys.WEIGHING_DEVICE_1_WEIGHT_STATUS];
                return (_ID_value & 0x1000) >> 12;
            }
        }

        public override int Unit
        {
            get
            {
                _ID_value = _connection.getData()[ID_keys.UNIT_PREFIX_FIXED_PARAMETER];
                return (_ID_value & 0xFF0000) >> 16;
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
        public override string NetGrossValueStringComment(int value, int decimals)
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


        public string UnitStringComment(int unitParam)
        {
            switch (unitParam)
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

        public string StatusStringComment(int statusParam)
        {
            switch(statusParam)
            {
                case 1801543519:
                    return "Execution OK!";

                case 1634168417:
                    return "Execution on go!";

                case 826629983:
                    return "Error 1, E1";

                case 843407199:
                    return "Error 2, E2";

                case 860184415:
                    return "Error 3, E3";

                default:
                    return "Invalid status";
            }
        }

        public override void Disconnect(Action<bool> DisconnectCompleted)
        {
            _connection.Disconnect();
        }

        public override bool isConnected
        {
            get
            {
                return _connection.IsConnected;
            }
        }

        public override void Connect(Action<bool> completed, double timeoutMs)
        {
            _connection.Connect();
        }


        // This method sets the value for the nominal weight in the WTX.
        public override void Calibrate(int calibrationValue, string calibrationWeightStr)
        {
            _connection.Write(ID_keys.LFT_SCALE_CALIBRATION_WEIGHT, calibrationValue);          // LFT_SCALE_CALIBRATION_WEIGHT = "6152/00" 

            _connection.Write(ID_keys.SCALE_COMMAND, command_values.CALIBRATE_NOMINAL_WEIGHT);  // CALIBRATE_NOMINAL_WEIGHT = 1852596579 // SCALE_COMMAND = "6002/01"

            this._isCalibrating = true;
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
        public override void Calculate(double preload, double capacity)
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

        public override void MeasureZero()
        {
            //write "calz" 0x7A6C6163 ( 2053923171 ) to path(ID)=6002/01

            _connection.Write(ID_keys.SCALE_COMMAND, command_values.CALIBRATE_ZERO);       // SCALE_COMMAND = "6002/01"
        }

        public override void zeroing(Action<IDeviceData> WriteDataCompleted)
        {
            _connection.Write(ID_keys.SCALE_COMMAND, command_values.ZEROING);       // SCALE_COMMAND = "6002/01"
        }

        public override void gross(Action<IDeviceData> WriteDataCompleted)
        {
            _connection.Write(ID_keys.SCALE_COMMAND, command_values.GROSS);       // SCALE_COMMAND = "6002/01"
        }

        public override void taring(Action<IDeviceData> WriteDataCompleted)
        {
            _connection.Write(ID_keys.SCALE_COMMAND, command_values.TARING);       // SCALE_COMMAND = "6002/01"
        }

        public override string UnitStringComment()
        {
            return "t";
        }


        /*
        // Input values : To implement these you have to get the ID's from the manual and set them like:
        // this._connection.Read(ParameterKeys.GROSS_VALUE);
        */

        public override int Input1
        {
            get
            {
                return _connection.getData()[ID_keys.FUNCTION_DIGITAL_INPUT_1];
            }
        }         // ID = IM1

        public override int Input2
        {
            get
            {
                return _connection.getData()[ID_keys.FUNCTION_DIGITAL_INPUT_2];
            }
        }         // ID = IM2

        public override int Input3
        {
            get
            {
                return _connection.getData()[ID_keys.FUNCTION_DIGITAL_INPUT_3];
            }
        }         // ID = IM3        

        public override int Input4
        {
            get
            {
                return _connection.getData()[ID_keys.FUNCTION_DIGITAL_INPUT_4];
            }
        }         // ID = IM4          

        public override int Output1
        {
            get
            {
                return _connection.getData()[ID_keys.FUNCTION_DIGITAL_OUTPUT_1];
            }
        }        // ID = OM1

        public override int Output2
        {
            get
            {
                return _connection.getData()[ID_keys.FUNCTION_DIGITAL_OUTPUT_2];
            }
        }        // ID = OM2 

        public override int Output3
        {
            get
            {
                return _connection.getData()[ID_keys.FUNCTION_DIGITAL_OUTPUT_3];
            }
        }        // ID = OM3

        public override int Output4
        {
            get
            {
                return _connection.getData()[ID_keys.FUNCTION_DIGITAL_OUTPUT_4];
            }
        }        // ID = OM4

        public override int LimitStatus1
        {
            get
            {
                return _connection.getData()[ID_keys.STATUS_DIGITAL_OUTPUT_1];
            }
        }   // ID = OS1 

        public override int LimitStatus2
        {
            get
            {
                return _connection.getData()[ID_keys.STATUS_DIGITAL_OUTPUT_2];
            }
        }   // ID = OS2

        public override int LimitStatus3
        {
            get
            {
                return _connection.getData()[ID_keys.STATUS_DIGITAL_OUTPUT_3];
            }
        }   // ID = OS3

        public override int LimitStatus4
        {
            get
            {
                return _connection.getData()[ID_keys.STATUS_DIGITAL_OUTPUT_4];
            }
        }   // ID = OS4

        public override int MaxDosingTime
        {
            get
            {
                return _connection.getData()[ID_keys.MAXIMUM_DOSING_TIME];
            }
        } // MDT

        public override int MeanValueDosingResults
        {
            get
            {
                return _connection.getData()[ID_keys.MEAN_VALUE_DOSING_RESULTS];
            }
        }    // SDM
        public override int StandardDeviation
        {
            get
            {
                return _connection.getData()[ID_keys.STANDARD_DEVIATION];
            }
        }         // SDS 

        public override int FineFlowCutOffPoint
        {
            get
            {
                return _connection.getData()[ID_keys.FINE_FLOW_CUT_OFF_POINT];
            }
        }       // FFD
        public override int CoarseFlowCutOffPoint
        {
            get
            {
                return _connection.getData()[ID_keys.COARSE_FLOW_CUT_OFF_POINT];
            }
        }     // CFD

        public override int ResidualFlowTime
        {
            get
            {
                return _connection.getData()[ID_keys.RESIDUAL_FLOW_TIME];
            }
            set { throw new NotImplementedException(); }
        }    // RFT

        public override int MinimumFineFlow
        {
            get
            {
                return _connection.getData()[ID_keys.MINIMUM_FINE_FLOW];
            }
            set { throw new NotImplementedException(); }
        }     //FFM
        public override int OptimizationOfCutOffPoints
        {
            get
            {
                return _connection.getData()[ID_keys.OPTIMIZATION];
            }
            set { throw new NotImplementedException(); }
        }   // OSN
        public override int MaximumDosingTime
        {
            get
            {
                return _connection.getData()[ID_keys.STATUS_DIGITAL_OUTPUT_3];
            }
            set { throw new NotImplementedException(); }
        }   // MDT

        public override int CoarseLockoutTime
        {
            get
            {
                return _connection.getData()[ID_keys.COARSE_FLOW_TIME];
            }
            set { throw new NotImplementedException(); }
        }    // CFT
        public override int FineLockoutTime
        {
            get
            {
                return _connection.getData()[ID_keys.FINE_FLOW_TIME];
            }
            set { throw new NotImplementedException(); }
        }      // Fine flow time = FFT
        public override int TareMode
        {
            get
            {
                return _connection.getData()[ID_keys.TARE_MODE];
            }
            set { throw new NotImplementedException(); }
        }             // ID = TMD 

        public override int UpperToleranceLimit
        {
            get
            {
                return _connection.getData()[ID_keys.UPPER_TOLERANCE_LIMIT];
            }
            set { throw new NotImplementedException(); }
        }      // UTL
        public override int LowerToleranceLimit
        {
            get
            {
                return _connection.getData()[ID_keys.LOWER_TOLERANCE_LOMIT];
            }
            set { throw new NotImplementedException(); }
        }      // LTL

        public override int MinimumStartWeight
        {
            get
            {
                return _connection.getData()[ID_keys.MINIMUM_START_WEIGHT];
            }
            set { throw new NotImplementedException(); }
        }        // MSW
        public override int EmptyWeight
        {
            get
            {
                return _connection.getData()[ID_keys.EMPTY_WEIGHT];
            }
            set { throw new NotImplementedException(); }
        }  // EWT

        public override int TareDelay
        {
            get
            {
                return _connection.getData()[ID_keys.TARE_DELAY];
            }
            set { throw new NotImplementedException(); }
        }    // TAD

        public override int CoarseFlowMonitoringTime
        {
            get
            {
                return _connection.getData()[ID_keys.COARSE_FLOW_MONITORING_TIME];
            }
            set { throw new NotImplementedException(); }
        }  // CBT
        public override int CoarseFlowMonitoring
        {
            get
            {
                return _connection.getData()[ID_keys.COARSE_FLOW_MONITORING];
            }
            set { throw new NotImplementedException(); }
        }      // CBK
        public override int FineFlowMonitoring
        {
            get
            {
                return _connection.getData()[ID_keys.FINE_FLOW_MONITORING];
            }
            set { throw new NotImplementedException(); }
        }        // FBK
        public override int FineFlowMonitoringTime
        {
            get
            {
                return _connection.getData()[ID_keys.FINE_FLOW_MONITORING_TIME];
            }
            set { throw new NotImplementedException(); }
        }    // FBT

        public override int SystematicDifference
        {
            get
            {
                return _connection.getData()[ID_keys.SYSTEMATIC_DIFFERENCE];
            }
            set { throw new NotImplementedException(); }
        }  // SYD

        public override int DownwardsDosing { get; set; }

        public override int ValveControl
        {
            get
            {
                return _connection.getData()[ID_keys.VALVE_CONTROL];
            }
            set { throw new NotImplementedException(); }
        }      // VCT
        public override int EmptyingMode
        {
            get
            {
                return _connection.getData()[ID_keys.EMPTYING_MODE];
            }
            set { throw new NotImplementedException(); }
        }      // EMD


        public override int Status
        {
            get
            {
                return _connection.getData()[ID_keys.SCALE_COMMAND_STATUS];
            }
        }

        public override int DelayTimeAfterFineFlow
        {
            get
            {
                return _connection.getData()[ID_keys.DELAY1_DOSING];
            }
            set
            {
                throw new NotImplementedException();
            }
        }      // ID = DL1 : Delay 1 für Dosieren. 

        public override int ActivationTimeAfterFineFlow
        {
            get
            {
                return _connection.getData()[ID_keys.FINEFLOW_PHASE_BEFORE_COARSEFLOW];
            }
            set
            {
                throw new NotImplementedException();
            }
        } // ID = FFL : "Feinstromphase vor Grobstrom". 


        // Limit values 1-4: 
        public override int LimitValue1Input
        {
            get
            {
                int value = _connection.getData()[ID_keys.LIMIT_VALUE];
                return (value & 0x1);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int LimitValue2Source
        {
            get
            {
                int value = _connection.getData()[ID_keys.LIMIT_VALUE];
                return (value & 0x2)>>1;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int LimitValue3Source
        {
            get
            {
                int value = _connection.getData()[ID_keys.LIMIT_VALUE];
                return (value & 0x4) >> 2;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int LimitValue4Source
        {
            get
            {
                int value = _connection.getData()[ID_keys.LIMIT_VALUE];
                return (value & 0x8) >> 3;
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        // Method to check if the handshake is done.
        public override int Handshake
        {
            get
            {
                if (_connection.getData()[ID_keys.SCALE_COMMAND_STATUS] == 1801543519)
                    return 1;
                else
                    return 0;
            }
        }

        /*
        // Alternative implementation to the get-property 'handshake' previously:
        public override bool HandshakeCheck()
        {

            int ScaleCommandStatus_before = 1801543519;

            if (_connection.getData()[ID_keys.SCALE_COMMAND_STATUS] == 1634168417)
            {
                ScaleCommandStatus_before = 1634168417;
                return false;
            }
            else
                if (_connection.getData()[ID_keys.SCALE_COMMAND_STATUS] == 1801543519 && ScaleCommandStatus_before == 1634168417)
            {
                ScaleCommandStatus_before = 1801543519;
                return true;
            }
            return false;
        }
        */

        /*
        // Input values : To implement these you have to get the ID's from the manual and set them like:
        // this._connection.Read(ParameterKeys.GROSS_VALUE);
        */

        public override int ApplicationMode { get { return 1; } }

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

        public override int LegalTradeOp { get { return 1; } }
        public override int ToleranceErrorPlus { get { return 1; } }
        public override int ToleranceErrorMinus { get { return 1; } }
        public override int StatusInput1 { get { return 1; } }
        public override int GeneralScaleError { get { return 1; } }

        public override int CurrentDosingTime { get { return 1; } }
        public override int CurrentCoarseFlowTime { get { return 1; } }
        public override int CurrentFineFlowTime { get { return 1; } }
        public override int ParameterSetProduct { get { return 1; } }

        public override int ManualTareValue { get; set; }
        public override int LimitValue1Mode { get; set; }
        public override int LimitValue1ActivationLevelLowerBandLimit { get; set; }
        public override int LimitValue1HysteresisBandHeight { get; set; }

        // Output words for the standard application: Not implemented so far

        public override int LimitValue2Mode { get; set; }
        public override int LimitValue2ActivationLevelLowerBandLimit { get; set; }
        public override int LimitValue2HysteresisBandHeight { get; set; }        
        public override int LimitValue3Mode { get; set; }
        public override int LimitValue3ActivationLevelLowerBandLimit { get; set; }
        public override int LimitValue3HysteresisBandHeight { get; set; }
        public override int LimitValue4Mode { get; set; }
        public override int LimitValue4ActivationLevelLowerBandLimit { get; set; }
        public override int LimitValue4HysteresisBandHeight { get; set; }

        // Output words for the filler application: Not implemented so far

        public override int TotalWeight { get { return 1; } }
        public override int TargetFillingWeight { get; set; }
        public override int CoarseFlowCutOffPointSet { get; set; }
        public override int FineFlowCutOffPointSet { get; set; }
        public override int StartWithFineFlow { get; set; }

        public override IDeviceData DeviceValues { get; }
    }
}
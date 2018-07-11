using System;
using System.ComponentModel;
using System.Threading.Tasks;
using HBM.WT.API;
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

    public class WTXJet : BaseWTDevice
    {
        private string[] data_str_arr;
        private ushort[] data;
        private INetConnection m_Connection;
        private string ipAddr;
        private bool dataReceived;

        public override event Func<object, EventArgs, Task> Shutdown;
        public override event EventHandler<NetConnectionEventArgs<ushort[]>> DataUpdateEvent;

        private struct ParameterKeys
        {
            public const string MeasuredValue = "601A/01";      // _601A_01 
            
            public const string GrossValue = "6144/00";
            public const string ZeroValue  = "6142/00";
            public const string TareValue  = "6143/00";
            public const string Decimals   = "6013/01";

            public const string dosingCounter= "NDS";
            public const string dosingStatus = "SDO";
            public const string dosingResult = "FRS1";

            public const string weightMovingDetection = "6153/00";
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

        public WTXJet(INetConnection connection) : base(connection)  // ParameterProperty umändern 
        {
            m_Connection = connection;

            this.ipAddr = "172.19.103.8";

            this.dataReceived = false;
            data_str_arr = new string[59];
            data = new ushort[59];

            for (int index = 0; index < 59; index++)
                data[index] = 0x00;

            m_Connection.RaiseDataEvent += this.UpdateEvent;   // Subscribe to the event.
        }


        public override void initialize_timer(int timer_interval)
        {
            throw new NotImplementedException();
        }


        public override bool isDataReceived
        {
            get
            {
                return this.dataReceived;
            }
            set
            {
                this.dataReceived = value;
            }
        }
        public override void Calibration(ushort command)
        {
        }

        public override void UpdateEvent(object sender, NetConnectionEventArgs<ushort[]> e) { }

        
        public override void Async_Call(ushort commandParam, Action<IDeviceData> callbackParam)
        {
           throw new NotImplementedException();
        }

        public override void SyncCall_Write_Command(ushort wordNumber, ushort commandParam, Action<IDeviceData> callbackParam)
        {
            throw new NotImplementedException();
        }

        public override void ReadDoWork(object sender, DoWorkEventArgs dowork_asynchronous)
        {
            throw new NotImplementedException();
        }

        public override IDeviceData asyncReadData(BackgroundWorker worker)
        {
            throw new NotImplementedException();
        }

        public override IDeviceData syncReadData()
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
        

        public override Task OnShutdown()
        {
            throw new NotImplementedException();
        }

        public override ushort[] getValuesAsync()
        {
            throw new NotImplementedException();
        }

        //public override void UpdateEvent(object sender, MessageEvent<ushort> e) { }

        public override ushort[] getDataUshort { get { return new ushort[1]; } set { this.data = value;    } }

        public override int NetValue
        {
            get
            {
                return this.m_Connection.Read<int>(ParameterKeys.MeasuredValue);
            }
        }


        public override int GrossValue
        {
            get
            {
                return this.m_Connection.Read<int>(ParameterKeys.GrossValue);        // GrossValue = "6144/00";
            }
            
        }

        public override int generalWeightError { get { return 1; } }       // data[3]
        public override int scaleAlarmTriggered { get { return 1; } }      // data[4]
        public override int limitStatus { get { return 1; } }               // data[5]

        public override int weightMoving
        {
            get
            {
                return this.m_Connection.Read<int>(ParameterKeys.weightMovingDetection);
            }
        }              // data[6]


        public override int scaleSealIsOpen { get { return 1; } }         // data[7]
        public override int manualTare { get { return 1; } }                // data[8]
        public override int weightType { get { return 1; } }                // data[9]
        public override int scaleRange { get { return 1; } }                // data[10]
        public override int zeroRequired { get { return 1; } }              // data[11]
        public override int weightWithinTheCenterOfZero { get { return 1; } }   // data[12]
        public override int weightInZeroRange { get { return 1; } }               // data[13]
        public override int applicationMode { get { return 1; } }           // data[14]

        public override int decimals
        {
            get
            {
                return m_Connection.Read<int>(ParameterKeys.Decimals);   // Decimals = "DPT";
            }
        }     // data[15]

        public override int unit { get { return 1; }}                       // data[16]
        public override int handshake { get { return 1; }}                  // data[17]
        public override int status { get { return 1; }}                     // data[18]

        public override int input1 { get { return 1; } }            // data[19]    // IS1
        public override int input2 { get { return 1; } }            // data[20]    // IS2
        public override int input3 { get { return 1; } }            // data[21]    // IS3
        public override int input4 { get { return 1; } }            // data[22]    // IS4 
        public override int output1 { get { return 1; } }           // data[23]    // OS1
        public override int output2 { get { return 1; } }           // data[24]    // OS2
        public override int output3 { get { return 1; } }           // data[25]    // OS3
        public override int output4 { get { return 1; } }           // data[26]    // OS4 

        public override int limitStatus1 { get { return 1; } }       // data[27]
        public override int limitStatus2 { get { return 1; } }       // data[28]
        public override int limitStatus3 { get { return 1; } }       // data[29]
        public override int limitStatus4 { get { return 1; } }       // data[30]

        public override int weightMemDay { get { return 1; } }          // data[31]
        public override int weightMemMonth { get { return 1; } }        // data[32]
        public override int weightMemYear { get { return 1; } }         // data[33]
        public override int weightMemSeqNumber { get { return 1; } }   // data[34]
        public override int weightMemGross { get { return 1; } }        // data[35]
        public override int weightMemNet { get { return 1; } }          // data[36]

        public override int coarseFlow { get { return 1; } }                // data[37]
        public override int fineFlow { get { return 1; } }                  // data[38]
        public override int ready { get { return 1; } }                      // data[39]
        public override int reDosing { get { return 1; } }                  // data[40]
        public override int emptying { get { return 1; } }                   // data[41]
        public override int flowError { get { return 1; } }                 // data[42]
        public override int alarm { get { return 1; } }                      // data[43]
        public override int ADC_overUnderload { get { return 1; } }     // data[44]
        public override int maxDosingTime { get { return 1; } }            // data[45]
        public override int legalTradeOp { get { return 1; } }  // data[46]
        public override int toleranceErrorPlus { get { return 1; } }       // data[47]
        public override int toleranceErrorMinus { get { return 1; } }      // data[48]
        public override int statusInput1 { get { return 1; } }     // data[49]
        public override int generalScaleError { get { return 1; } }        // data[50]

        public override int fillingProcessStatus
        {
            get
            {
                return this.m_Connection.Read<int>(ParameterKeys.dosingStatus);
            }
        }             // data[51]

        public override int numberDosingResults
        {
            get
            {
                return this.m_Connection.Read<int>(ParameterKeys.dosingCounter);
            }
        }            // data[52]

        public override int dosingResult
        {
            get
            {
                return this.m_Connection.Read<int>(ParameterKeys.dosingResult);
            }
        }           // data[53]

        public override int meanValueDosingResults { get { return 1; } }      // data[54]
        public override int standardDeviation { get { return 1; } }                // data[55]
        public override int totalWeight { get { return 1; } }                      // data[56]
        public override int fineFlowCutOffPoint { get { return 1; } }           // data[57]
        public override int coarseFlowCutOffPoint { get { return 1; } }         // data[58]
        public override int currentDosingTime { get { return 1; } }                // data[59]
        public override int currentCoarseFlowTime { get { return 1; } }           // data[60]
        public override int currentFineFlowTime { get { return 1; } }             // data[61]
        public override int parameterSetProduct { get { return 1; } }                     // data[62]

        public override int manualTareValue { get; set; }
        public override int limitValue1Input { get; set; }
        public override int limitValue1Mode { get; set; }
        public override int limitValue1ActivationLevelLowerBandLimit { get; set; }
        public override int limitValue1HysteresisBandHeight { get; set; }

        // Output words for the standard application: Not used so far

        public override int limitValue2Source { get; set; }
        public override int limitValue2Mode { get; set; }
        public override int limitValue2ActivationLevelLowerBandLimit { get; set; }
        public override int limitValue2HysteresisBandHeight { get; set; }
        public override int limitValue3Source { get; set; }
        public override int limitValue3Mode { get; set; }
        public override int limitValue3ActivationLevelLowerBandLimit { get; set; }
        public override int limitValue3HysteresisBandHeight { get; set; }
        public override int limitValue4Source { get; set; }
        public override int limitValue4Mode { get; set; }
        public override int limitValue4ActivationLevelLowerBandLimit { get; set; }
        public override int limitValue4HysteresisBandHeight { get; set; }

        // Output words for the filler application: Not used so far


        public override int ResidualFlowTime { get; set; }
        public override int targetFillingWeight { get; set; }
        public override int coarseFlowCutOffPointSet { get; set; }
        public override int fineFlowCutOffPointSet { get; set; }
        public override int minimumFineFlow { get; set; }
        public override int optimizationOfCutOffPoints { get; set; }
        public override int maximumDosingTime { get; set; }
        public override int startWithFineFlow { get; set; }
        public override int coarseLockoutTime { get; set; }
        public override int fineLockoutTime { get; set; }
        public override int tareMode { get; set; }
        public override int upperToleranceLimit { get; set; }
        public override int lowerToleranceLimit { get; set; }
        public override int minimumStartWeight { get; set; }
        public override int emptyWeight { get; set; }
        public override int tareDelay { get; set; }
        public override int coarseFlowMonitoringTime { get; set; }
        public override int coarseFlowMonitoring { get; set; }
        public override int fineFlowMonitoring { get; set; }
        public override int fineFlowMonitoringTime { get; set; }
        public override int delayTimeAfterFineFlow { get; set; }
        public override int activationTimeAfterFineFlow { get; set; }
        public override int systematicDifference { get; set; }
        public override int downardsDosing { get; set; }
        public override int valveControl { get; set; }
        public override int emptyingMode { get; set; }



        public override BaseWTDevice getDeviceAbstract { get; }


        public override ModbusTCPConnection getConnection { get; }


        public override IDeviceData DeviceValues { get; }

        public override string[] getDataStr
        {
            get
            {
                return this.data_str_arr;
            }
            set
            {
                this.data_str_arr = value;
            }
        }

        /* 
*In the following methods the different options for the single integer values are used to define and
*interpret the value. Finally a string should be returned from the methods to write it onto the GUI Form. 
*/
        public string netGrossValueStringComment(int value, int decimals)
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


        public string unitStringComment()
        {
            switch (this.unit)
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

        public string statusStringComment()
        {
            if (this.status == 1)
                return "Execution OK!";
            else
                if (this.status != 1)
                return "Execution not OK!";
            else
                return "error.";

        }

        public override void Connect()
        {
            throw new NotImplementedException();
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }
    }
}

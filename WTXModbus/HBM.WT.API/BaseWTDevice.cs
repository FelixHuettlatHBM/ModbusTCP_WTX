using HBM.WT.API.WTX.Jet;
using HBM.WT.API.WTX.Modbus;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HBM.WT.API
{
    public abstract class BaseWtDevice : IDeviceData
    {

        /*
        private Action<IDeviceData> callback_obj;

        private ushort command;

        private string ipAddr;

        private ModbusTCPConnection ModbusConnObj;

        protected INetConnection m_Connection;

        private JetBusConnection JetConnObj;      
        
        private INetConnection NetObj; 

        private int timeoutMS;
        private bool inputModbusJet;
        */

        //public abstract INetConnection Connection { get; }

        //private INetCommunication<uint, JToken> commObj;

        /*
    public BaseWTDevice(INetConnection connection)
    {
        m_Connection = connection;
        inputModbusJet = true;
        timeoutMS = 5000;

        this.ipAddr = "172.19.103.8";

        this.ModbusConnObj = new ModbusTCPConnection(ipAddr);
*/

        //this.NetObj = new ModbusTCPConnection(ipAddr);

        /*
        if (inputModbusJet == true)
        { 
            this.ModbusConnObj = new ModbusTCPConnection(ipAddr);
            }
        else
            if (inputModbusJet == false)
            {
            IJetConnection IJetObj = new WebSocketJetConnection(ipAddr, delegate { return true; });      // Unter Umständen die Certification Callback ausimplementieren. 
            JetPeer jetObj = new JetPeer(IJetObj);                                                       // Certification Callbackmethode in API verpackt? Oder Nutzer selbst implementieren? Machen wir! Erstmal als delegate -> true. 

            this.JetConnObj = new JetBusConnection(jetObj, timeoutMS);
        }

    }
    */

        protected INetConnection m_Connection;

        public BaseWtDevice(INetConnection connection)
        {
            m_Connection = connection;
        }


        public abstract event EventHandler<DataEvent> DataUpdateEvent;

        public abstract bool isConnected { get; set; }

        public abstract ushort[] GetValuesAsync();        // Neu : 4.5 für eventbasierten asynchronen Abruf der Daten

        public abstract void Connect(Action<bool> completed, double timeoutMs);

        //public abstract void Connect();

        public abstract INetConnection getModbusConnection { get; }

        public abstract INetConnection getJetBusConnection { get; }
        
        public abstract void Disconnect(Action<bool> DisconnectCompleted);

        public abstract IDeviceData DeviceValues { get; }

        public abstract void initialize_timer(int timerInterval);

        public abstract bool IsDataReceived { get; set; }
        
        public abstract void Calibration(ushort command);

        public abstract void UpdateEvent(object sender, DataEvent e);
        
        public abstract string[] GetDataStr { get; set; }

        public abstract ushort[] GetDataUshort { get; set; }

        public abstract int NetValue { get; }                    // data[1]
        public abstract int GrossValue { get; }                  // data[2]
        public abstract int GeneralWeightError { get; }          // data[3]
        public abstract int ScaleAlarmTriggered { get; }         // data[4]
        public abstract int LimitStatus { get; }                 // data[5]
        public abstract int WeightMoving { get; }                // data[6]
        public abstract int ScaleSealIsOpen { get; }             // data[7]
        public abstract int ManualTare { get; }                  // data[8]
        public abstract int WeightType { get; }                  // data[9]
        public abstract int ScaleRange { get; }                  // data[10]
        public abstract int ZeroRequired { get; }                // data[11]
        public abstract int WeightWithinTheCenterOfZero { get; } // data[12]
        public abstract int WeightInZeroRange { get; }           // data[13]
        public abstract int ApplicationMode { get; }             // data[14]
        public abstract int Decimals { get; }                    // data[15]
        public abstract int Unit { get; }                        // data[16]
        public abstract int Handshake { get; }                   // data[17]
        public abstract int Status { get; }                      // data[18]

        public abstract int Input1 { get; }            // data[19]
        public abstract int Input2 { get; }            // data[20]
        public abstract int Input3 { get; }            // data[21]
        public abstract int Input4 { get; }            // data[22]
        public abstract int Output1 { get; }           // data[23]
        public abstract int Output2 { get; }           // data[24]
        public abstract int Output3 { get; }           // data[25]
        public abstract int Output4 { get; }           // data[26]

        public abstract int LimitStatus1 { get; }       // data[27]
        public abstract int LimitStatus2 { get; }       // data[28]
        public abstract int LimitStatus3 { get; }       // data[29]
        public abstract int LimitStatus4 { get; }       // data[30]

        public abstract int WeightMemDay { get; }          // data[31]
        public abstract int WeightMemMonth { get; }        // data[32]
        public abstract int WeightMemYear { get; }         // data[33]
        public abstract int WeightMemSeqNumber { get; }    // data[34]
        public abstract int WeightMemGross { get; }        // data[35]
        public abstract int WeightMemNet { get; }          // data[36]

        public abstract int CoarseFlow { get; }            // data[37]
        public abstract int FineFlow { get; }              // data[38]
        public abstract int Ready { get; }                 // data[39]
        public abstract int ReDosing { get; }              // data[40]
        public abstract int Emptying { get; }              // data[41]
        public abstract int FlowError { get; }             // data[42]
        public abstract int Alarm { get; }                 // data[43]
        public abstract int AdcOverUnderload { get; }     // data[44]
        public abstract int MaxDosingTime { get; }         // data[45]
        public abstract int LegalTradeOp { get; }          // data[46]
        public abstract int ToleranceErrorPlus { get; }    // data[47]
        public abstract int ToleranceErrorMinus { get; }   // data[48]
        public abstract int StatusInput1 { get; }          // data[49]
        public abstract int GeneralScaleError { get; }     // data[50]

        public abstract int FillingProcessStatus { get; }    // data[51]
        public abstract int NumberDosingResults { get; }     // data[52]
        public abstract int DosingResult { get; }            // data[53]
        public abstract int MeanValueDosingResults { get; }  // data[54]
        public abstract int StandardDeviation { get; }       // data[55]
        public abstract int TotalWeight { get; }             // data[56]
        public abstract int FineFlowCutOffPoint { get; }     // data[57]
        public abstract int CoarseFlowCutOffPoint { get; }   // data[58]
        public abstract int CurrentDosingTime { get; }        // data[59]
        public abstract int CurrentCoarseFlowTime { get; }    // data[60]
        public abstract int CurrentFineFlowTime { get; }      // data[61]
        public abstract int ParameterSetProduct { get; }     // data[62]

        public abstract int ManualTareValue { get; set; }
        public abstract int LimitValue1Input { get; set; }
        public abstract int LimitValue1Mode { get; set; }
        public abstract int LimitValue1ActivationLevelLowerBandLimit { get; set; }
        public abstract int LimitValue1HysteresisBandHeight { get; set; }

        // Output words for the standard application: Not used so far

        public abstract int LimitValue2Source { get; set; }
        public abstract int LimitValue2Mode { get; set; }
        public abstract int LimitValue2ActivationLevelLowerBandLimit { get; set; }
        public abstract int LimitValue2HysteresisBandHeight { get; set; }
        public abstract int LimitValue3Source { get; set; }
        public abstract int LimitValue3Mode { get; set; }
        public abstract int LimitValue3ActivationLevelLowerBandLimit { get; set; }
        public abstract int LimitValue3HysteresisBandHeight { get; set; }
        public abstract int LimitValue4Source { get; set; }
        public abstract int LimitValue4Mode { get; set; }
        public abstract int LimitValue4ActivationLevelLowerBandLimit { get; set; }
        public abstract int LimitValue4HysteresisBandHeight { get; set; }

        // Output words for the filler application: Not used so far


        public abstract int ResidualFlowTime { get; set; }
        public abstract int TargetFillingWeight { get; set; }
        public abstract int CoarseFlowCutOffPointSet { get; set; }
        public abstract int FineFlowCutOffPointSet { get; set; }
        public abstract int MinimumFineFlow { get; set; }
        public abstract int OptimizationOfCutOffPoints { get; set; }
        public abstract int MaximumDosingTime { get; set; }
        public abstract int StartWithFineFlow { get; set; }
        public abstract int CoarseLockoutTime { get; set; }
        public abstract int FineLockoutTime { get; set; }
        public abstract int TareMode { get; set; }
        public abstract int UpperToleranceLimit { get; set; }
        public abstract int LowerToleranceLimit { get; set; }
        public abstract int MinimumStartWeight { get; set; }
        public abstract int EmptyWeight { get; set; }
        public abstract int TareDelay { get; set; }
        public abstract int CoarseFlowMonitoringTime { get; set; }
        public abstract int CoarseFlowMonitoring { get; set; }
        public abstract int FineFlowMonitoring { get; set; }
        public abstract int FineFlowMonitoringTime { get; set; }
        public abstract int DelayTimeAfterFineFlow { get; set; }
        public abstract int ActivationTimeAfterFineFlow { get; set; }
        public abstract int SystematicDifference { get; set; }
        public abstract int DownardsDosing { get; set; }
        public abstract int ValveControl { get; set; }
        public abstract int EmptyingMode { get; set; }






        public abstract void Async_Call(/*ushort wordNumberParam, */ushort commandParam, Action<IDeviceData> callbackParam);


        // Neu - 8.3.2018 - Ohne Backgroundworker - Ohne Asynchronität
        public abstract void SyncCall(ushort wordNumber, ushort commandParam, Action<IDeviceData> callbackParam);      // Callback-Methode nicht benötigt. 


        // This method is executed asynchronously in the background for reading the register by a Backgroundworker. 
        // @param : sender - the object of this class. dowork_asynchronous - the argument of the event. 
        public abstract void ReadDoWork(object sender, DoWorkEventArgs doworkAsynchronous);

        // This method read the register of the Device(here: WTX120), therefore it calls the method in class Modbus_TCP to read the register. 
        // @return: IDevice_Values - Interface, that contains all values for the device. 
        public abstract IDeviceData AsyncReadData(BackgroundWorker worker);

        // Neu : 8.3.2018
        public abstract IDeviceData SyncReadData();

        public abstract BaseWtDevice GetDeviceAbstract { get; }

        public abstract void ReadCompleted(object sender, RunWorkerCompletedEventArgs e);

        public abstract void WriteDoWork(object sender, DoWorkEventArgs e);

        public abstract void WriteCompleted(object sender, RunWorkerCompletedEventArgs e);
        
    }
}


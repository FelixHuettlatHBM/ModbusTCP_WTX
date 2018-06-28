
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/*
 *  This abstract class predefines the methods and auto-properties for the use in class 'WTX120' and it inherits from interface 
 *  IDeviceValues,having all neccessary values as auto-properties, which are read from the WTX device. 
 *  
 *  So class 'WTX120' inherits from class 'DeviceAbstract'. The purpose for this class is to allow other communication protocols, 
 *  like Jetbus or CanBus to connect to the  WTX120 device. For this application there is no JetBus or CanBus required, so the 
 *  neccessary for JetBus is commented.
 *
 */
namespace WTXModbus
{
    public abstract class BaseWTDevice : IDeviceData
    {

        //private INetCommunication<uint, JToken> commObj;

        public BaseWTDevice(ModbusTCPConnection connection, int paramTimerInterval)
        {
            /*
            inputModbusJet = true;
            timeoutMS = 5000;

            this.ipAddr = "172.19.103.8";

            this.ModbusConnObj = new ModbusConnection(ipAddr);
            */

            /*
            if (inputModbusJet == true)
            { 
                this.ModbusConnObj = new ModbusConnection(ipAddr);
                }
            else
                if (inputModbusJet == false)
                {
                IJetConnection IJetObj = new WebSocketJetConnection(ipAddr, delegate { return true; });      // Unter Umständen die Certification Callback ausimplementieren. 
                JetPeer jetObj = new JetPeer(IJetObj);                                                       // Certification Callbackmethode in API verpackt? Oder Nutzer selbst implementieren? Machen wir! Erstmal als delegate -> true. 

                this.JetConnObj = new JetBusConnection(jetObj, timeoutMS);
            }
            */
        }

        public abstract ModbusTCPConnection getConnection { get; }
        public abstract IDeviceData DeviceValues { get; }

        public abstract void connect();
        public abstract void disconnect();

        public abstract event EventHandler<NetConnectionEventArgs<ushort[]>> DataUpdateEvent;

        public abstract void initialize_timer(int timer_interval);
        
        public abstract void Calibration(ushort command);
        public abstract void UpdateEvent(object sender, NetConnectionEventArgs<ushort[]> e);

        public abstract string[] getDataStr { get; set; }
        public abstract ushort[] getDataUshort { get; set; }
      
        public abstract int NetValue { get; }                    // data[1]
        public abstract int GrossValue { get; }                  // data[2]
        public abstract int generalWeightError { get; }          // data[3]
        public abstract int scaleAlarmTriggered { get; }         // data[4]
        public abstract int limitStatus { get; }                 // data[5]
        public abstract int weightMoving { get; }                // data[6]
        public abstract int scaleSealIsOpen { get; }             // data[7]
        public abstract int manualTare { get; }                  // data[8]
        public abstract int weightType { get; }                  // data[9]
        public abstract int scaleRange { get; }                  // data[10]
        public abstract int zeroRequired { get; }                // data[11]
        public abstract int weightWithinTheCenterOfZero { get; } // data[12]
        public abstract int weightInZeroRange { get; }           // data[13]
        public abstract int applicationMode { get; }             // data[14]
        public abstract int decimals { get; }                    // data[15]
        public abstract int unit { get; }                        // data[16]
        public abstract int handshake { get; }                   // data[17]
        public abstract int status { get; }                      // data[18]
        
        public abstract int input1 { get; }            // data[19]
        public abstract int input2 { get; }            // data[20]
        public abstract int input3 { get; }            // data[21]
        public abstract int input4 { get; }            // data[22]
        public abstract int output1 { get; }           // data[23]
        public abstract int output2 { get; }           // data[24]
        public abstract int output3 { get; }           // data[25]
        public abstract int output4 { get; }           // data[26]

        public abstract int limitStatus1 { get; }       // data[27]
        public abstract int limitStatus2 { get; }       // data[28]
        public abstract int limitStatus3 { get; }       // data[29]
        public abstract int limitStatus4 { get; }       // data[30]

        public abstract int weightMemDay { get; }          // data[31]
        public abstract int weightMemMonth { get; }        // data[32]
        public abstract int weightMemYear { get; }         // data[33]
        public abstract int weightMemSeqNumber { get; }    // data[34]
        public abstract int weightMemGross { get; }        // data[35]
        public abstract int weightMemNet { get; }          // data[36]

        public abstract int coarseFlow { get; }            // data[37]
        public abstract int fineFlow { get; }              // data[38]
        public abstract int ready { get; }                 // data[39]
        public abstract int reDosing { get; }              // data[40]
        public abstract int emptying { get; }              // data[41]
        public abstract int flowError { get; }             // data[42]
        public abstract int alarm { get; }                 // data[43]
        public abstract int ADC_overUnderload { get; }     // data[44]
        public abstract int maxDosingTime { get; }         // data[45]
        public abstract int legalTradeOp { get; }          // data[46]
        public abstract int toleranceErrorPlus { get; }    // data[47]
        public abstract int toleranceErrorMinus { get; }   // data[48]
        public abstract int statusInput1 { get; }          // data[49]
        public abstract int generalScaleError { get; }     // data[50]

        public abstract int fillingProcessStatus { get; }    // data[51]
        public abstract int numberDosingResults { get; }     // data[52]
        public abstract int dosingResult { get; }            // data[53]
        public abstract int meanValueDosingResults { get; }  // data[54]
        public abstract int standardDeviation { get; }       // data[55]
        public abstract int totalWeight { get; }             // data[56]
        public abstract int fineFlowCutOffPoint { get; }     // data[57]
        public abstract int coarseFlowCutOffPoint { get; }   // data[58]
        public abstract int currentDosingTime { get; }        // data[59]
        public abstract int currentCoarseFlowTime { get; }    // data[60]
        public abstract int currentFineFlowTime { get; }      // data[61]
        public abstract int parameterSetProduct { get; }     // data[62]

        public abstract int manualTareValue { get; set; }
        public abstract int limitValue1Input { get; set; }
        public abstract int limitValue1Mode  { get; set; }
        public abstract int limitValue1ActivationLevelLowerBandLimit { get; set; }
        public abstract int limitValue1HysteresisBandHeight { get; set; }

        // Output words for the standard application: Not used so far

        public abstract int limitValue2Source { get; set; }
        public abstract int limitValue2Mode { get; set; }
        public abstract int limitValue2ActivationLevelLowerBandLimit { get; set; }
        public abstract int limitValue2HysteresisBandHeight { get; set; }
        public abstract int limitValue3Source { get; set; }
        public abstract int limitValue3Mode { get; set; }
        public abstract int limitValue3ActivationLevelLowerBandLimit { get; set; }
        public abstract int limitValue3HysteresisBandHeight { get; set; }
        public abstract int limitValue4Source { get; set; }
        public abstract int limitValue4Mode { get; set; }
        public abstract int limitValue4ActivationLevelLowerBandLimit { get; set; }
        public abstract int limitValue4HysteresisBandHeight { get; set; }
 
        // Output words for the filler application: Not used so far

        
        public abstract int ResidualFlowTime { get; set; }
        public abstract int targetFillingWeight { get; set; }
        public abstract int coarseFlowCutOffPointSet { get; set; }
        public abstract int fineFlowCutOffPointSet { get; set; }
        public abstract int minimumFineFlow { get; set; }
        public abstract int optimizationOfCutOffPoints { get; set; }
        public abstract int maximumDosingTime { get; set; }
        public abstract int startWithFineFlow { get; set; }
        public abstract int coarseLockoutTime { get; set; }
        public abstract int fineLockoutTime { get; set; }
        public abstract int tareMode { get; set; }
        public abstract int upperToleranceLimit { get; set; }
        public abstract int lowerToleranceLimit { get; set; }
        public abstract int minimumStartWeight { get; set; }
        public abstract int emptyWeight { get; set; }
        public abstract int tareDelay { get; set; }
        public abstract int coarseFlowMonitoringTime { get; set; }
        public abstract int coarseFlowMonitoring { get; set; }
        public abstract int fineFlowMonitoring { get; set; }
        public abstract int fineFlowMonitoringTime { get; set; }
        public abstract int delayTimeAfterFineFlow { get; set; }
        public abstract int activationTimeAfterFineFlow { get; set; }
        public abstract int systematicDifference { get; set; }
        public abstract int downardsDosing { get; set; }
        public abstract int valveControl { get; set; }
        public abstract int emptyingMode { get; set; }
        

    }
}


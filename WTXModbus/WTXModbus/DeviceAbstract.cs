using Hbm.Devices.WTXModbus;
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
    public abstract class DeviceAbstract : IDeviceValues
    {

        //private INetCommunication<uint, JToken> commObj;

        public DeviceAbstract(ModbusConnection connection, int paramTimerInterval)
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

        public abstract ModbusConnection getConnection { get; }
        public abstract IDeviceValues DeviceValues { get; }

        public abstract event EventHandler<NetConnectionEventArgs<ushort[]>> DataUpdateEvent;

        public abstract void initialize_timer(int timer_interval);
        
        public abstract void Calibration(ushort command);
        public abstract void UpdateEvent(object sender, NetConnectionEventArgs<ushort[]> e);

        public abstract string[] getDataStr { get; set; }
        public abstract ushort[] getDataUshort { get; set; }
      
        public abstract int NetValue { get; }                   // data[1]
        public abstract int GrossValue { get; }                 // data[2]
        public abstract int generalWeightError { get; }       // data[3]
        public abstract int scaleAlarmTriggered { get; }      // data[4]
        public abstract int limitStatus { get; }               // data[5]
        public abstract int weightMoving { get; }              // data[6]
        public abstract int scaleSealIsOpen { get; }         // data[7]
        public abstract int manualTare { get; }                // data[8]
        public abstract int weightType { get; }                // data[9]
        public abstract int scaleRange { get; }                // data[10]
        public abstract int zeroRequired { get; }              // data[11]
        public abstract int weightWithinTheCenterOfZero { get; }   // data[12]
        public abstract int weightInZeroRange { get; }               // data[13]
        public abstract int applicationMode { get; }           // data[14]
        public abstract int decimals { get; }                   // data[15]
        public abstract int unit { get; }                       // data[16]
        public abstract int handshake { get; }                  // data[17]
        public abstract int status { get; }                     // data[18]
        
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
        public abstract int weightMemSeqNumber { get; }   // data[34]
        public abstract int weightMemGross { get; }        // data[35]
        public abstract int weightMemNet { get; }          // data[36]

        public abstract int coarseFlow { get; }                // data[37]
        public abstract int fineFlow { get; }                  // data[38]
        public abstract int ready { get; }                      // data[39]
        public abstract int reDosing { get; }                  // data[40]
        public abstract int emptying { get; }                   // data[41]
        public abstract int flowError { get; }                 // data[42]
        public abstract int alarm { get; }                      // data[43]
        public abstract int ADC_overUnderload { get; }     // data[44]
        public abstract int maxDosingTime { get; }            // data[45]
        public abstract int legalTradeOp { get; }  // data[46]
        public abstract int toleranceErrorPlus { get; }       // data[47]
        public abstract int toleranceErrorMinus { get; }      // data[48]
        public abstract int statusInput1 { get; }     // data[49]
        public abstract int generalScaleError { get; }        // data[50]

        public abstract int fillingProcessStatus { get; }             // data[51]
        public abstract int numberDosingResults { get; }                      // data[52]
        public abstract int dosingResult { get; }                     // data[53]
        public abstract int meanValueDosingResults { get; }      // data[54]
        public abstract int standardDeviation { get; }                // data[55]
        public abstract int totalWeight { get; }                      // data[56]
        public abstract int fineFlowCutOffPoint { get; }           // data[57]
        public abstract int coarseFlowCutOffPoint { get; }         // data[58]
        public abstract int actualDosingTime { get; }                // data[59]
        public abstract int actualCoarseFlowTime { get; }           // data[60]
        public abstract int actualFineFlowTime { get; }             // data[61]
        public abstract int parameterSetProduct { get; }                     // data[62]

        /*
        public abstract int Residual_flow_time { set; }
        public abstract int target_fillingWeight { set; }
        public abstract int coarse_flow_cut_off_pointSet { set; }
        public abstract int fine_flow_cut_off_pointSet { set; }
        public abstract int minimum_fine_flow { set; }
        public abstract int optimization_of_cut_off_points{ set; }
        public abstract int maximum_dosing_tome { set; }
        public abstract int startWith_fine_flow { set; }
        public abstract int coarse_lockout_time { set; }
        public abstract int fine_lockout_time { set; }
        public abstract int tare_mode { set; }
        public abstract int upper_tolerance_limit { set; }
        public abstract int lower_tolerance_limit { set; }
        public abstract int minimumStartWeight { set; }
        public abstract int emptyWeight { set; }
        public abstract int tare_delay { set; }
        public abstract int coarse_flow_monitoring_time { set; }
        public abstract int coarse_flow_monitoring { set; }
        public abstract int fine_flow_monitoring { set; }
        public abstract int fine_flow_monitoring_time { set; }
        public abstract int delay_time_after_fine_flow { set; }
        public abstract int activation_time_after_fine_flow { set; }
        public abstract int systematic_difference { set; }
        public abstract int downards_dosing { set; }
        public abstract int valve_control { set; }
        public abstract int emptying_mode { set; }
        */
        public abstract int calibrationWeight { set; }
        public abstract int zero_load { set; }
        public abstract int nominal_load { set; }
        
        public int calibration_weight { set => throw new NotImplementedException(); }
    }
}


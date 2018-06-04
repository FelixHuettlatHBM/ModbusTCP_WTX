/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
 * 
 * TCP/MODBUS Interface for WTX120 | 03/2018
 * 
 * Author : Felix Huettl
 * 
 *  */


using System;
using WTXModbus;

namespace Hbm.Devices.WTXModbus
{
    /// <summary>
    /// This is the interface for the values of the device. For example for the device WTX120. 
    /// The values are given in realtime by the device via the method ReadHoldingRegister() of the ModbusIpMaster. 
    /// The values have to be declared in this interface and initalized in the derived class "WTX120". 
    /// 
    /// For data transfer the entire interface is submitted from the derived class of IDevice_Values to the GUI or console application:
    /// From method Read_Completed(...) in class "WTX120" to method Read_DataReceived(IDevice_Values Device_Values) in class "GUI". 
    /// Furthermore you can access individual values if the interface is known and its derived class is completely implemented by 
    /// for example > IDevice_Values.NetandGrossValue <  or > IDevice_Values.get_data_str[0] > IDevice_Values.get_data_ushort[0] <. 
    /// >
    /// There are 2 more arrays: string[] get_data_str and ushort[] get_data_ushort to sum up all values in an array to simplify 
    /// further operations, like output or conditions.
    /// The Eventhandler "DataUpdateEvent" is triggered once the data read in class ModbusConnection from the WTX device and committed 
    /// to the GUI or consoleapplication "Programm.cs". 
    /// 
    /// Behind the variables the index of the arrays is given. 
    /// </summary>
    public interface IDeviceValues
    {
        event EventHandler<NetConnectionEventArgs<ushort[]>> DataUpdateEvent;   

        string[] getDataStr { get; set; }
        ushort[] getDataUshort { get; set; }
                
        int NetValue { get; }                      // data[1]
        int GrossValue { get; }                    // data[2]
        int generalWeightError { get; }            // data[3]
        int scaleAlarmTriggered { get; }           // data[4]
        int limitStatus { get; }                   // data[5]
        int weightMoving { get; }                  // data[6]
        int scaleSealIsOpen { get; }               // data[7]
        int manualTare { get; }                    // data[8]
        int weightType { get; }                    // data[9]
        int scaleRange { get; }                    // data[10]
        int zeroRequired { get; }                  // data[11]
        int weightWithinTheCenterOfZero { get; }   // data[12]
        int weightInZeroRange { get; }             // data[13]
        int applicationMode { get; }               // data[14]
        int decimals { get; }                      // data[15]
        int unit { get; }                          // data[16]
        int handshake { get; }                     // data[17]
        int status { get; }                        // data[18]

        int input1 { get; }            // data[19] - Digital input 1 to 4
        int input2 { get; }            // data[20]
        int input3 { get; }            // data[21]
        int input4 { get; }            // data[22]
        int output1 { get; }           // data[23]
        int output2 { get; }           // data[24]
        int output3 { get; }           // data[25]
        int output4 { get; }           // data[26]

        int limitStatus1 { get; }       // data[27]
        int limitStatus2 { get; }       // data[28]
        int limitStatus3 { get; }       // data[29]
        int limitStatus4 { get; }       // data[30]

        int weightMemDay { get; }          // data[31]
        int weightMemMonth { get; }        // data[32]
        int weightMemYear { get; }         // data[33]
        int weightMemSeqNumber { get; }    // data[34]
        int weightMemGross { get; }        // data[35]
        int weightMemNet { get; }          // data[36]

        int coarseFlow { get; }            // data[37]
        int fineFlow { get; }              // data[38]
        int ready { get; }                 // data[39]
        int reDosing { get; }              // data[40]
        int emptying { get; }              // data[41]
        int flowError { get; }             // data[42]
        int alarm { get; }                 // data[43]
        int ADC_overUnderload { get; }     // data[44]
        int maxDosingTime { get; }         // data[45]
        int legalTradeOp { get; }          // data[46]
        int toleranceErrorPlus { get; }    // data[47]
        int toleranceErrorMinus { get; }   // data[48]
        int statusInput1 { get; }          // data[49]
        int generalScaleError { get; }     // data[50]

        int fillingProcessStatus { get; }     // data[51]
        int numberDosingResults { get; }      // data[52]
        int dosingResult { get; }             // data[53]
        int meanValueDosingResults { get; }   // data[54]
        int standardDeviation { get; }        // data[55]
        int totalWeight { get; }              // data[56]
        int fineFlowCutOffPoint { get; }      // data[57]
        int coarseFlowCutOffPoint { get; }    // data[58]
        int actualDosingTime { get; }         // data[59]
        int actualCoarseFlowTime { get; }     // data[60]
        int actualFineFlowTime { get; }       // data[61]
        int parameterSetProduct { get; }      // data[62]

        // Get-Set-properties to set the output words from 2 to 26 for the standard application. 

        int manualTareValue { get; set; }

        int limitValue1Input { get; set; }
        int limitValue1Mode { get; set; }
        int limitValue1ActivationLevelLowerBandLimit { get; set; }
        int limitValue1HysteresisBandHeight { get; set; }
    
        int limitValue2Source { get; set; }
        int limitValue2Mode { get; set; }
        int limitValue2ActivationLevelLowerBandLimit { get; set; }
        int limitValue2HysteresisBandHeight { get; set; }

        int limitValue3Source { get; set; }
        int limitValue3Mode { get; set; }
        int limitValue3ActivationLevelLowerBandLimit { get; set; }
        int limitValue3HysteresisBandHeight { get; set; }

        int limitValue4Source { get; set; }
        int limitValue4Mode { get; set; }
        int limitValue4ActivationLevelLowerBandLimit { get; set; }
        int limitValue4HysteresisBandHeight { get; set; }
        

        // Get-Set-properties to set the output words from 9 to 44 for the filler application. 

        
        int ResidualFlowTime { get; set; }             
        int targetFillingWeight { get;  set; }
        int coarseFlowCutOffPointSet { get;  set; }
        int fineFlowCutOffPointSet { set; }
        int minimumFineFlow { set; }
        int optimizationOfCutOffPoints { set; }
        int maximumDosingTime { set; }
        int startWithFineFlow { get; set; }
        int coarseLockoutTime { get; set; }
        int fineLockoutTime { get; set; }
        int tareMode { get; set; }
        int upperToleranceLimit { get; set; }
        int lowerToleranceLimit { get; set; }
        int minimumStartWeight { get; set; }
        int emptyWeight { get; set; }
        int tareDelay { get; set; }
        int coarseFlowMonitoringTime { get; set; }
        int coarseFlowMonitoring { get; set; }
        int fineFlowMonitoring { get; set; }
        int fineFlowMonitoringTime { get; set; }
        int delayTimeAfterFineFlow { get; set; }
        int activationTimeAfterFineFlow { get; set; }
        int systematicDifference { get; set; }
        int downardsDosing { get; set; }
        int valveControl { get; set; }
        int emptyingMode { get; set; }
        
    }
}

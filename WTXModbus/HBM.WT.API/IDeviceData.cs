// <copyright file="IDeviceData.cs" company="Hottinger Baldwin Messtechnik GmbH">
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

namespace HBM.WT.API
{

    /// <summary>
    /// This is the interface for the values of the device. For example for the device WTX120.
    /// The values are given in realtime by the device via the method ReadHoldingRegister() of the ModbusIpMaster.
    /// The values have to be declared in this interface and initalized in the derived class "WTX120".
    ///
    /// For data transfer the entire interface is submitted from the derived class of IDevice_Values to the GUI:
    /// From method Read_Completed(...) in class "WTX120" to method Read_DataReceived(IDevice_Values Device_Values) in class "GUI".
    /// Furthermore you can access individual values if the interface is known and its derived class is completely implemented by
    /// for example > IDevice_Values.NetandGrossValue <  or > IDevice_Values.get_data_str[0] > IDevice_Values.get_data_ushort[0] <.
    /// >
    /// There are 2 more arrays: string[] get_data_str and ushort[] get_data_ushort to sum up all values in an array to simplify
    /// further operations, like output or conditions.
    ///
    /// Behind the integer variables the index of the arrays is given.
    /// </summary>
    public interface IDeviceData
    {

        /// <summary>
        ///
        /// </summary>
        event EventHandler<DataEvent> DataUpdateEvent;

        /// <summary>
        ///Gets or sets a value indicating whether
        /// </summary>
        bool IsDataReceived
        {
            get;
            set;
        }

        /// <summary>
        ///Gets
        /// </summary>
        int NetValue
        {
            get;
        } // data[1]

        /// <summary>
        ///Gets
        /// </summary>
        int GrossValue
        {
            get;
        } // data[2]

        /// <summary>
        ///Gets
        /// </summary>
        int GeneralWeightError
        {
            get;
        } // data[3]

        /// <summary>
        ///Gets
        /// </summary>
        int ScaleAlarmTriggered
        {
            get;
        } // data[4]

        /// <summary>
        ///Gets
        /// </summary>
        int LimitStatus
        {
            get;
        } // data[5]

        /// <summary>
        ///Gets
        /// </summary>
        int WeightMoving
        {
            get;
        } // data[6]

        /// <summary>
        ///Gets
        /// </summary>
        int ScaleSealIsOpen
        {
            get;
        } // data[7]

        /// <summary>
        ///Gets
        /// </summary>
        int ManualTare
        {
            get;
        } // data[8]

        /// <summary>
        ///Gets
        /// </summary>
        int WeightType
        {
            get;
        } // data[9]

        /// <summary>
        ///Gets
        /// </summary>
        int ScaleRange
        {
            get;
        } // data[10]

        /// <summary>
        ///Gets
        /// </summary>
        int ZeroRequired
        {
            get;
        } // data[11]

        /// <summary>
        ///Gets
        /// </summary>
        int WeightWithinTheCenterOfZero
        {
            get;
        }

        // data[12]

        /// <summary>
        ///Gets
        /// </summary>
        int WeightInZeroRange
        {
            get;
        }  // data[13]

        /// <summary>
        ///Gets
        /// </summary>
        int ApplicationMode
        {
            get;
        } // data[14]

        /// <summary>
        ///Gets
        /// </summary>
        int Decimals
        {
            get;
        } // data[15]

        /// <summary>
        ///Gets
        /// </summary>
        int Unit
        {
            get;
        } // data[16]

        /// <summary>
        ///Gets
        /// </summary>
        int Handshake
        {
            get;
        }  // data[17]

        /// <summary>
        ///Gets
        /// </summary>
        int Status
        {
            get;
        } // data[18]

        /// <summary>
        ///Gets
        /// </summary>
        int Input1
        {
            get;
        } // data[19] - Digital input 1

        /// <summary>
        ///Gets
        /// </summary>
        int Input2
        {
            get;
        } // data[20]

        /// <summary>
        ///Gets
        /// </summary>
        int Input3
        {
            get;
        } // data[21]

        /// <summary>
        ///Gets
        /// </summary>
        int Input4
        {
            get;
        }  // data[22]

        /// <summary>
        ///Gets
        /// </summary>
        int Output1
        {
            get;
        }  // data[23]

        /// <summary>
        ///Gets
        /// </summary>
        int Output2
        {
            get;
        }  // data[24]

        /// <summary>
        ///Gets
        /// </summary>
        int Output3
        {
            get;
        }  // data[25]

        /// <summary>
        ///Gets
        /// </summary>
        int Output4
        {
            get;
        }  // data[26]

        /// <summary>
        ///Gets
        /// </summary>
        int LimitStatus1
        {
            get;
        }  // data[27]

        /// <summary>
        ///Gets
        /// </summary>
        int LimitStatus2
        {
            get;
        }  // data[28]

        /// <summary>
        ///Gets
        /// </summary>
        int LimitStatus3
        {
            get;
        } // data[29]

        /// <summary>
        ///Gets
        /// </summary>
        int LimitStatus4
        {
            get;
        } // data[30]

        /// <summary>
        ///Gets
        /// </summary>
        int WeightMemDay
        {
            get;
        } // data[31]

        /// <summary>
        ///Gets
        /// </summary>
        int WeightMemMonth
        {
            get;
        } // data[32]

        /// <summary>
        ///Gets
        /// </summary>
        int WeightMemYear
        {
            get;
        } // data[33]

        /// <summary>
        ///Gets
        /// </summary>
        int WeightMemSeqNumber
        {
            get;
        } // data[34]

        /// <summary>
        ///Gets
        /// </summary>
        int WeightMemGross
        {
            get;
        } // data[35]

        /// <summary>
        ///Gets
        /// </summary>
        int WeightMemNet
        {
            get;
        } // data[36]

        /// <summary>
        ///Gets
        /// </summary>
        int CoarseFlow
        {
            get;
        } // data[37]

        /// <summary>
        ///Gets
        /// </summary>
        int FineFlow
        {
            get;
        } // data[38]

        /// <summary>
        ///Gets
        /// </summary>
        int Ready
        {
            get;
        } // data[39]

        /// <summary>
        ///Gets
        /// </summary>
        int ReDosing
        {
            get;
        } // data[40]

        /// <summary>
        ///Gets
        /// </summary>
        int Emptying
        {
            get;
        } // data[41]

        /// <summary>
        ///Gets
        /// </summary>
        int FlowError
        {
            get;
        } // data[42]

        /// <summary>
        ///Gets
        /// </summary>
        int Alarm
        {
            get;
        } // data[43]

        /// <summary>
        ///Gets
        /// </summary>
        int AdcOverUnderload
        {
            get;
        } // data[44]

        /// <summary>
        ///Gets
        /// </summary>
        int MaxDosingTime
        {
            get;
        } // data[45]

        /// <summary>
        ///Gets
        /// </summary>
        int LegalTradeOp
        {
            get;
        } // data[46]

        /// <summary>
        ///Gets
        /// </summary>
        int ToleranceErrorPlus
        {
            get;
        } // data[47]

        /// <summary>
        ///Gets
        /// </summary>
        int ToleranceErrorMinus
        {
            get;
        } // data[48]

        /// <summary>
        ///Gets
        /// </summary>
        int StatusInput1
        {
            get;
        } // data[49]

        /// <summary>
        ///Gets
        /// </summary>
        int GeneralScaleError
        {
            get;
        } // data[50]

        /// <summary>
        ///Gets
        /// </summary>
        int FillingProcessStatus
        {
            get;
        } // data[51]

        /// <summary>
        ///Gets
        /// </summary>
        int NumberDosingResults
        {
            get;
        } // data[52]

        /// <summary>
        ///Gets
        /// </summary>
        int DosingResult
        {
            get;
        } // data[53]

        /// <summary>
        ///Gets
        /// </summary>
        int MeanValueDosingResults
        {
            get;
        } // data[54]

        /// <summary>
        ///Gets
        /// </summary>
        int StandardDeviation
        {
            get;
        } // data[55]

        /// <summary>
        ///Gets
        /// </summary>
        int TotalWeight
        {
            get;
        } // data[56]

        /// <summary>
        ///Gets
        /// </summary>
        int FineFlowCutOffPoint
        {
            get;
        } // data[57]

        /// <summary>
        ///Gets
        /// </summary>
        int CoarseFlowCutOffPoint
        {
            get;
        } // data[58]

        /// <summary>
        ///Gets
        /// </summary>
        int CurrentDosingTime
        {
            get;
        } // data[59]

        /// <summary>
        ///Gets
        /// </summary>
        int CurrentCoarseFlowTime
        {
            get;
        } // data[60]

        /// <summary>
        ///Gets
        /// </summary>
        int CurrentFineFlowTime
        {
            get;
        } // data[61]

        /// <summary>
        ///Gets
        /// </summary>
        int ParameterSetProduct
        {
            get;
        } // data[62]

        // Get-Set-properties to set the output words from 2 to 26 for the standard application.

        /// <summary>
        ///Gets or sets
        /// </summary>
        int ManualTareValue
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int LimitValue1Input
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int LimitValue1Mode
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int LimitValue1ActivationLevelLowerBandLimit
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int LimitValue1HysteresisBandHeight
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int LimitValue2Source
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int LimitValue2Mode
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int LimitValue2ActivationLevelLowerBandLimit
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int LimitValue2HysteresisBandHeight
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int LimitValue3Source
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int LimitValue3Mode
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int LimitValue3ActivationLevelLowerBandLimit
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int LimitValue3HysteresisBandHeight
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int LimitValue4Source
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int LimitValue4Mode
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int LimitValue4ActivationLevelLowerBandLimit
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int LimitValue4HysteresisBandHeight
        {
            get;
            set;
        }

        // Get-Set-properties to set the output words from 9 to 44 for the filler application.

        /// <summary>
        ///Gets or sets
        /// </summary>
        int ResidualFlowTime
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int TargetFillingWeight
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int CoarseFlowCutOffPointSet
        {
            get;
            set;
        }

        /// <summary>
        ///Sets
        /// </summary>
        int FineFlowCutOffPointSet
        {
            set;
        }

        /// <summary>
        ///Sets
        /// </summary>
        int MinimumFineFlow
        {
            set;
        }

        /// <summary>
        ///Sets
        /// </summary>
        int OptimizationOfCutOffPoints
        {
            set;
        }

        /// <summary>
        ///Sets
        /// </summary>
        int MaximumDosingTime
        {
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int StartWithFineFlow
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int CoarseLockoutTime
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int FineLockoutTime
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int TareMode
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int UpperToleranceLimit
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int LowerToleranceLimit
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int MinimumStartWeight
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int EmptyWeight
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int TareDelay
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int CoarseFlowMonitoringTime
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int CoarseFlowMonitoring
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int FineFlowMonitoring
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int FineFlowMonitoringTime
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int DelayTimeAfterFineFlow
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int ActivationTimeAfterFineFlow
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int SystematicDifference
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int DownwardsDosing
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int ValveControl
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        int EmptyingMode
        {
            get;
            set;
        }

    }
}

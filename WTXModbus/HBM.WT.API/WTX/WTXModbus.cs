// <copyright file="WTXModbus.cs" company="Hottinger Baldwin Messtechnik GmbH">
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
using HBM.WT.API.WTX.Modbus;
using System;
using System.ComponentModel;
using System.Threading;
using System.Timers;

namespace HBM.WT.API.WTX
{

    public class WtxModbus : BaseWtDevice   
    {
        private string[] _dataStr;
        private ushort[] _previousData;
        public ushort[] _data;
        private ushort[] _outputData;
        private ushort[] _dataWritten;

        private bool _isNet;
        private bool _isCalibrating;
        private bool _isRefreshed;
        private bool _compareDataChanged;
        private int _timerInterval;

        private Action<IDeviceData> _callbackObj;

        private bool _dataReceived;
        private ushort _command;
        private string _ipAddr;

        private double dPreload, dNominalLoad, multiplierMv2D;

        public System.Timers.Timer _aTimer;
        
        private INetConnection _connection;

        private IDeviceData _thisValues;

        public override event EventHandler<DataEvent> DataUpdateEvent;

        public WtxModbus(INetConnection connection, int paramTimerInterval) : base(connection)
        {
            _connection = connection;

            this._previousData = new ushort[59];
            this._dataStr = new string[59];
            this._data = new ushort[59];
            this._outputData = new ushort[43]; // Output data length for filler application, also used for the standard application.
            this._dataWritten = new ushort[2];

            for (int i = 0; i < 59; i++)
            {
                _dataStr[i] = "0";
                _data[i] = 0;
                this._previousData[i] = 0;
            }

            for (int i = 0; i < 43; i++)
            {
                this._outputData[i] = 0;
            }

            this._compareDataChanged = false;
            this._isCalibrating = false;
            this._isRefreshed = false;
            this._isNet = false;
            this._dataReceived = false;

            this._timerInterval = 0;

            this.dPreload = 0;
            this.dNominalLoad = 0;
            this.multiplierMv2D = 500000;

            // For the connection and initializing of the timer: 

            this._connection.RaiseDataEvent += this.UpdateEvent;   // Subscribe to the event.

            this.initialize_timer(paramTimerInterval);
        }

        // To establish a connection to the WTX device via class WTX120_Modbus.
        public override void Connect(Action<bool> ConnectCompleted, double timeoutMs)
        {
            this._connection.Connect();
        }


        // To establish a connection to the WTX device via class WTX120_Modbus.
        public override void Connect()
        {
            this._connection.Connect();

            //this.UpdateEvent(this, null);
        }

        public override bool isConnected
        {
            get
            {
                return _connection.IsConnected;
            }
        }

        // To terminate,break, a connection to the WTX device via class WTX120_Modbus.
        public override void Disconnect(Action<bool> DisconnectCompleted)
        {
            this._connection.Disconnect();
        }

        public int getCommand
        {
            get { return this._command; }
        }

        public void Async_Call(/*ushort wordNumberParam, */ushort commandParam, Action<IDeviceData> callbackParam)
        {
            this._dataReceived = false;
            BackgroundWorker bgWorker = new BackgroundWorker();   // At the class level, create an instance of the BackgroundWorker class.

            //this.wordNumber = wordNumberParam;
            this._command = commandParam;
            this._callbackObj = callbackParam;

            bgWorker.WorkerSupportsCancellation = true;  // Specify whether you want the background operation to allow cancellation and to report progress.
            bgWorker.WorkerReportsProgress = true;

            if (this._command == 0x00)       // command=0x00 , read data from register 
            {
                bgWorker.DoWork += new DoWorkEventHandler(this.ReadDoWork);  // To set up for a background operation, an event handler, "DoWorkEventHandler" is added.
                bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.ReadCompleted);  // Create an event handler for the RunWorkerCompleted event (method "Read_Completed"). 
            }
            else  // else , write command into register 
            {
                bgWorker.DoWork += new DoWorkEventHandler(this.WriteDoWork);
                bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.WriteCompleted);
            }

            bgWorker.WorkerReportsProgress = true;
            bgWorker.RunWorkerAsync();
        }


        // This method writes a data word to the WTX120 device synchronously. 
        public void SyncCall(ushort wordNumber, ushort commandParam, Action<IDeviceData> callbackParam)      // Callback-Methode nicht benötigt. 
        {
            this._dataReceived = false;
            this._command = commandParam;
            this._callbackObj = callbackParam;

            if (this._command == 0x00)
                this._connection.Read(0);

            else
            {
                // (1) Sending of a command:        
                this._connection.Write(wordNumber, this._command);

                // Handshake protocol as given in the manual:                            
                              
                do
                {
                    this._connection.Read(0);
                } while (this.Handshake == 0);
                             
                // (2) If the handshake bit is equal to 0, the command has to be set to 0x00.
                if (this.Handshake == 1)
                {
                    this._connection.Write(wordNumber, 0x00);  
                }
                                
                while ( this.Handshake == 1) // Before : 'this.status == 1' additionally in the while condition. 
                {
                    Thread.Sleep(100);
                    this.
                    _connection.Read(0);
                }   
                
            }
        }

        // This method is executed asynchronously in the background for reading the register by a Backgroundworker. 
        // @param : sender - the object of this class. dowork_asynchronous - the argument of the event. 
        public void ReadDoWork(object sender, DoWorkEventArgs doworkAsynchronous)
        {
            this._dataReceived = false;
            doworkAsynchronous.Result = (IDeviceData)this.AsyncReadData((BackgroundWorker)sender); // the private method "this.read_data" in called to read the register in class Modbus_TCP
            // dowork_asynchronous.Result contains all values defined in Interface IDevice_Values.
        }

        // This method read the register of the Device(here: WTX120), therefore it calls the method in class Modbus_TCP to read the register. 
        // @return: IDevice_Values - Interface, that contains all values for the device. 
        public IDeviceData AsyncReadData(BackgroundWorker worker)
        {
            this._connection.Read(0);

            return this;
        }

        // Neu : 8.3.2018
        public IDeviceData SyncReadData()
        {
            this._connection.Read(0);
            //this.JetConnObj.Read();

            return this;
        }

        public override IDeviceData DeviceValues
        {
            get
            {
                return _thisValues;
            }
        }

        public void ReadCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this._callbackObj((IDeviceData)e.Result);         // Interface commited via callback. 

            // For synchronous check that data is received:
            _dataReceived = true;
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

        public void WriteDoWork(object sender, DoWorkEventArgs e)
        {
            // (1) Sending of a command:        
            
            this._connection.Write(0, this._command);
            this._connection.Read(0);

            while (this.Handshake == 0)
            {
                this._connection.Read(0);
            }

            // (2) If the handshake bit is equal to 1, the command has to be set to 0x00:
            if (this.Handshake == 1)
            {
                this._connection.Write(0, 0x00);
            }

            // (3) Wait until the handshake bit is reset to 0x00: 
            while (/*this.status == 1 && */this.Handshake == 1)
            {
                this._connection.Read(0);
            }
        }

        public void WriteCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this._callbackObj(this);         // Neu : 21.11.2017         Interface übergeben. 
        }

        public void WriteOutputWordS32(int valueParam, ushort wordNumber, Action<IDeviceData> callbackParam)
        {
            this._callbackObj = callbackParam;

            _dataWritten[0] = (ushort)((valueParam & 0xffff0000) >> 16);
            _dataWritten[1] = (ushort)(valueParam & 0x0000ffff);

            this._connection.WriteArray(wordNumber, _dataWritten);
        }

        public void WriteOutputWordU08(int valueParam, ushort wordNumber, Action<IDeviceData> callbackParam)
        {
            this._callbackObj = callbackParam;
      
            _dataWritten[0] = (ushort)((valueParam & 0x000000ff));
            this._connection.Write(wordNumber, _dataWritten[0]);
        }

        public void WriteOutputWordU16(int valueParam, ushort wordNumber, Action<IDeviceData> callbackParam)
        {
            this._callbackObj = callbackParam;

            _dataWritten[0] = (ushort)((valueParam & 0xffff0000) >> 16);

            this._connection.Write(wordNumber, _dataWritten[0]);
        }

        // This methods sets the interval value of the timer. 
        public void ResetTimer(int timerIntervalParam)
        {
            this._aTimer.Enabled = false;
            this._aTimer.Stop();

            this._timerInterval = timerIntervalParam;

            this.initialize_timer(timerIntervalParam);
        }

        // This method initializes the with the timer interval as a parameter: 
        public void initialize_timer(int paramTimerInterval)
        {
            // Create a timer with an interval of the parameter value, if the argument paramTimerInterval is not valid,
            // an exception is catched and a default value for the timer interval is set, the timer tries to start again. 
            try
            {
                _aTimer = new System.Timers.Timer(paramTimerInterval);
            }
            catch (ArgumentException)
            {
                this._timerInterval = 100;   // In case if the timer interval is not valid, an 'ArgumentException' is catched and a default value for
                                             // the timer interval is set. 
                _aTimer = new System.Timers.Timer(this._timerInterval);
            }
            // Connect the elapsed event for the timer. 
            _aTimer.Elapsed += OnTimedEvent;

            _aTimer.AutoReset = true;
            _aTimer.Enabled = true;
            _aTimer.Start();
        }

        /*
        * This method stops the timer, for example in case for the calibration.
        */
        public void StopTimer()
        {
            _aTimer.Elapsed -= OnTimedEvent;
            _aTimer.Enabled = false;
            _aTimer.Stop();
        }

        /*
         * This method restarts the timer, for example in case for the calibration.
         */
        public void RestartTimer()
        {
            _aTimer.Elapsed += OnTimedEvent;
            _aTimer.Enabled = true;
            _aTimer.Start();
        }

        // Event method, which will be triggered after a interval of the timer is elapsed- 
        // After triggering (after 500ms) the register is read. 
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Async_Call(0x00, DataReceivedTimer);

            //thisConnection.RaiseDataEvent += UpdateEvent;   // Subscribe to the event.
        }

        private void DataReceivedTimer(IDeviceData deviceValues)
        {
            _thisValues = deviceValues;

            int previousNetValue = deviceValues.NetValue;

        }

        public ushort[] GetValuesAsync()
        {
            return _data;
        }


        //public override void UpdateEvent(object sender, MessageEvent<ushort> e)
        public override void UpdateEvent(object sender, DataEvent e)
        {
            this._data = e.ushortArgs;
            this.GetDataUshort = e.ushortArgs;

            this.GetDataStr[0] = this.NetGrossValueStringComment(this.NetValue, this.Decimals);  // 1 equal to "Net measured" as a parameter
            this.GetDataStr[1] = this.NetGrossValueStringComment(this.GrossValue, this.Decimals);  // 2 equal to "Gross measured" as a parameter

            this.GetDataStr[2] = this.GeneralWeightError.ToString();
            this.GetDataStr[3] = this.ScaleAlarmTriggered.ToString();
            this.GetDataStr[4] = this.LimitStatusStringComment();
            this.GetDataStr[5] = this.WeightMovingStringComment();

            this.GetDataStr[6] = this.ScaleSealIsOpen.ToString();
            this.GetDataStr[7] = this.ManualTare.ToString();
            this.GetDataStr[8] = this.WeightTypeStringComment();
            this.GetDataStr[9] = this.ScaleRangeStringComment();

            this.GetDataStr[10] = this.ZeroRequired.ToString();
            this.GetDataStr[11] = this.WeightWithinTheCenterOfZero.ToString();
            this.GetDataStr[12] = this.WeightInZeroRange.ToString();
            this.GetDataStr[13] = this.ApplicationModeStringComment();

            this.GetDataStr[14] = this.Decimals.ToString();
            this.GetDataStr[15] = this.UnitStringComment();
            this.GetDataStr[16] = this.Handshake.ToString();
            this.GetDataStr[17] = this.StatusStringComment();

            this.GetDataStr[18] = this.Input1.ToString();
            this.GetDataStr[19] = this.Input2.ToString();
            this.GetDataStr[20] = this.Input3.ToString();
            this.GetDataStr[21] = this.Input4.ToString();

            this.GetDataStr[22] = this.Output1.ToString();
            this.GetDataStr[23] = this.Output2.ToString();
            this.GetDataStr[24] = this.Output3.ToString();
            this.GetDataStr[25] = this.Output4.ToString();

            if (this.ApplicationMode == 0)
            {
                this.GetDataStr[26] = this.LimitStatus1.ToString();
                this.GetDataStr[27] = this.LimitStatus2.ToString();
                this.GetDataStr[28] = this.LimitStatus3.ToString();
                this.GetDataStr[29] = this.LimitStatus4.ToString();

                this.GetDataStr[30] = this.WeightMemDay.ToString();
                this.GetDataStr[31] = this.WeightMemMonth.ToString();
                this.GetDataStr[32] = this.WeightMemYear.ToString();
                this.GetDataStr[33] = this.WeightMemSeqNumber.ToString();
                this.GetDataStr[34] = this.WeightMemGross.ToString();
                this.GetDataStr[35] = this.WeightMemNet.ToString();
            }
            else
                if (this.ApplicationMode == 2 || this.ApplicationMode == 1) // in filler mode 
            {
                this.GetDataStr[26] = this.CoarseFlow.ToString();
                this.GetDataStr[27] = this.FineFlow.ToString();
                this.GetDataStr[28] = this.Ready.ToString();
                this.GetDataStr[29] = this.ReDosing.ToString();

                this.GetDataStr[30] = this.Emptying.ToString();
                this.GetDataStr[31] = this.FlowError.ToString();
                this.GetDataStr[32] = this.Alarm.ToString();
                this.GetDataStr[33] = this.AdcOverUnderload.ToString();

                this.GetDataStr[34] = this.MaxDosingTime.ToString();
                this.GetDataStr[35] = this.LegalTradeOp.ToString();
                this.GetDataStr[36] = this.ToleranceErrorPlus.ToString();
                this.GetDataStr[37] = this.ToleranceErrorMinus.ToString();

                this.GetDataStr[38] = this.Status.ToString();
                this.GetDataStr[39] = this.GeneralScaleError.ToString();
                this.GetDataStr[40] = this.FillingProcessStatus.ToString();
                this.GetDataStr[41] = this.NumberDosingResults.ToString();

                this.GetDataStr[42] = this.DosingResult.ToString();
                this.GetDataStr[43] = this.MeanValueDosingResults.ToString();
                this.GetDataStr[44] = this.StandardDeviation.ToString();
                this.GetDataStr[45] = this.TotalWeight.ToString();

                this.GetDataStr[46] = this.FineFlowCutOffPoint.ToString();
                this.GetDataStr[47] = this.CoarseFlowCutOffPoint.ToString();
                this.GetDataStr[48] = this.CurrentDosingTime.ToString();
                this.GetDataStr[49] = this.CurrentCoarseFlowTime.ToString();

                this.GetDataStr[50] = this.CurrentFineFlowTime.ToString();
                this.GetDataStr[51] = this.ParameterSetProduct.ToString();

                this.GetDataStr[52] = this.FillerWeightMemoryDay.ToString();
                this.GetDataStr[53] = this.FillerWeightMemoryMonth.ToString();
                this.GetDataStr[54] = this.FillerWeightMemoryYear.ToString();
                this.GetDataStr[55] = this.FillerWeightMemorySeqNumber.ToString();
                this.GetDataStr[56] = this.FillerWeightMemoryGross.ToString();
                this.GetDataStr[57] = this.FillerWeightMemoryNet.ToString();
            }

            _compareDataChanged = false;

            e.ushortArgs = this._data;

            for (int index = 0; index < 6; index++)
            {
                if (this._previousData[index] != this._data[index])
                    _compareDataChanged = true;
            }

            // If one value of the data changes, the boolean value 'compareDataChanged' will be set to true and the data will be 
            // updated in the following, as well as the GUI form. ('compareDataChanged' is for the purpose of comparision.)

            // The data is only invoked by the event 'DataUpdateEvent' if the data has been changed. The comparision is made by...
            // ... the arrays 'previousData' and 'data' with the boolean 

            if ((this._compareDataChanged == true) || (this._isCalibrating == true) || this._isRefreshed == true)   // 'isCalibrating' indicates if a calibration is done just before ...
            {                                                                                                    // and the data should be send to the GUI/console and be printed out. 
                                                                                                                 // If the GUI has been refreshed, the values should also be send to the GUI/Console and be printed out. 
                DataUpdateEvent?.Invoke(this, e);

                this._isCalibrating = false;
                this.Refreshed = false;
            }

            this._previousData = this._data;

            // As an alternative to 'DataUpdateEvent?.Invoke(this, e);' : Both implementations do the same.  
            /*
            EventHandler<NetConnectionEventArgs<ushort[]>> handler2 = DataUpdateEvent;        

            if (handler2 != null)
                handler2(this, e);
            */
        }

        public bool Refreshed
        {
            get { return this._isRefreshed; }
            set { this._isRefreshed = value; }
        }

        public bool DataChanged
        {
            get { return this._compareDataChanged; }
            set { this._compareDataChanged = value; }
        }


        // The following methods set the specific, single values from the whole array "data".


        public override int NetValue
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 1)
                        return (_data[1] + (_data[0] << 16));
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }

        public override int GrossValue
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 3)
                        return (_data[3] + (_data[2] << 16));
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int GeneralWeightError
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 4)
                        return (_data[4] & 0x1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int ScaleAlarmTriggered
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 4)
                        return ((_data[4] & 0x2) >> 1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int LimitStatus
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 4)
                        return ((_data[4] & 0xC) >> 2);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int WeightMoving
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 4)
                        return ((_data[4] & 0x10) >> 4);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int ScaleSealIsOpen
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 4)
                        return ((_data[4] & 0x20) >> 5);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int ManualTare
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 4)
                        return ((_data[4] & 0x40) >> 6);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int WeightType
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 4)
                        return ((_data[4] & 0x80) >> 7);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int ScaleRange
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 4)
                        return ((_data[4] & 0x300) >> 8);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int ZeroRequired
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 4)
                        return ((_data[4] & 0x400) >> 10);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int WeightWithinTheCenterOfZero
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 4)
                        return ((_data[4] & 0x800) >> 11);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int WeightInZeroRange
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 4)
                        return ((_data[4] & 0x1000) >> 12);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int ApplicationMode
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 5)
                        return (_data[5] & 0x3>>1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int Decimals
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 5)
                        return ((_data[5] & 0x70) >> 4);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int Unit
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 5)
                        return ((_data[5] & 0x180) >> 7);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int Handshake
        {
            get
            {
                try
                {
                    //if (this._connection.NumofPoints > 5)
                    return ((_data[5] & 0x4000) >> 14);
                    //else
                    //    return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }

            }
        }
        public override int Status
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 5)
                        return ((_data[5] & 0x8000) >> 15);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }

        public override ushort[] GetDataUshort
        {
            get
            {
                return this._data;
            }
            set
            {
                this._data = value;
            }
        }

        public override string[] GetDataStr
        {
            get
            {
                return this._dataStr;
            }
            set
            {
                this._dataStr = value;
            }
        }

        public override int Input1
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 6)
                        return (_data[6] & 0x1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }


        }
        public override int Input2
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 6)
                        return ((_data[6] & 0x2) >> 1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int Input3
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 6)
                        return ((_data[6] & 0x4) >> 2);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int Input4
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 6)
                        return ((_data[6] & 0x8) >> 3);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int Output1
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 7)
                        return (_data[7] & 0x1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int Output2
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 7)
                        return ((_data[7] & 0x2) >> 1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int Output3
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 7)
                        return ((_data[7] & 0x4) >> 2);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int Output4
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 7)
                        return ((_data[7] & 0x8) >> 3);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int LimitStatus1
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 8)
                        return (_data[8] & 0x1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int LimitStatus2
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 8)
                        return ((_data[8] & 0x2) >> 1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int LimitStatus3
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 8)
                        return ((_data[8] & 0x4) >> 2);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int LimitStatus4
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 8)
                        return ((_data[8] & 0x8) >> 3);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int WeightMemDay
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 9)
                        return (_data[9]);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int WeightMemMonth
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 10)
                        return (_data[10]);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int WeightMemYear
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 11)
                        return (_data[11]);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int WeightMemSeqNumber
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 12)
                        return (_data[12]);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int WeightMemGross
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 13)
                        return (_data[13]);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int WeightMemNet
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 14)
                        return (_data[14]);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int CoarseFlow
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 8)
                        return (_data[8] & 0x1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int FineFlow
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 8)
                        return ((_data[8] & 0x2) >> 1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int Ready
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 8)
                        return ((_data[8] & 0x4) >> 2);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int ReDosing
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 8)
                        return ((_data[8] & 0x8) >> 3);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int Emptying
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 8)
                        return ((_data[8] & 0x10) >> 4);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int FlowError
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 8)
                        return ((_data[8] & 0x20) >> 5);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int Alarm
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 8)
                        return ((_data[8] & 0x40) >> 6);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int AdcOverUnderload
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 8)
                        return ((_data[8] & 0x80) >> 7);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }


        public override int MaxDosingTime
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 8)
                        return ((_data[8] & 0x100) >> 8);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int LegalTradeOp
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 8)
                        return ((_data[8] & 0x200) >> 9);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int ToleranceErrorPlus
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 8)
                        return ((_data[8] & 0x400) >> 10);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int ToleranceErrorMinus
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 8)
                        return ((_data[8] & 0x800) >> 11);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int StatusInput1
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 8)
                        return ((_data[8] & 0x4000) >> 14);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int GeneralScaleError
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 8)
                        return ((_data[8] & 0x8000) >> 15);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int FillingProcessStatus
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 9)
                        return _data[9];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int NumberDosingResults
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 11)
                        return _data[11];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int DosingResult
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 12)
                        return _data[12];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int MeanValueDosingResults
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 14)
                        return _data[14];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int StandardDeviation
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 16)
                        return _data[16];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int TotalWeight
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 18)
                        return _data[18];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int FineFlowCutOffPoint
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 20)
                        return _data[20];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int CoarseFlowCutOffPoint
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 22)
                        return _data[22];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int CurrentDosingTime
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 24)
                        return _data[24];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int CurrentCoarseFlowTime
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 25)
                        return _data[25];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int CurrentFineFlowTime
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 26)
                        return _data[26];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int ParameterSetProduct
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 27)
                        return _data[27];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }


        public int FillerWeightMemoryDay
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 28)
                        return _data[28];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
            set
            {
                this._data[62] = (ushort)value;
            }
        }
        public int FillerWeightMemoryMonth
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 29)
                        return _data[29];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
            set
            {
                this._data[63] = (ushort)value;
            }
        }
        public int FillerWeightMemoryYear
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 30)
                        return _data[30];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
            set
            {
                this._data[64] = (ushort)value;
            }

        }
        public int FillerWeightMemorySeqNumber
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 31)
                        return _data[31];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
            set
            {
                this._data[65] = (ushort)value;
            }
        }
        public int FillerWeightMemoryGross
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 32)
                        return _data[32];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
            set
            {
                this._data[66] = (ushort)value;
            }
        }
        public int FillerWeightMemoryNet
        {
            get
            {
                try
                {
                    if (this._connection.NumofPoints > 33)
                        return _data[33];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
            set
            {
                this._data[67] = (ushort)value;
            }
        }

        // Get and Set-Properties of the output words, for the standard and filler application. (To be continued on 04-06-2018 for all the rest...)

        public override int ManualTareValue
        {
            get
            {
                return this._outputData[0];
            }
            set
            {
                this._outputData[0] = (ushort)value;
            }
        }

        public override int LimitValue1Input
        {
            get
            {
                return this._outputData[1];
            }
            set
            {
                this._outputData[1] = (ushort)value;
            }
        }

        public override int LimitValue1Mode
        {
            get
            {
                return this._outputData[2];
            }
            set
            {
                this._outputData[2] = (ushort)value;
            }
        }

        public override int LimitValue1ActivationLevelLowerBandLimit
        {
            get
            {
                return this._outputData[3];
            }
            set
            {
                this._outputData[3] = (ushort)value;
            }
        }

        public override int LimitValue1HysteresisBandHeight
        {
            get
            {
                return this._outputData[4];
            }
            set
            {
                this._outputData[4] = (ushort)value;
            }
        }



        public override int LimitValue2Source
        {
            get
            {
                return this._outputData[5];
            }
            set
            {
                this._outputData[5] = (ushort)value;
            }
        }

        public override int LimitValue2Mode
        {
            get
            {
                return this._outputData[6];
            }
            set
            {
                this._outputData[6] = (ushort)value;
            }
        }

        public override int LimitValue2ActivationLevelLowerBandLimit
        {
            get
            {
                return this._outputData[7];
            }
            set
            {
                this._outputData[7] = (ushort)value;
            }
        }

        public override int LimitValue2HysteresisBandHeight
        {
            get
            {
                return this._outputData[8];
            }
            set
            {
                this._outputData[8] = (ushort)value;
            }
        }



        public override int LimitValue3Source
        {
            get
            {
                return this._outputData[9];
            }
            set
            {
                this._outputData[9] = (ushort)value;
            }
        }

        public override int LimitValue3Mode
        {
            get
            {
                return this._outputData[10];
            }
            set
            {
                this._outputData[10] = (ushort)value;
            }
        }

        public override int LimitValue3ActivationLevelLowerBandLimit
        {
            get
            {
                return this._outputData[11];
            }
            set
            {
                this._outputData[11] = (ushort)value;
            }
        }

        public override int LimitValue3HysteresisBandHeight
        {
            get
            {
                return this._outputData[12];
            }
            set
            {
                this._outputData[12] = (ushort)value;
            }
        }


        public override int LimitValue4Source
        {
            get
            {
                return this._outputData[13];
            }
            set
            {
                this._outputData[13] = (ushort)value;
            }
        }

        public override int LimitValue4Mode
        {
            get
            {
                return this._outputData[14];
            }
            set
            {
                this._outputData[14] = (ushort)value;
            }
        }

        public override int LimitValue4ActivationLevelLowerBandLimit
        {
            get
            {
                return this._outputData[15];
            }
            set
            {
                this._outputData[15] = (ushort)value;
            }
        }

        public override int LimitValue4HysteresisBandHeight
        {
            get
            {
                return this._outputData[16];
            }
            set
            {
                this._outputData[16] = (ushort)value;
            }
        }


        public override int ResidualFlowTime
        {
            get { return this._outputData[17]; }
            set { this._outputData[17] = (ushort)value; }
        }

        public override int TargetFillingWeight
        {
            get { return this._outputData[18]; }
            set { this._outputData[18] = (ushort)value; }
        }

        public override int CoarseFlowCutOffPointSet
        {
            get { return this._outputData[19]; }
            set { this._outputData[19] = (ushort)value; }
        }

        public override int FineFlowCutOffPointSet
        {
            get { return this._outputData[20]; }
            set { this._outputData[20] = (ushort)value; }
        }
        public override int MinimumFineFlow
        {
            get { return this._outputData[21]; }
            set { this._outputData[21] = (ushort)value; }
        }

        public override int OptimizationOfCutOffPoints
        {
            get { return this._outputData[22]; }
            set { this._outputData[22] = (ushort)value; }
        }
        public override int MaximumDosingTime
        {
            get { return this._outputData[23]; }
            set { this._outputData[23] = (ushort)value; }
        }
        public override int StartWithFineFlow
        {
            get { return this._outputData[24]; }
            set { this._outputData[24] = (ushort)value; }
        }
        public override int CoarseLockoutTime
        {
            get { return this._outputData[25]; }
            set { this._outputData[25] = (ushort)value; }
        }
        public override int FineLockoutTime
        {
            get { return this._outputData[26]; }
            set { this._outputData[26] = (ushort)value; }
        }
        public override int TareMode
        {
            get { return this._outputData[27]; }
            set { this._outputData[27] = (ushort)value; }
        }
        public override int UpperToleranceLimit
        {
            get { return this._outputData[28]; }
            set { this._outputData[28] = (ushort)value; }
        }
        public override int LowerToleranceLimit
        {
            get { return this._outputData[29]; }
            set { this._outputData[29] = (ushort)value; }
        }
        public override int MinimumStartWeight
        {
            get { return this._outputData[30]; }
            set { this._outputData[30] = (ushort)value; }
        }
        public override int EmptyWeight
        {
            get { return this._outputData[31]; }
            set { this._outputData[31] = (ushort)value; }
        }
        public override int TareDelay
        {
            get { return this._outputData[32]; }
            set { this._outputData[32] = (ushort)value; }
        }
        public override int CoarseFlowMonitoringTime
        {
            get { return this._outputData[33]; }
            set { this._outputData[33] = (ushort)value; }
        }
        public override int CoarseFlowMonitoring
        {
            get { return this._outputData[34]; }
            set { this._outputData[34] = (ushort)value; }
        }
        public override int FineFlowMonitoring
        {
            get { return this._outputData[35]; }
            set { this._outputData[35] = (ushort)value; }
        }
        public override int FineFlowMonitoringTime
        {
            get { return this._outputData[36]; }
            set { this._outputData[36] = (ushort)value; }
        }
        public override int DelayTimeAfterFineFlow
        {
            get { return this._outputData[37]; }
            set { this._outputData[37] = (ushort)value; }
        }
        public override int ActivationTimeAfterFineFlow
        {
            get { return this._outputData[38]; }
            set { this._outputData[38] = (ushort)value; }
        }
        public override int SystematicDifference
        {
            get { return this._outputData[39]; }
            set { this._outputData[39] = (ushort)value; }
        }
        public override int DownwardsDosing
        {
            get { return this._outputData[40]; }
            set { this._outputData[40] = (ushort)value; }
        }
        public override int ValveControl
        {
            get { return this._outputData[41]; }
            set { this._outputData[41] = (ushort)value; }
        }
        public override int EmptyingMode
        {
            get { return this._outputData[42]; }
            set { this._outputData[42] = (ushort)value; }
        }



        public bool GetIsNet
        {
            get
            {
                return this._isNet;
            }
        }

        /* In den folgenden Comment-Methoden werden jeweils verschiedene Auswahloptionen mit Fallunterscheidungen
        * betrachtet und je nach Fall eine unterschiedliche Option ausgewählt.
        */

        // In the following methods the different options for the single integer values are used to define and
        // interpret the value. Finally a string should be returned from the methods to write it onto the GUI Form. 

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

        public string WeightMovingStringComment()
        {
            if (this.WeightMoving == 0)
                return "0=Weight is not moving.";
            else
                return "1=Weight is moving";
        }
        public string LimitStatusStringComment()
        {
            switch (this.LimitStatus)
            {
                case 0:
                    return "Weight within limits";
                case 1:
                    return "Lower than minimum";
                case 2:
                    return "Higher than maximum capacity";
                case 3:
                    return "Higher than safe load limit";
                default: 
                    return "Lower than minimum";
            }
        }
        public string WeightTypeStringComment()
        {
            if (this.WeightType == 0)
            {
                this._isNet = false;
                return "gross";
            }
            else // if (this.WeightType == 1)
            {
                this._isNet = true;
                return "net";
            }
        }
        public string ScaleRangeStringComment()
        {
            switch (this.ScaleRange)
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
        public string ApplicationModeStringComment()
        {
            if (this.ApplicationMode == 0)
                return "Standard";
            else

                if (this.ApplicationMode == 2 || this.ApplicationMode == 1)  // Will be changed to '2', so far '1'. 
                return "Filler";
            else

                return "error";
        }
        public override string UnitStringComment()
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



        // This method sets the value for the nominal weight in the WTX.
        public override void Calibrate(int calibrationValue, string calibrationWeightStr)
        {
            //write reg 46, CalibrationWeight     
            
            this.WriteOutputWordS32(calibrationValue, 46, Write_DataReceived);

            //write reg 50, 0x7FFFFFFF

            this.WriteOutputWordS32(0x7FFFFFFF, 50, Write_DataReceived);

            Console.Write(".");

            this.SyncCall(0, 0x100, Write_DataReceived);

            this.RestartTimer();

            this._isCalibrating = true;

            // Check if the values of the WTX device are equal to the calibration value. It is also checked within a certain interval if the measurement is noisy.
            if ((this.NetValue != calibrationValue || this.GrossValue != calibrationValue))
            {
                Console.Write("Wait for setting the nominal weight into the WTX.");
                this.Async_Call(0x00, DataReceivedTimer);
            }
            else
            if (this.NetValue > (calibrationValue + 10) || (this.NetValue < (calibrationValue - 10)))
            {
                Console.Write("Wait for setting the nominal weight into the WTX.");
                this.Async_Call(0x00, DataReceivedTimer);
            }
            else
            if (this.GrossValue > (calibrationValue + 10) || (this.GrossValue < (calibrationValue - 10)))
            {
                Console.Write("Wait for setting the nominal weight into the WTX.");
            }
            else
            {
                Console.Write("Calibration failed, please restart the application");
            }

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
        public override void Calculate(double preload, double capacity)
        {
            dPreload = 0;
            dNominalLoad = 0; 

            multiplierMv2D = 500000; //   2 / 1000000; // 2mV/V correspond 1 million digits (d)

            dPreload = preload * multiplierMv2D;
            dNominalLoad = dPreload + (capacity * multiplierMv2D);
                           
            this.StopTimer();

            //write reg 48, DPreload;         

            this.WriteOutputWordS32(Convert.ToInt32(dPreload), 48, Write_DataReceived);

            this.SyncCall(0, 0x80, Write_DataReceived);

            //write reg 50, DNominalLoad;          

            this.WriteOutputWordS32(Convert.ToInt32(dNominalLoad), 50, Write_DataReceived);

            this.SyncCall(0, 0x100, Write_DataReceived);

            this._isCalibrating = true;

            this.RestartTimer();

        }

        public override void MeasureZero()
        {
            this.StopTimer();

            //todo: write reg 48, 0x7FFFFFFF

            this.WriteOutputWordS32(0x7FFFFFFF, 48, Write_DataReceived);
            
            Console.Write(".");

            this.SyncCall(0, 0x80, Write_DataReceived);

            /*
            if ((this.NetValue != 0 || this.GrossValue != 0))
            {
                this.Async_Call(0x00, DataReceivedTimer);  // Wait for setting the dead load into the WTX.
            }
            else
            if (this.NetValue > (0 + 10) || (this.NetValue < (0 - 10)))
            {
                this.Async_Call(0x00, DataReceivedTimer);   // Wait for setting the dead load into the WTX.
            }
            else
              if (this.GrossValue > (0 + 10) || (this.GrossValue < (0 - 10)))
            {
                this.Async_Call(0x00, DataReceivedTimer);    // Wait for setting the dead load into the WTX.
            }
            */
        }

        public bool Calibrating
        {
            get { return this._isCalibrating; }
            set { this._isCalibrating = value; }
        }

        public override void gross(Action<IDeviceData> WriteDataCompleted)
        {
            this.Async_Call(0x2, WriteDataCompleted);
        }
        public override void taring(Action<IDeviceData> WriteDataCompleted)
        {
            this.Async_Call(0x1, WriteDataCompleted);
        }
        public override void zeroing(Action<IDeviceData> WriteDataCompleted)
        {
            this.Async_Call(0x40, WriteDataCompleted);
        }

        public void adjustZero(Action<IDeviceData> WriteDataCompleted)
        {
            this.Async_Call(0x80, Write_DataReceived);
        }

        public void adjustNominal(Action<IDeviceData> WriteDataCompleted)
        {
            this.Async_Call(0x100, Write_DataReceived);
        }

        public void activateData(Action<IDeviceData> WriteDataCompleted)
        {
            this.Async_Call(0x800, Write_DataReceived);  // Set Bit .11 for 'Activate data'. For example, after your input for the limit values.
        }

        public void manualTaring(Action<IDeviceData> WriteDataCompleted)
        {
            this.Async_Call(0x1000, Write_DataReceived);
        }

        public void clearDosingResults(Action<IDeviceData> WriteDataCompleted)
        {
            this.Async_Call(0x4, Write_DataReceived);
        }

        public void abortDosing(Action<IDeviceData> WriteDataCompleted)
        {
            this.Async_Call(0x8, Write_DataReceived);
        }

        public void startDosing(Action<IDeviceData> WriteDataCompleted)
        {
            this.Async_Call(0x10, Write_DataReceived);
        }

        public void recordWeight(Action<IDeviceData> WriteDataCompleted)
        {
            this.Async_Call(0x4000,Write_DataReceived);
        }

        public void manualReDosing(Action<IDeviceData> WriteDataCompleted)
        {
            this.Async_Call(0x8000, Write_DataReceived);
        }
    }
}

